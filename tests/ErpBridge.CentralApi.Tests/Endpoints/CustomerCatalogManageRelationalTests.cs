using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §5.1 / S4: the catalog's management endpoints — module first, then the locked permission
/// (ADMIN and MANAGER only), layout writes under the revision lock, and customer accounts with one live account per
/// customer and per username and a server-made password shown once.
/// </summary>
public sealed class CustomerCatalogManageRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public CustomerCatalogManageRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task The_module_comes_first_then_only_admins_and_managers_manage()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        foreach (var token in new[] { c.Patron, c.Ali })
            await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Base + "/settings", token), HttpStatusCode.Forbidden, "MODULE_NOT_ENABLED");

        await SetModulesAsync(_factory, c, TenantModules.CustomerCatalog);
        foreach (var (method, path) in new[] { (HttpMethod.Get, "/settings"), (HttpMethod.Get, "/categories"), (HttpMethod.Get, "/products"), (HttpMethod.Get, "/accounts"), (HttpMethod.Post, "/accounts") })
        {
            foreach (var token in new[] { c.Ali, c.Muhasebe })
                await ShouldFailAsync(await SendAsync(_factory, method, Base + path, token, method == HttpMethod.Post ? new { } : null), HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
        }
        foreach (var token in new[] { c.Patron, c.Mudur })
            (await SendAsync(_factory, HttpMethod.Get, Base + "/settings", token)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Settings_give_the_address_the_lists_and_the_counts_and_make_a_missing_company_code()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await SeedAsync(_factory, db =>
        {
            db.CatalogImages.Add(new CatalogImage { TenantId = c.Id, StockCode = "A", SourceHash = "x", SizeBytes = 1234, HasLarge = true, CreatedAtMs = now });
            // "Görsel kotası" is the company's one storage quota (GOAL_DEPOLAMA_R2): its counter, every area.
            db.TenantStorage.Add(new TenantStorage { TenantId = c.Id, UsedBytes = 5678, QuotaBytes = 2L << 30, UpdatedAtMs = now });
            db.CatalogOrders.Add(new CatalogOrder
            {
                Id = Guid.NewGuid(), TenantId = c.Id, AccountId = Guid.NewGuid(), CustomerCode = "C1", CustomerName = "Yılmaz", AccountUsername = "y",
                No = "KT-AAAAA2", SubmittedAtMs = now, UpdatedAtMs = now,
            });
        });
        // A company that never had seats has no code yet: the catalog makes one, its address needs it.
        await SeedAsync(_factory, db => db.Tenants.Single(t => t.Id == c.Id).Code = null);

        var settings = await OkAsync<CatalogSettingsDto>(await SendAsync(_factory, HttpMethod.Get, Base + "/settings", c.Mudur));

        settings.TenantCode.Should().MatchRegex("^[A-Z2-9]{8}$").And.NotBe(c.Code);
        settings.PublicUrl.Should().Be("https://sipariscepte.appsgo.cloud/" + settings.TenantCode);
        settings.IsEnabled.Should().BeFalse();
        settings.Revision.Should().Be(0);
        settings.DefaultPriceListNo.Should().BeNull();
        settings.EffectiveDefaultPriceListNo.Should().Be(1);
        settings.PriceLists.Select(l => (l.No, l.Name, l.IncludesVat)).Should().Equal((1, "Perakende", true), (2, "Bayi", false));
        settings.ImageQuota.Should().BeEquivalentTo(new CatalogImageQuotaDto { UsedBytes = 5678, LimitBytes = 2L << 30 });
        settings.Counts.Should().BeEquivalentTo(new CatalogCountsDto { Categories = 3, Products = 3, VisibleProducts = 3, Accounts = 0, OpenOrders = 1 });
        (await ReadAsync(_factory, db => db.Tenants.AsNoTracking().SingleAsync(t => t.Id == c.Id))).Code.Should().Be(settings.TenantCode);
    }

    [Fact]
    public async Task A_layout_write_needs_the_revision_it_read()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);

        var saved = await OkAsync<CatalogSettingsDto>(await SendAsync(_factory, HttpMethod.Put, Base + "/settings", c.Patron,
            new { revision = 0, isEnabled = true, defaultPriceListNo = 2 }));
        saved.Should().Match<CatalogSettingsDto>(s => s.IsEnabled && s.DefaultPriceListNo == 2 && s.EffectiveDefaultPriceListNo == 2 && s.Revision == 1);
        saved.Counts.VisibleProducts.Should().Be(1, "only A has a price in list 2");

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Base + "/settings", c.Mudur, new { revision = 0, isEnabled = false, defaultPriceListNo = (int?)null }),
            HttpStatusCode.Conflict, "CATALOG_CHANGED");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Base + "/categories", c.Mudur, new { revision = 0, items = Array.Empty<object>() }),
            HttpStatusCode.Conflict, "CATALOG_CHANGED");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Base + "/products", c.Mudur, new { revision = 0, items = Array.Empty<object>() }),
            HttpStatusCode.Conflict, "CATALOG_CHANGED");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Base + "/settings", c.Mudur, new { revision = 1, isEnabled = true, defaultPriceListNo = 7 }),
            HttpStatusCode.BadRequest, "UNKNOWN_PRICE_LIST");

        var back = await OkAsync<CatalogSettingsDto>(await SendAsync(_factory, HttpMethod.Put, Base + "/settings", c.Mudur,
            new { revision = 1, isEnabled = false, defaultPriceListNo = (int?)null }));
        back.Should().Match<CatalogSettingsDto>(s => !s.IsEnabled && s.DefaultPriceListNo == null && s.EffectiveDefaultPriceListNo == 1 && s.Revision == 2);
        var row = await ReadAsync(_factory, db => db.CatalogSettings.AsNoTracking().SingleAsync(s => s.TenantId == c.Id));
        row.Revision.Should().Be(2);
        row.UpdatedByUserId.Should().NotBeNull();
    }

    [Fact]
    public async Task Categories_take_the_order_and_flags_of_the_whole_list()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var before = await OkAsync<CatalogCategoriesResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/categories", c.Patron));
        before.Items.Select(i => i.Key).Should().Equal("Çay", "Diğer", "Kahve");
        before.Items[0].Should().BeEquivalentTo(new CatalogCategoryDto { Key = "Çay", Name = "Çay", ProductCount = 1 });

        var saved = await OkAsync<CatalogRevisionDto>(await SendAsync(_factory, HttpMethod.Put, Base + "/categories", c.Patron, new
        {
            revision = before.Revision,
            items = new object[] { new { key = "Kahve", hidden = false }, new { key = " Diğer ", hidden = true }, new { key = "Çay", hidden = false } },
        }));
        saved.Revision.Should().Be(before.Revision + 1);

        var after = await OkAsync<CatalogCategoriesResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/categories", c.Patron));
        after.Revision.Should().Be(saved.Revision);
        after.Items.Select(i => (i.Key, i.SortOrder, i.Hidden)).Should().Equal(("Kahve", 0, false), ("Diğer", 1, true), ("Çay", 2, false));
        (await OkAsync<CatalogSettingsDto>(await SendAsync(_factory, HttpMethod.Get, Base + "/settings", c.Patron))).Counts.VisibleProducts.Should().Be(2);

        // The next full list leaves "Diğer" out: it loses its place but stays hidden; a shown one left out loses its row.
        (await SendAsync(_factory, HttpMethod.Put, Base + "/categories", c.Patron, new
        {
            revision = saved.Revision,
            items = new object[] { new { key = "Çay", hidden = false } },
        })).StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await ReadAsync(_factory, db => db.CatalogCategorySettings.AsNoTracking().Where(s => s.TenantId == c.Id).ToListAsync());
        rows.Select(r => (r.CategoryKey, r.SortOrder, r.IsHidden)).Should().BeEquivalentTo(new[] { ("Çay", (int?)0, false), ("Diğer", (int?)null, true) });

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Base + "/categories", c.Patron, new
        {
            revision = saved.Revision + 1,
            items = new object[] { new { key = "Çay", hidden = false }, new { key = "Çay", hidden = true } },
        }), HttpStatusCode.BadRequest, "INVALID_BODY");
    }

    [Fact]
    public async Task Products_change_only_those_sent_and_carton_only_needs_a_carton()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Base + "/products", c.Patron, new
        {
            revision = 0,
            items = new object[] { new { stockCode = "B", sortOrder = (int?)null, hidden = false, noDiscount = false, cartonOnly = true, cartonQuantity = (int?)null } },
        }), HttpStatusCode.BadRequest, "CARTON_QUANTITY_REQUIRED");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, Base + "/products", c.Patron, new
        {
            revision = 0,
            items = new object[] { new { stockCode = "B", sortOrder = (int?)null, hidden = false, noDiscount = false, cartonOnly = false, cartonQuantity = 1 } },
        }), HttpStatusCode.BadRequest, "INVALID_CARTON_QUANTITY");

        var saved = await OkAsync<CatalogRevisionDto>(await SendAsync(_factory, HttpMethod.Put, Base + "/products", c.Patron, new
        {
            revision = 0,
            items = new object[]
            {
                new { stockCode = "a", sortOrder = (int?)3, hidden = false, noDiscount = true, cartonOnly = true, cartonQuantity = (int?)null },
                new { stockCode = "B", sortOrder = (int?)null, hidden = true, noDiscount = false, cartonOnly = true, cartonQuantity = (int?)6 },
            },
        }));
        saved.Revision.Should().Be(1);

        var cay = await OkAsync<CatalogProductsResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/products?category=" + Uri.EscapeDataString("Çay"), c.Patron));
        cay.Revision.Should().Be(1);
        cay.Truncated.Should().BeFalse();
        cay.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(new CatalogProductDto
        {
            StockCode = "A", Name = "Çay Rize", Unit = "KG", CategoryKey = "Çay", SortOrder = 3, NoDiscount = true, CartonOnly = true,
            ErpCartonQuantity = 12, ListPrice = 100m, InStock = true,
        });
        var kahve = (await OkAsync<CatalogProductsResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/products?category=Kahve", c.Patron))).Items.Single();
        kahve.Should().Match<CatalogProductDto>(p => p.Hidden && p.CartonOnly && p.CartonQuantity == 6 && p.ErpCartonQuantity == null && !p.InStock);
        (await ReadAsync(_factory, db => db.CatalogProductSettings.AsNoTracking().SingleAsync(s => s.TenantId == c.Id && s.StockCode == "A")))
            .StockCode.Should().Be("A", "the card's own code is stored");

        // Back to every default: the row goes; a product not sent keeps its settings.
        (await SendAsync(_factory, HttpMethod.Put, Base + "/products", c.Patron, new
        {
            revision = 1,
            items = new object[] { new { stockCode = "A", sortOrder = (int?)null, hidden = false, noDiscount = false, cartonOnly = false, cartonQuantity = (int?)null } },
        })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadAsync(_factory, db => db.CatalogProductSettings.AsNoTracking().Where(s => s.TenantId == c.Id).Select(s => s.StockCode).ToListAsync()))
            .Should().Equal("B");
    }

    [Fact]
    public async Task Products_are_searched_in_Turkish_across_the_catalog()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        await SeedAsync(_factory, db =>
        {
            for (var i = 0; i < 55; i++)
                Record(db, c.Id, "stocks", $"T{i:00}", new { stockCode = $"T{i:00}", name = $"Test ürünü {i:00}", mainGroupCode = "GIDA", subGroupCode = "CAY" });
        });

        async Task<CatalogProductsResponse> Search(string query) =>
            await OkAsync<CatalogProductsResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/products?q=" + Uri.EscapeDataString(query), c.Patron));

        (await Search("ışık")).Items.Select(p => p.StockCode).Should().Equal("C");
        (await Search("8690001")).Items.Select(p => p.StockCode).Should().Equal("A");
        (await Search("ÇAY")).Items.Select(p => p.StockCode).Should().Equal("A");
        var many = await Search("ÜRÜNÜ");
        many.Items.Should().HaveCount(50);
        many.Truncated.Should().BeTrue();
        var all = await OkAsync<CatalogProductsResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/products?category=" + Uri.EscapeDataString("Çay"), c.Patron));
        all.Items.Should().HaveCount(56);
        all.Truncated.Should().BeFalse();
        (await OkAsync<CatalogProductsResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/products?category=Yok", c.Patron))).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task An_account_is_made_for_a_known_customer_with_a_password_shown_once()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts/by-customer?code=YOK", c.Patron), HttpStatusCode.NotFound, "CUSTOMER_NOT_FOUND");
        var lookup = await OkAsync<CatalogAccountByCustomerResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts/by-customer?code=c1", c.Patron));
        lookup.Account.Should().BeNull();
        lookup.CustomerName.Should().Be("Yılmaz Market Ltd. Şti.");
        lookup.SuggestedUsername.Should().Be("yilmaz-market");

        var createdResponse = await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Mudur, NewAccount("c1", " A.B "));
        var created = await OkAsync<CatalogAccountSavedResponse>(createdResponse, HttpStatusCode.Created);
        created.IssuedPassword.Should().MatchRegex("^[a-hjkmnp-z2-9]{10}$");
        created.Account.Should().Match<CatalogAccountDto>(a =>
            a.Username == "a.b" && a.CustomerCode == "C1" && a.CustomerName == "Yılmaz Market Ltd. Şti." && a.IsActive && a.DiscountPercent == 12.5m
            && a.PriceListNo == 2 && a.ShowStatement && !a.ShowInvoices && a.CanOrder && a.CreatedByName == "mudur bey" && a.OpenOrderCount == 0);
        created.Account.Visibility.Mode.Should().Be("all");
        created.Account.Visibility.Rules.Select(r => (r.Type, r.Key, r.Effect)).Should().Equal(("category", "Kahve", "deny"));
        var row = await ReadAsync(_factory, db => db.CatalogAccounts.AsNoTracking().SingleAsync(a => a.Id == created.Account.Id));
        BCrypt.Net.BCrypt.Verify(created.IssuedPassword, row.PasswordHash).Should().BeTrue();
        row.TokenVersion.Should().Be(0);

        // Never again: the lookup and the list carry the account, not the password.
        var again = await SendAsync(_factory, HttpMethod.Get, Base + "/accounts/by-customer?code=C1", c.Patron);
        (await again.Content.ReadAsStringAsync()).Should().NotContain("issuedPassword").And.NotContain(created.IssuedPassword!);
        (await again.ReadAsJsonAsync<CatalogAccountByCustomerResponse>()).Account!.Id.Should().Be(created.Account.Id);
        var list = await OkAsync<CatalogAccountListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts", c.Patron));
        list.Total.Should().Be(1);
        list.Items.Single().Should().Match<CatalogAccountSummaryDto>(a => a.Username == "a.b" && a.DiscountPercent == 12.5m);

        // A password staff typed comes back as nothing.
        var typed = await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron,
            NewAccount("C2", "akgida", password: "uzun-bir-sifre")), HttpStatusCode.Created);
        typed.IssuedPassword.Should().BeNull();
    }

    [Fact]
    public async Task One_live_account_per_customer_and_per_username()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var first = await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz-market")), HttpStatusCode.Created);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C2", "Yilmaz-Market")), HttpStatusCode.Conflict, "CATALOG_USERNAME_TAKEN");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "baska")), HttpStatusCode.Conflict, "CATALOG_ACCOUNT_EXISTS");
        // The suggestion steps around a name in use.
        (await OkAsync<CatalogAccountByCustomerResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts/by-customer?code=C1", c.Patron)))
            .SuggestedUsername.Should().Be("yilmaz-market2");

        (await SendAsync(_factory, HttpMethod.Delete, $"{Base}/accounts/{first.Account.Id}", c.Patron)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Delete, $"{Base}/accounts/{first.Account.Id}", c.Patron), HttpStatusCode.NotFound, "CATALOG_ACCOUNT_NOT_FOUND");
        var deleted = await ReadAsync(_factory, db => db.CatalogAccounts.AsNoTracking().SingleAsync(a => a.Id == first.Account.Id));
        deleted.Should().Match<CatalogAccount>(a => a.DeletedAtMs != null && !a.IsActive && a.TokenVersion == 1);
        (await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz-market"))).StatusCode
            .Should().Be(HttpStatusCode.Created, "a deleted account frees its customer and its username");
    }

    [Fact]
    public async Task Bad_account_fields_are_refused()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var other = await CompanyAsync(_factory);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("YOK", "kim")), HttpStatusCode.NotFound, "CUSTOMER_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "ab")), HttpStatusCode.BadRequest, "INVALID_USERNAME");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "a b c")), HttpStatusCode.BadRequest, "INVALID_USERNAME");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz", password: "1234567")), HttpStatusCode.BadRequest, "INVALID_PASSWORD");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz", password: new string('x', 73))), HttpStatusCode.BadRequest, "INVALID_PASSWORD");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz", discount: 100m)), HttpStatusCode.BadRequest, "INVALID_DISCOUNT");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz", discount: -1m)), HttpStatusCode.BadRequest, "INVALID_DISCOUNT");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz", priceListNo: 9)), HttpStatusCode.BadRequest, "UNKNOWN_PRICE_LIST");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz", mode: "some")), HttpStatusCode.BadRequest, "INVALID_VISIBILITY");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz", responsible: other.AliId)), HttpStatusCode.BadRequest, "INVALID_RESPONSIBLE_USER");
        (await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz", password: "çğşıöüçğ", discount: 99.99m, responsible: c.AliId)))
            .StatusCode.Should().Be(HttpStatusCode.Created, "eight Turkish letters are 16 bytes");
    }

    [Fact]
    public async Task Patch_changes_only_what_is_sent_and_sessions_end_on_deactivation_password_and_revoke()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var account = (await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron,
            NewAccount("C1", "yilmaz", responsible: c.AliId)), HttpStatusCode.Created)).Account;
        await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C2", "akgida")), HttpStatusCode.Created);
        var path = $"{Base}/accounts/{account.Id}";
        async Task<int> TokenVersion() => (await ReadAsync(_factory, db => db.CatalogAccounts.AsNoTracking().SingleAsync(a => a.Id == account.Id))).TokenVersion;

        var patched = (await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Patch, path, c.Mudur,
            new { discountPercent = 5, showInvoices = true }))).Account;
        patched.Should().Match<CatalogAccountDto>(a => a.DiscountPercent == 5m && a.ShowInvoices && a.ShowStatement && a.PriceListNo == 2
            && a.ResponsibleUserId == c.AliId && a.Username == "yilmaz" && a.Visibility.Rules.Length == 1);

        // Sent as null: back to the company default and to no responsible person.
        var cleared = (await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Patch, path, c.Mudur,
            new { priceListNo = (int?)null, responsibleUserId = (Guid?)null, visibility = new { mode = "only", rules = new[] { new { type = "product", key = "A", effect = "allow" } } } }))).Account;
        cleared.Should().Match<CatalogAccountDto>(a => a.PriceListNo == null && a.ResponsibleUserId == null && a.Visibility.Mode == "only" && a.DiscountPercent == 5m);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Patch, path, c.Mudur, new { username = "akgida" }), HttpStatusCode.Conflict, "CATALOG_USERNAME_TAKEN");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Patch, path, c.Mudur, new { discountPercent = 120 }), HttpStatusCode.BadRequest, "INVALID_DISCOUNT");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Patch, $"{Base}/accounts/{Guid.NewGuid()}", c.Mudur, new { canOrder = false }), HttpStatusCode.NotFound, "CATALOG_ACCOUNT_NOT_FOUND");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Patch, path, c.Ali, new { canOrder = false }), HttpStatusCode.Forbidden, "CATALOG_MANAGE_REQUIRED");
        (await TokenVersion()).Should().Be(0);

        (await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Patch, path, c.Mudur, new { username = "Yilmaz.Market", isActive = false })))
            .Account.Should().Match<CatalogAccountDto>(a => a.Username == "yilmaz.market" && !a.IsActive);
        (await TokenVersion()).Should().Be(1, "a deactivated account's sessions end");

        var reset = await OkAsync<CatalogPasswordResponse>(await SendAsync(_factory, HttpMethod.Put, path + "/password", c.Mudur, new { password = (string?)null }));
        reset.IssuedPassword.Should().HaveLength(10);
        (await TokenVersion()).Should().Be(2);
        var typedResponse = await SendAsync(_factory, HttpMethod.Put, path + "/password", c.Mudur, new { password = "yeni-sifre-1" });
        (await OkAsync<CatalogPasswordResponse>(typedResponse)).IssuedPassword.Should().BeNull();
        var row = await ReadAsync(_factory, db => db.CatalogAccounts.AsNoTracking().SingleAsync(a => a.Id == account.Id));
        BCrypt.Net.BCrypt.Verify("yeni-sifre-1", row.PasswordHash).Should().BeTrue();
        row.TokenVersion.Should().Be(3);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Put, path + "/password", c.Mudur, new { password = "kisa" }), HttpStatusCode.BadRequest, "INVALID_PASSWORD");

        (await SendAsync(_factory, HttpMethod.Post, path + "/revoke-sessions", c.Mudur)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await TokenVersion()).Should().Be(4);
    }

    [Fact]
    public async Task The_account_list_is_searched_in_Turkish_paged_and_counts_open_requests()
    {
        var c = await CompanyAsync(_factory);
        await SeedCatalogAsync(_factory, c.Id);
        var yilmaz = (await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C1", "yilmaz")), HttpStatusCode.Created)).Account;
        await OkAsync<CatalogAccountSavedResponse>(await SendAsync(_factory, HttpMethod.Post, Base + "/accounts", c.Patron, NewAccount("C2", "akgida")), HttpStatusCode.Created);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await SeedAsync(_factory, db =>
        {
            foreach (var (no, status) in new[] { ("KT-AAAAA3", CatalogOrderStatuses.New), ("KT-AAAAA4", CatalogOrderStatuses.Claimed), ("KT-AAAAA5", CatalogOrderStatuses.Completed) })
                db.CatalogOrders.Add(new CatalogOrder
                {
                    Id = Guid.NewGuid(), TenantId = c.Id, AccountId = yilmaz.Id, CustomerCode = "C1", CustomerName = "Yılmaz", AccountUsername = "yilmaz",
                    No = no, Status = status, SubmittedAtMs = now, UpdatedAtMs = now,
                });
        });

        var all = await OkAsync<CatalogAccountListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts", c.Patron));
        all.Total.Should().Be(2);
        all.Items.Select(a => (a.CustomerName, a.OpenOrderCount)).Should().Equal(("Ak Gıda", 0), ("Yılmaz Market Ltd. Şti.", 2));
        (await OkAsync<CatalogAccountListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts?q=" + Uri.EscapeDataString("YILMAZ"), c.Patron)))
            .Items.Select(a => a.Username).Should().Equal("yilmaz");
        (await OkAsync<CatalogAccountListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts?q=" + Uri.EscapeDataString("gıda"), c.Patron)))
            .Items.Select(a => a.Username).Should().Equal("akgida");
        var second = await OkAsync<CatalogAccountListResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts?page=2", c.Patron));
        second.Total.Should().Be(2);
        second.Items.Should().BeEmpty();
        (await OkAsync<CatalogAccountByCustomerResponse>(await SendAsync(_factory, HttpMethod.Get, Base + "/accounts/by-customer?code=C1", c.Patron)))
            .Account!.OpenOrderCount.Should().Be(2);
    }

    private static object NewAccount(
        string customerCode, string username, string? password = null, decimal discount = 12.5m, int? priceListNo = 2, string mode = "all", Guid? responsible = null) => new
    {
        customerCode,
        username,
        password,
        isActive = true,
        discountPercent = discount,
        priceListNo,
        visibility = new { mode, rules = new[] { new { type = "category", key = "Kahve", effect = "deny" } } },
        showStatement = true,
        showInvoices = false,
        showPurchased = false,
        canOrder = true,
        responsibleUserId = responsible,
    };
}
