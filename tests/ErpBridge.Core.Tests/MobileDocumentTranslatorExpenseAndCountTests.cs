using ErpBridge.Core.Jobs;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Gider ve sayım gövdelerinin çevrilmesi (ERP yazım 3 Y3d). İkisinin de karşı tarafı yoktur ve
/// ikisi de telefondan yeni alan istiyor, bu yüzden testler çoğunlukla "eksik gelirse ne olur"u sabitler.
/// </summary>
public class MobileDocumentTranslatorExpenseAndCountTests
{
    private readonly MobileDocumentTranslator _sut = new();

    private static ErpWriteContext Context(string? cash = "001", string? card = "14", string? transfer = "04", int? warehouse = 1) =>
        new("invoice", "approved", new ErpWriteSeries("S", "I", "T", "R", "M"), warehouse, cash, card, transfer, 1, "PLS01", 1, "ÇEK", "SENET", "plasiyer1");

    private const string Expense = """
        {
          "mobileDocumentId": "MOB-GD-1", "occurredAt": "19.09.2026 14:20", "amount": 2400.00,
          "expenseCardCode": "YAKIT", "paymentType": "Nakit", "cashCode": "001",
          "vatAmount": 400.00, "vatPointer": 4, "description": "Araç yakıtı"
        }
        """;

    private const string Count = """
        {
          "mobileDocumentId": "MOB-SY-1", "occurredAt": "19.09.2026 16:30", "amount": 0,
          "warehouseNo": 1, "description": "Depo sayımı",
          "lines": [
            { "stockCode": "59030", "barcode": "8690000000001", "countedQuantity": 12 },
            { "stockCode": "58780", "countedQuantity": 0 }
          ]
        }
        """;

    // ---- gider ---------------------------------------------------------------------------------

    [Fact]
    public void An_expense_becomes_a_command_naming_the_card_and_the_paying_account()
    {
        var result = _sut.Translate("expense", "MOB-GD-1", Expense, Context());

        result.Error.Should().BeNull();
        var expense = result.Expense!;
        expense.Method.Should().Be(ExpensePaymentMethod.Cash);
        expense.ExpenseCardCode.Should().Be("YAKIT");
        expense.AccountCode.Should().Be("001");
        expense.Amount.Should().Be(2400.00m);
        expense.VatAmount.Should().Be(400.00m);
        expense.VatPointer.Should().Be(4);
        expense.Header.CustomerCode.Should().BeEmpty("giderin karşı tarafı bir cari değil");
        expense.Header.Series.Should().BeEmpty("canlıda tip 37 serisizdir");
    }

    [Theory]
    [InlineData("Havale", ExpensePaymentMethod.Transfer, "04")]
    [InlineData("Kredi Kartı", ExpensePaymentMethod.CreditCard, "14")]
    public void Each_payment_method_falls_back_to_the_account_the_company_configured(
        string paymentType, ExpensePaymentMethod method, string account)
    {
        var body = Expense.Replace(
            "\"paymentType\": \"Nakit\", \"cashCode\": \"001\"",
            $"\"paymentType\": \"{paymentType}\"",
            StringComparison.Ordinal);

        var expense = _sut.Translate("expense", "MOB-GD-1", body, Context()).Expense!;

        expense.Method.Should().Be(method);
        expense.AccountCode.Should().Be(account);
    }

