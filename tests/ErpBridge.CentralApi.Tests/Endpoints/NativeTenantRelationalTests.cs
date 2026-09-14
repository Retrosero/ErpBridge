using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// A company without an ERP (Faz 33). Phones create product and customer cards,
/// and the central API books sales and collections itself, so every device sees
/// the same stock and balances through <c>/api/v1/android/sync/pull</c>.
/// Relational because booking, projection and the cursor share one transaction.
/// </summary>
public sealed class NativeTenantRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public NativeTenantRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Cards_created_on_a_phone_reach_every_device_as_products_and_customers()
    {
        var t = await NativeTenantAsync();

        (await PostAsync(t.AdminToken, t, "stock_card", "CARD-S1", new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 150.5, vatRate = 1, unit = "Paket", openingQuantity = 40 }))
            .Status.Should().Be("Succeeded");
        (await PostAsync(t.SalesToken, t, "customer_card", "CARD-C1", new { customerCode = "C-001", title = "Bakkal Ali", phone = "05320000000" }))
            .Status.Should().Be("Succeeded");

        var feed = await PullAllAsync(t.SalesToken, t);
        var product = feed.Single(c => c.Entity == "urun" && c.Key == "CAY-1").Data;
        product.GetProperty("name").GetString().Should().Be("Çay 1 kg");
        product.GetProperty("stok").GetInt32().Should().Be(40);
        product.GetProperty("satis_fiyati").GetDecimal().Should().Be(150.5m);
        product.GetProperty("barkod").GetString().Should().Be("8690000000011");
        var customer = feed.Single(c => c.Entity == "cari" && c.Key == "C-001").Data;
        customer.GetProperty("unvan").GetString().Should().Be("Bakkal Ali");
        customer.GetProperty("bakiye").GetDecimal().Should().Be(0m);
    }

    [Fact]
    public async Task A_sale_lowers_stock_and_raises_the_balance_and_a_collection_lowers_it_again()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);
        var cursor = await CursorAtEndAsync(t);

        var sale = await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-1", Sale("MOB-SO-1", customerCode: "C-001", quantity: 3, unitPrice: 150, paymentType: "Cari Borç"));
        sale.Status.Should().Be("Succeeded");

        var afterSale = await PullAllAsync(t.SalesToken, t, cursor);
        afterSale.Single(c => c.Entity == "urun" && c.Key == "CAY-1").Data.GetProperty("stok").GetInt32().Should().Be(37);
        afterSale.Single(c => c.Entity == "cari" && c.Key == "C-001").Data.GetProperty("bakiye").GetDecimal().Should().Be(450m);
        var movement = afterSale.Single(c => c.Entity == "cariHareketleri").Data;
        movement.GetProperty("type").GetString().Should().Be("Satış");
        movement.GetProperty("meblag").GetDecimal().Should().Be(450m);
        afterSale.Single(c => c.Entity == "stokHareketleri").Data.GetProperty("cikisMiktar").GetDecimal().Should().Be(3m);

        (await PostAsync(t.SalesToken, t, "collection", "TAH-1", new { mobileDocumentId = "TAH-1", counterparty = "Bakkal Ali", customerCode = "C-001", amount = 200, paymentType = "Nakit" }))
            .Status.Should().Be("Succeeded");

        (await BalanceAsync(t.Id, "C-001")).Should().Be(250m);
        (await StockAsync(t.Id, "CAY-1")).Should().Be(37m);
    }

    [Fact]
    public async Task A_sale_paid_on_the_spot_leaves_no_open_balance_but_shows_both_movements()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);
        var cursor = await CursorAtEndAsync(t);

        (await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-CASH", Sale("MOB-SO-CASH", "C-001", quantity: 2, unitPrice: 100, paymentType: "Nakit")))
            .Status.Should().Be("Succeeded");

        (await BalanceAsync(t.Id, "C-001")).Should().Be(0m);
        var movements = (await PullAllAsync(t.SalesToken, t, cursor)).Where(c => c.Entity == "cariHareketleri").ToList();
        movements.Select(m => m.Data.GetProperty("type").GetString()).Should().BeEquivalentTo("Satış", "Tahsilat");
    }

    [Fact]
    public async Task Sending_the_same_document_again_books_it_once()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);
        var body = Sale("MOB-SO-RETRY", "C-001", quantity: 5, unitPrice: 10, paymentType: "Cari Borç");

        await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-RETRY", body);
        var again = await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-RETRY", body);

        again.Idempotent.Should().BeTrue();
        (await StockAsync(t.Id, "CAY-1")).Should().Be(35m);
        (await BalanceAsync(t.Id, "C-001")).Should().Be(50m);
    }

    [Fact]
    public async Task Offline_sales_are_never_refused_for_stock_and_older_apps_may_name_the_customer_by_title()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t); // 40 in stock

        var sale = await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-BIG", Sale("MOB-SO-BIG", customerCode: null, quantity: 50, unitPrice: 1, paymentType: null));

        sale.Status.Should().Be("Succeeded");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(-10m);
        (await BalanceAsync(t.Id, "C-001")).Should().Be(50m);
    }

    [Fact]
    public async Task A_document_that_cannot_be_booked_is_kept_as_failed_and_changes_nothing()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);

        var unknownCustomer = await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-X",
            new { mobileDocumentId = "MOB-SO-X", counterparty = "Olmayan Market", amount = 10, lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 10 } } });
        var fieldUserProduct = await PostAsync(t.SalesToken, t, "stock_card", "CARD-S2", new { stockCode = "KAHVE", name = "Kahve" });

        unknownCustomer.Status.Should().Be("Failed");
        fieldUserProduct.Status.Should().Be("Failed", "only company administrators create products");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(40m);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.Jobs.SingleAsync(j => j.TenantId == t.Id && j.ExternalId == "MOB-SO-X")).LastError.Should().Contain("customer");
        (await db.NativeStockLevels.AnyAsync(l => l.TenantId == t.Id && l.StockCode == "KAHVE")).Should().BeFalse();
    }

    [Fact]
    public async Task A_sale_with_one_bad_line_moves_no_stock_and_no_balance_at_all()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);

        var sale = await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-HALF", new
        {
            mobileDocumentId = "MOB-SO-HALF",
            customerCode = "C-001",
            amount = 30,
            lines = new object[]
            {
                new { productCode = "CAY-1", quantity = 2, unitPrice = 10 },
                new { barcode = "0000000000000", quantity = 1, unitPrice = 10 },
            },
        });

        sale.Status.Should().Be("Failed");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(40m);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.NativeCustomerBalances.SingleAsync(b => b.TenantId == t.Id && b.CustomerCode == "C-001")).Balance.Should().Be(0m);
    }

    [Fact]
    public async Task A_sale_of_an_unknown_product_or_with_a_negative_total_is_refused()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);

        var phantom = await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-PHANTOM", new
        {
            mobileDocumentId = "MOB-SO-PHANTOM", customerCode = "C-001", amount = 10,
            lines = new[] { new { productCode = "YOK-BOYLE-URUN", quantity = 1, unitPrice = 10 } },
        });
        var negative = await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-NEG", Sale("MOB-SO-NEG", "C-001", quantity: 1, unitPrice: -100, paymentType: "Cari Borç"));

        phantom.Status.Should().Be("Failed");
        negative.Status.Should().Be("Failed");
        (await BalanceAsync(t.Id, "C-001")).Should().Be(0m);
        (await StockAsync(t.Id, "CAY-1")).Should().Be(40m);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.NativeStockLevels.AnyAsync(l => l.TenantId == t.Id && l.StockCode == "YOK-BOYLE-URUN")).Should().BeFalse();
    }

    [Fact]
    public async Task An_erp_agent_cannot_register_or_upload_for_a_company_without_an_erp()
    {
        var t = await NativeTenantAsync();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var licenseKey = $"NATIVE-LIC-{Guid.NewGuid():N}";
        db.Licenses.Add(new License { TenantId = t.Id, LicenseKey = licenseKey, IsActive = true, ExpiresAtUtc = DateTimeOffset.UtcNow.AddYears(1) });
        await db.SaveChangesAsync();
        var client = _factory.CreateClient();

        var register = await client.PostJsonAsync("/api/v1/agents/register", new { licenseKey, machineId = "MACHINE-X", agentVersion = "1.0.0" });
        register.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await register.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("TENANT_IS_NATIVE");

        // An agent token minted before the switch must not be able to upload either.
        var agentToken = _factory.IssueTestJwt(Guid.NewGuid(), t.Id);
        var upload = await client.PostJsonAsync("/api/v1/bootstrap/upload/start", new { sourceDatabase = "MIKRO_DB", isIncremental = false }, agentToken);
        upload.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await db.Agents.AnyAsync(a => a.TenantId == t.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task A_tenant_with_documents_waiting_for_an_agent_cannot_be_switched_to_native()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"QUEUED-{suffix}", $"Queued tenant {suffix}");
        await _factory.SeedJobAsync(tenant.Id, $"SO-QUEUED-{suffix}");
        var adminToken = await AdminTokenAsync();

        var switched = await PutAsync($"/api/v1/admin/tenants/{tenant.Id}/mobile/data-source", new { dataSource = "native" }, adminToken);

        switched.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await switched.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("TENANT_HAS_ERP_DATA");
    }

    [Fact]
    public async Task An_edited_product_card_reaches_devices_and_its_old_barcode_stops_selling()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);
        var cursor = await CursorAtEndAsync(t);

        (await PostAsync(t.AdminToken, t, "stock_card", "CARD-S1-EDIT", new { stockCode = "CAY-1", name = "Çay 1 kg Tiryaki", barcode = "8690000000099", price = 175, openingQuantity = 999 }))
            .Status.Should().Be("Succeeded");

        var product = (await PullAllAsync(t.SalesToken, t, cursor)).Single(c => c.Entity == "urun" && c.Key == "CAY-1").Data;
        product.GetProperty("name").GetString().Should().Be("Çay 1 kg Tiryaki");
        product.GetProperty("satis_fiyati").GetDecimal().Should().Be(175m);
        product.GetProperty("barkod").GetString().Should().Be("8690000000099");
        product.GetProperty("stok").GetInt32().Should().Be(40, "an edit never resets stock");

        var byOldBarcode = await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-OLDBAR", new
        {
            mobileDocumentId = "MOB-SO-OLDBAR", customerCode = "C-001", amount = 10,
            lines = new[] { new { barcode = "8690000000011", quantity = 1, unitPrice = 10 } },
        });
        byOldBarcode.Status.Should().Be("Failed");
    }

    [Fact]
    public async Task A_product_without_movements_is_deleted_on_every_device()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);
        var cursor = await CursorAtEndAsync(t);

        (await PostAsync(t.AdminToken, t, "stock_card_delete", "DEL-S1", new { stockCode = "CAY-1" })).Status.Should().Be("Succeeded");

        var change = (await PullAllAsync(t.SalesToken, t, cursor)).Single(c => c.Entity == "urun" && c.Key == "CAY-1");
        change.Deleted.Should().BeTrue();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.NativeStockLevels.AnyAsync(l => l.TenantId == t.Id && l.StockCode == "CAY-1")).Should().BeFalse();
        (await db.MobileRecords.AnyAsync(r => r.TenantId == t.Id && r.StockKey == "CAY-1" && !r.IsDeleted)).Should().BeFalse();
    }

    [Fact]
    public async Task A_product_that_was_sold_cannot_be_deleted_and_a_field_user_cannot_delete_at_all()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);
        (await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-KEEP", Sale("MOB-SO-KEEP", "C-001", quantity: 1, unitPrice: 150, paymentType: "Cari Borç")))
            .Status.Should().Be("Succeeded");

        var byAdmin = await PostAsync(t.AdminToken, t, "stock_card_delete", "DEL-S1-SOLD", new { stockCode = "CAY-1" });
        var byFieldUser = await PostAsync(t.SalesToken, t, "stock_card_delete", "DEL-S1-SALES", new { stockCode = "CAY-1" });

        byAdmin.Status.Should().Be("Failed");
        byFieldUser.Status.Should().Be("Failed");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(39m);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.Jobs.SingleAsync(j => j.TenantId == t.Id && j.ExternalId == "DEL-S1-SOLD")).LastError.Should().Contain("movements");
        (await db.MobileRecords.AnyAsync(r => r.TenantId == t.Id && r.Entity == "stocks" && r.RecordKey == "CAY-1" && !r.IsDeleted)).Should().BeTrue();
    }

    [Fact]
    public async Task An_excel_import_batch_books_every_valid_card_and_notes_the_skipped_rows()
    {
        var t = await NativeTenantAsync();

        var products = await PostAsync(t.AdminToken, t, "stock_card_batch", "IMPORT-S-1", new
        {
            cards = new object[]
            {
                new { stockCode = "UN-1", name = "Un 5 kg", barcode = "8690000000101", price = 120, openingQuantity = 10 },
                new { stockCode = "SUT-1", name = "Süt 1 L", price = 35, openingQuantity = 24 },
                new { stockCode = "", name = "Kodsuz satır" },
            },
        });
        var customers = await PostAsync(t.SalesToken, t, "customer_card_batch", "IMPORT-C-1", new
        {
            cards = new object[] { new { customerCode = "C-101", title = "Market Bir" }, new { customerCode = "C-102", title = "Market İki", openingBalance = 250 } },
        });

        products.Status.Should().Be("Succeeded");
        customers.Status.Should().Be("Succeeded");
        var feed = await PullAllAsync(t.SalesToken, t);
        feed.Where(c => c.Entity == "urun").Select(c => c.Key).Should().BeEquivalentTo("UN-1", "SUT-1");
        feed.Single(c => c.Entity == "urun" && c.Key == "SUT-1").Data.GetProperty("stok").GetInt32().Should().Be(24);
        feed.Single(c => c.Entity == "cari" && c.Key == "C-102").Data.GetProperty("bakiye").GetDecimal().Should().Be(250m);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.Jobs.SingleAsync(j => j.TenantId == t.Id && j.ExternalId == "IMPORT-S-1")).LastError.Should().Contain("1 skipped");
    }

    [Fact]
    public async Task A_field_user_cannot_import_products()
    {
        var t = await NativeTenantAsync();

        var batch = await PostAsync(t.SalesToken, t, "stock_card_batch", "IMPORT-S-SALES", new { cards = new[] { new { stockCode = "X-1", name = "X" } } });

        batch.Status.Should().Be("Failed");
    }

    [Fact]
    public async Task A_return_on_account_puts_stock_back_and_credits_the_customer()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t); // 40 in stock
        await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-R", Sale("MOB-SO-R", "C-001", quantity: 5, unitPrice: 100, paymentType: "Cari Borç"));
        var cursor = await CursorAtEndAsync(t);

        var result = await PostAsync(t.SalesToken, t, "sales_return", "MOB-SR-1", new
        {
            mobileDocumentId = "MOB-SR-1", customerCode = "C-001", paymentType = "Cari Alacak", amount = 180,
            lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 100, lineTotal = 180, reason = "Hasarlı" } },
        });

        result.Status.Should().Be("Succeeded");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(37m);
        (await BalanceAsync(t.Id, "C-001")).Should().Be(320m);
        var feed = await PullAllAsync(t.SalesToken, t, cursor);
        var movement = feed.Single(c => c.Entity == "stokHareketleri").Data;
        movement.GetProperty("tip").GetInt32().Should().Be(2);
        movement.GetProperty("girisMiktar").GetDecimal().Should().Be(2m);
        feed.Single(c => c.Entity == "cariHareketleri").Data.GetProperty("type").GetString().Should().Be("İade");
    }

    [Fact]
    public async Task A_return_paid_back_in_cash_moves_stock_but_not_the_open_balance()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);

        (await PostAsync(t.SalesToken, t, "sales_return", "MOB-SR-CASH", new
        {
            mobileDocumentId = "MOB-SR-CASH", customerCode = "C-001", paymentType = "Nakit",
            lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } },
        })).Status.Should().Be("Succeeded");

        (await StockAsync(t.Id, "CAY-1")).Should().Be(41m);
        (await BalanceAsync(t.Id, "C-001")).Should().Be(0m);
    }

    [Fact]
    public async Task A_purchase_adds_stock_owes_the_supplier_and_makes_the_product_undeletable()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);
        (await PostAsync(t.AdminToken, t, "customer_card", "CARD-SUP", new { customerCode = "T-001", title = "Toptancı Çay A.Ş." })).Status.Should().Be("Succeeded");

        var onAccount = await PostAsync(t.AdminToken, t, "purchase_receipt", "MOB-PR-1", new
        {
            mobileDocumentId = "MOB-PR-1", invoiceNo = "ALS-0001", supplierCode = "T-001",
            lines = new[] { new { productCode = "CAY-1", quantity = 60, unitPrice = 90 } },
        });
        var paid = await PostAsync(t.AdminToken, t, "purchase_receipt", "MOB-PR-2", new
        {
            mobileDocumentId = "MOB-PR-2", supplierCode = "T-001", paymentType = "EFT / Havale",
            lines = new[] { new { productCode = "CAY-1", quantity = 10, unitPrice = 90 } },
        });

        onAccount.Status.Should().Be("Succeeded");
        paid.Status.Should().Be("Succeeded");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(110m);
        (await BalanceAsync(t.Id, "T-001")).Should().Be(-5400m, "the company owes the supplier for the purchase on account only");
        (await PostAsync(t.AdminToken, t, "stock_card_delete", "DEL-AFTER-PURCHASE", new { stockCode = "CAY-1" })).Status.Should().Be("Failed");
    }

    [Fact]
    public async Task A_return_or_purchase_with_an_unknown_product_changes_nothing()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);

        var badReturn = await PostAsync(t.SalesToken, t, "sales_return", "MOB-SR-BAD", new
        {
            customerCode = "C-001",
            lines = new object[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 10 }, new { productCode = "YOK", quantity = 1, unitPrice = 10 } },
        });
        var badPurchase = await PostAsync(t.AdminToken, t, "purchase_receipt", "MOB-PR-BAD", new
        {
            supplierCode = "C-001", lines = new[] { new { productCode = "YOK", quantity = 5, unitPrice = 1 } },
        });

        badReturn.Status.Should().Be("Failed");
        badPurchase.Status.Should().Be("Failed");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(40m);
        (await BalanceAsync(t.Id, "C-001")).Should().Be(0m);
    }

    [Fact]
    public async Task A_stock_count_applies_the_difference_it_found_and_keeps_sales_booked_after_it()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t); // 40 in stock
        (await PostAsync(t.AdminToken, t, "stock_card", "CARD-S2", new { stockCode = "SU-1", name = "Su", price = 5, openingQuantity = 100 })).Status.Should().Be("Succeeded");

        // Counted offline while 40 were expected; another phone sells 5 before the count arrives.
        await PostAsync(t.SalesToken, t, "sales_order", "MOB-SO-DURING", Sale("MOB-SO-DURING", "C-001", quantity: 5, unitPrice: 10, paymentType: "Cari Borç"));
        var cursor = await CursorAtEndAsync(t);

        var count = await PostAsync(t.SalesToken, t, "stock_count", "COUNT-1", new
        {
            mobileDocumentId = "COUNT-1", status = "COMPLETED", countedBy = "ali",
            lines = new object[]
            {
                new { productCode = "CAY-1", expectedQuantity = 40, countedQuantity = 30 },
                new { barcode = (string?)null, productCode = "SU-1", expectedQuantity = 100, countedQuantity = 100 },
            },
        });

        count.Status.Should().Be("Succeeded");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(25m, "40 - 5 sold - 10 missing");
        (await StockAsync(t.Id, "SU-1")).Should().Be(100m);
        var movements = (await PullAllAsync(t.SalesToken, t, cursor)).Where(c => c.Entity == "stokHareketleri").ToList();
        movements.Should().ContainSingle("a line without a difference writes no movement");
        movements[0].Data.GetProperty("cikisMiktar").GetDecimal().Should().Be(10m);
    }

    [Fact]
    public async Task A_stock_count_with_an_unknown_product_or_not_completed_changes_nothing()
    {
        var t = await NativeTenantAsync();
        await SeedCardsAsync(t);

        var unknown = await PostAsync(t.SalesToken, t, "stock_count", "COUNT-BAD", new
        {
            status = "COMPLETED",
            lines = new object[] { new { productCode = "CAY-1", expectedQuantity = 40, countedQuantity = 0 }, new { productCode = "YOK", expectedQuantity = 1, countedQuantity = 2 } },
        });
        var pending = await PostAsync(t.SalesToken, t, "stock_count", "COUNT-PENDING", new
        {
            status = "PENDING", lines = new[] { new { productCode = "CAY-1", expectedQuantity = 40, countedQuantity = 0 } },
        });

        unknown.Status.Should().Be("Failed");
        pending.Status.Should().Be("Failed");
        (await StockAsync(t.Id, "CAY-1")).Should().Be(40m);
    }

    [Fact]
    public async Task An_erp_tenant_keeps_documents_for_its_agent_and_refuses_cards()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ERP-{suffix}", $"ERP tenant {suffix}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, $"AK-{Guid.NewGuid():N}", scopes: ["ingest:write", "mobile:read"]);
        var client = _factory.CreateClient();

        var card = await SendAsync(client, rawKey, tenant.Id, "/api/v1/ingest/jobs", new { externalId = "CARD-1", documentType = "stock_card", payload = new { stockCode = "S", name = "S" } });
        card.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await card.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("CARDS_REQUIRE_NATIVE_TENANT");
        // Returns and purchases are booked by the central API only; an agent could not write them.
        var salesReturn = await SendAsync(client, rawKey, tenant.Id, "/api/v1/ingest/jobs", new { externalId = "SR-1", documentType = "sales_return", payload = new { ok = true } });
        salesReturn.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await salesReturn.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("DOCUMENT_REQUIRES_NATIVE_TENANT");

        // ERP data is changed in the ERP only: no product can be deleted from a phone either.
        var delete = await SendAsync(client, rawKey, tenant.Id, "/api/v1/ingest/jobs", new { externalId = "DEL-1", documentType = "stock_card_delete", payload = new { stockCode = "S" } });
        delete.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var sale = await SendAsync(client, rawKey, tenant.Id, "/api/v1/ingest/jobs", new { externalId = "SO-1", documentType = "sales_order", payload = new { ok = true } });
        (await sale.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Pending");
    }

    [Fact]
    public async Task A_tenant_with_an_erp_agent_cannot_be_switched_to_native()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"AGENT-{suffix}", $"Agent tenant {suffix}");
        await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-{suffix}");
        var adminToken = await AdminTokenAsync();

        var switched = await PutAsync($"/api/v1/admin/tenants/{tenant.Id}/mobile/data-source", new { dataSource = "native" }, adminToken);

        switched.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await switched.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("TENANT_HAS_ERP_DATA");
    }

    [Fact]
    public async Task The_phone_session_says_the_company_has_no_erp()
    {
        var t = await NativeTenantAsync();

        var me = await (await _factory.CreateClient().GetAsync("/api/v1/android/account/me", t.SalesToken)).ReadAsJsonAsync<MobileSessionDto>();

        me.DataSource.Should().Be("native");
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record NativeTenant(Guid Id, string Code, string AdminToken, string SalesToken);

    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<NativeTenant> NativeTenantAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"NATIVE-{suffix}", $"Native tenant {suffix}");
        var adminToken = await AdminTokenAsync();
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 3, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostJsonAsync($"{basePath}/users", new { username = "patron", fullName = "Patron", password = Password, role = "ADMIN" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await client.PostJsonAsync($"{basePath}/users", new { username = "ali", fullName = "Ali Saha", password = Password, role = "SALES" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.Created);
        var overview = await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();

        var patron = await LoginAsync(overview.TenantCode!, "patron", $"DEV-P-{suffix}");
        // These tests book documents directly; the approval centre has its own tests.
        var rules = ApprovalKinds.All.ToDictionary(kind => kind, _ => false);
        (await PutAsync("/api/v1/android/approvals/rules", new { rules }, patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        return new NativeTenant(tenant.Id, overview.TenantCode!, patron,
            await LoginAsync(overview.TenantCode!, "ali", $"DEV-A-{suffix}"));
    }

    private async Task SeedCardsAsync(NativeTenant t)
    {
        (await PostAsync(t.AdminToken, t, "stock_card", "CARD-S1", new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 150, openingQuantity = 40 }))
            .Status.Should().Be("Succeeded");
        (await PostAsync(t.AdminToken, t, "customer_card", "CARD-C1", new { customerCode = "C-001", title = "Bakkal Ali" }))
            .Status.Should().Be("Succeeded");
    }

    private static object Sale(string id, string? customerCode, int quantity, decimal unitPrice, string? paymentType) => new
    {
        mobileDocumentId = id,
        occurredAt = "2026-09-13T10:00:00",
        transactionType = "Satış",
        counterparty = "Bakkal Ali",
        customerCode,
        amount = quantity * unitPrice,
        paymentType,
        lines = new[] { new { barcode = "8690000000011", productCode = "CAY-1", productTitle = "Çay 1 kg", quantity, unitPrice, lineTotal = quantity * unitPrice } },
    };

    private async Task<IngestJobResponse> PostAsync(string token, NativeTenant t, string documentType, string externalId, object payload)
    {
        var response = await SendAsync(_factory.CreateClient(), token, t.Id, "/api/v1/ingest/jobs", new { externalId, documentType, payload });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        return await response.ReadAsJsonAsync<IngestJobResponse>();
    }

    private async Task<List<Change>> PullAllAsync(string token, NativeTenant t, string? cursor = null)
    {
        var changes = new List<Change>();
        var client = _factory.CreateClient();
        while (true)
        {
            var response = await SendAsync(client, token, t.Id, "/api/v1/android/sync/pull", new { cursor, limit = 500 });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = document.RootElement;
            foreach (var change in root.GetProperty("changes").EnumerateArray())
            {
                changes.Add(new Change(
                    change.GetProperty("entity").GetString()!, change.GetProperty("key").GetString()!,
                    change.GetProperty("deleted").GetBoolean(),
                    change.TryGetProperty("data", out var data) ? data.Clone() : default));
            }
            cursor = root.GetProperty("nextCursor").GetString();
            if (!root.GetProperty("hasMore").GetBoolean()) return changes;
        }
    }

    private async Task<string> CursorAtEndAsync(NativeTenant t)
    {
        var client = _factory.CreateClient();
        string? cursor = null;
        while (true)
        {
            var response = await SendAsync(client, t.SalesToken, t.Id, "/api/v1/android/sync/pull", new { cursor, limit = 500 });
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            cursor = document.RootElement.GetProperty("nextCursor").GetString();
            if (!document.RootElement.GetProperty("hasMore").GetBoolean()) return cursor!;
        }
    }

    private async Task<decimal> BalanceAsync(Guid tenantId, string customerCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.NativeCustomerBalances.SingleAsync(b => b.TenantId == tenantId && b.CustomerCode == customerCode)).Balance;
    }

    private async Task<decimal> StockAsync(Guid tenantId, string stockCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.NativeStockLevels.SingleAsync(l => l.TenantId == tenantId && l.StockCode == stockCode)).Quantity;
    }

    private async Task<string> AdminTokenAsync()
    {
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        return _factory.IssueAdminJwt(admin.Id);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.218" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, string token, Guid tenantId, string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string path, object value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