    /// <summary>K2: kart ERP kataloğundan gelir; eski telefonun kategori adı koda çevrilmeye çalışılmaz.</summary>
    [Fact]
    public void An_expense_without_a_card_code_asks_for_the_phone_to_be_updated()
    {
        var body = Expense.Replace("\"expenseCardCode\": \"YAKIT\", ", "", StringComparison.Ordinal);

        _sut.Translate("expense", "MOB-GD-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    /// <summary>Tediyedeki kural (PR #141): adı bilinen ama kodu bilinmeyen banka varsayılana düşürülmez.</summary>
    [Fact]
    public void A_named_bank_without_its_code_is_refused_rather_than_posted_to_the_default()
    {
        var body = Expense.Replace(
            "\"paymentType\": \"Nakit\", \"cashCode\": \"001\"",
            "\"paymentType\": \"Havale\", \"bankName\": \"Ziraat\"",
            StringComparison.Ordinal);

        _sut.Translate("expense", "MOB-GD-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    [Fact]
    public void A_cheque_expense_is_refused_by_name_rather_than_written_as_something_else()
    {
        var body = Expense.Replace("\"paymentType\": \"Nakit\"", "\"paymentType\": \"Çek\"", StringComparison.Ordinal);

        _sut.Translate("expense", "MOB-GD-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.UnsupportedPaymentTypeCode);
    }

    [Fact]
    public void Vat_larger_than_the_amount_is_refused()
    {
        var body = Expense.Replace("\"vatAmount\": 400.00", "\"vatAmount\": 2500.00", StringComparison.Ordinal);

        _sut.Translate("expense", "MOB-GD-1", body, Context()).Error!.Code.Should().Be(ErpWriteError.InvalidAmountCode);
    }

    [Fact]
    public void An_expense_without_vat_translates_with_zero_rather_than_failing()
    {
        var body = Expense.Replace("\"vatAmount\": 400.00, \"vatPointer\": 4, ", "", StringComparison.Ordinal);

        var expense = _sut.Translate("expense", "MOB-GD-1", body, Context()).Expense!;

        expense.VatAmount.Should().Be(0m);
        expense.VatPointer.Should().Be(0);
    }

    /// <summary>
    /// Gider ERP uyumu: KDV Mikro'da işaretçinin kendi kolonuna yazılır. İşaretçisiz KDV gönderen telefon
    /// ERP'nin KDV tanımlarını bilmeyen eski sürümdür; oran tahmin edilmez, güncelleme istenir.
    /// </summary>
    [Fact]
    public void Vat_without_the_pointer_it_was_picked_under_asks_for_an_app_update()
    {
        var body = Expense.Replace("\"vatPointer\": 4, ", "", StringComparison.Ordinal);

        _sut.Translate("expense", "MOB-GD-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void A_vat_pointer_mikro_does_not_have_is_refused(int pointer)
    {
        var body = Expense.Replace("\"vatPointer\": 4", $"\"vatPointer\": {pointer}", StringComparison.Ordinal);

        _sut.Translate("expense", "MOB-GD-1", body, Context()).Error!.Code.Should().Be(ErpWriteError.InvalidDocumentCode);
    }

    [Fact]
    public void An_expense_with_no_cash_box_configured_names_the_missing_setting()
    {
        var body = Expense.Replace(", \"cashCode\": \"001\"", "", StringComparison.Ordinal);

        _sut.Translate("expense", "MOB-GD-1", body, Context(cash: null)).Error!.Code
            .Should().Be(ErpWriteError.ErpMappingMissingCode);
    }

    // ---- sayım ---------------------------------------------------------------------------------

    [Fact]
    public void A_count_becomes_a_command_of_lines_in_the_order_they_were_counted()
    {
        var result = _sut.Translate("stock_count", "MOB-SY-1", Count, Context());

        result.Error.Should().BeNull();
        var count = result.StockCount!;
        count.WarehouseNo.Should().Be(1);
        count.Lines.Should().HaveCount(2);
        count.Lines[0].Should().Be(new StockCountLine("59030", "8690000000001", 12m));
        count.Lines[1].Should().Be(new StockCountLine("58780", null, 0m));
        count.Header.ExpectedTotal.Should().Be(0m, "sayım para taşımaz");
        count.Header.CustomerCode.Should().BeEmpty();
    }

    /// <summary>K10: barkodu koda çevirmeyi ERP'ye bırakmak, yanlış ürünü saymak demek olurdu.</summary>
    [Fact]
    public void A_line_with_only_a_barcode_asks_for_the_phone_to_be_updated()
    {
        var body = Count.Replace("\"stockCode\": \"59030\", ", "", StringComparison.Ordinal);

        _sut.Translate("stock_count", "MOB-SY-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    [Fact]
    public void A_negative_count_is_refused_but_zero_is_not()
    {
        var body = Count.Replace("\"countedQuantity\": 0", "\"countedQuantity\": -1", StringComparison.Ordinal);

        _sut.Translate("stock_count", "MOB-SY-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.InvalidCountedQuantityCode);
        _sut.Translate("stock_count", "MOB-SY-1", Count, Context()).Error.Should().BeNull();
    }

    [Fact]
    public void A_count_with_no_lines_is_not_a_count()
    {
        var body = """
            { "mobileDocumentId": "MOB-SY-1", "occurredAt": "19.09.2026 16:30", "amount": 0, "warehouseNo": 1, "lines": [] }
            """;

        _sut.Translate("stock_count", "MOB-SY-1", body, Context()).Error!.Code.Should().Be(ErpWriteError.InvalidDocumentCode);
    }

    [Fact]
    public void A_count_without_a_warehouse_falls_back_to_the_company_setting()
    {
        var body = Count.Replace("\"warehouseNo\": 1, ", "", StringComparison.Ordinal);

        _sut.Translate("stock_count", "MOB-SY-1", body, Context(warehouse: 7)).StockCount!.WarehouseNo.Should().Be(7);
    }

    [Fact]
    public void A_count_with_no_warehouse_anywhere_names_the_missing_setting()
    {
        var body = Count.Replace("\"warehouseNo\": 1, ", "", StringComparison.Ordinal);

        _sut.Translate("stock_count", "MOB-SY-1", body, Context(warehouse: null)).Error!.Code
            .Should().Be(ErpWriteError.ErpMappingMissingCode);
    }

    // ---- alış ----------------------------------------------------------------------------------

    private const string Purchase = """
        {
          "mobileDocumentId": "MOB-PR-1", "occurredAt": "19.09.2026 11:00", "supplierCode": "JUMBO",
          "counterparty": "Jumbo Gıda", "invoiceNo": "A-42", "amount": 1000.00,
          "lines": [ { "productCode": "59030", "quantity": 10, "unitPrice": 100.00, "lineTotal": 1000.00 } ]
        }
        """;

    [Fact]
    public void A_purchase_becomes_a_command_naming_the_supplier_and_its_invoice()
    {
        var result = _sut.Translate("purchase_receipt", "MOB-PR-1", Purchase, Context());

        result.Error.Should().BeNull();
        var purchase = result.Purchase!;
        purchase.Header.CustomerCode.Should().Be("JUMBO", "alışta karşı taraf supplierCode'dur");
        purchase.SupplierInvoiceNo.Should().Be("A-42");
        purchase.WarehouseNo.Should().Be(1);
        purchase.PricesIncludeVat.Should().BeFalse("telefon alışta KDV tutmuyor; ayar söyler");
        purchase.Lines.Should().ContainSingle().Which.Should().Be(new PurchaseInvoiceLine("59030", 10m, 100.00m, 1));
        purchase.Header.Series.Should().BeEmpty("seriyi writer tedarikçinin geçmişinden sürdürür (K7)");
    }

    /// <summary>Tedarikçinin fiyatı belgenin kendisidir; fiyatsız satır başka bir yerden fiyatlanamaz.</summary>
    [Fact]
    public void A_purchase_line_without_a_unit_price_asks_for_the_phone_to_be_updated()
    {
        var body = Purchase.Replace("\"unitPrice\": 100.00, ", "", StringComparison.Ordinal);

        _sut.Translate("purchase_receipt", "MOB-PR-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    [Fact]
    public void A_purchase_without_a_supplier_code_names_the_missing_counterparty()
    {
        var body = Purchase.Replace("\"supplierCode\": \"JUMBO\",", "", StringComparison.Ordinal);

        _sut.Translate("purchase_receipt", "MOB-PR-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.MissingCustomerCodeCode);
    }

    /// <summary>
    /// K13: peşin ödenen alış tek evraktır, kapalı fatura. Ödeme bilgisi yoksa fatura açık kalır —
    /// eski telefon ödemeyi ayrı bir tediye olarak gönderiyor ve faturayı da kapatmak aynı ödemeyi
    /// iki kez gösterirdi.
    /// </summary>
    [Fact]
    public void A_purchase_without_payment_information_stays_open()
    {
        var purchase = _sut.Translate("purchase_receipt", "MOB-PR-1", Purchase, Context()).Purchase!;

        purchase.Settlement.Should().Be(PurchaseSettlement.Open);
        purchase.SettlementAccountCode.Should().BeNull();
    }

    [Theory]
    [InlineData("Nakit", PurchaseSettlement.Cash, "001")]
    [InlineData("Havale", PurchaseSettlement.Bank, "04")]
    public void A_paid_purchase_closes_to_the_account_it_was_paid_from(
        string paymentType, PurchaseSettlement settlement, string account)
    {
        var body = Purchase.Replace("\"invoiceNo\": \"A-42\",", $"\"invoiceNo\": \"A-42\", \"paymentType\": \"{paymentType}\",", StringComparison.Ordinal);

        var purchase = _sut.Translate("purchase_receipt", "MOB-PR-1", body, Context()).Purchase!;

        purchase.Settlement.Should().Be(settlement);
        purchase.SettlementAccountCode.Should().Be(account);
    }

    [Fact]
    public void A_purchase_paid_on_account_stays_open()
    {
        var body = Purchase.Replace("\"invoiceNo\": \"A-42\",", "\"invoiceNo\": \"A-42\", \"paymentType\": \"Cari Borç\",", StringComparison.Ordinal);

        _sut.Translate("purchase_receipt", "MOB-PR-1", body, Context()).Purchase!.Settlement
            .Should().Be(PurchaseSettlement.Open);
    }

    /// <summary>Tediyedeki kural: adı bilinen ama kodu bilinmeyen banka varsayılana düşürülmez.</summary>
    [Fact]
    public void A_purchase_paid_to_a_named_bank_without_its_code_is_refused()
    {
        var body = Purchase.Replace("\"invoiceNo\": \"A-42\",", "\"invoiceNo\": \"A-42\", \"paymentType\": \"Havale\", \"bankName\": \"Ziraat\",", StringComparison.Ordinal);

        _sut.Translate("purchase_receipt", "MOB-PR-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    [Fact]
    public void A_purchase_paid_by_cheque_is_refused_by_name()
    {
        var body = Purchase.Replace("\"invoiceNo\": \"A-42\",", "\"invoiceNo\": \"A-42\", \"paymentType\": \"Çek\",", StringComparison.Ordinal);

        _sut.Translate("purchase_receipt", "MOB-PR-1", body, Context()).Error!.Code
            .Should().Be(ErpWriteError.UnsupportedPaymentTypeCode);
    }

    [Fact]
    public void A_purchase_with_no_warehouse_anywhere_names_the_missing_setting()
    {
        _sut.Translate("purchase_receipt", "MOB-PR-1", Purchase, Context(warehouse: null)).Error!.Code
            .Should().Be(ErpWriteError.ErpMappingMissingCode);
    }

    /// <summary>Gövdenin adlandırdığı belge işin anahtarıyla aynı olmalı (PR #81).</summary>
    [Theory]
    [InlineData("expense", "MOB-GD-1")]
    [InlineData("stock_count", "MOB-SY-1")]
    [InlineData("purchase_receipt", "MOB-PR-1")]
    public void A_body_naming_another_document_does_not_pass_under_a_fresh_key(string type, string id)
    {
        var body = type switch { "expense" => Expense, "stock_count" => Count, _ => Purchase };

        _sut.Translate(type, id + "-BASKA", body, Context()).Error!.Code.Should().Be(ErpWriteError.DocumentIdMismatchCode);
    }
}
