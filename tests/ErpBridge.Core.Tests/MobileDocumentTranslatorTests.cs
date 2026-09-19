using ErpBridge.Core.Jobs;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

public class MobileDocumentTranslatorTests
{
    private readonly MobileDocumentTranslator _sut = new();

    private static ErpWriteContext Context(string kind = "invoice", int? warehouse = 1, string? cash = "001", string? card = "14", string? transfer = "04", int? user = 1) =>
        new(kind, "approved", new ErpWriteSeries("S", "I", "T", "R", "M"), warehouse, cash, card, transfer, user, "PLS01", 1, "ÇEK", "SENET", "plasiyer1");

    private const string Sale = """
        {
          "mobileDocumentId": "MOB-SO-1", "revision": 1, "occurredAt": "17.09.2026 10:15",
          "customerCode": "120.001", "counterparty": "Bakkal Ali", "amount": 684.00,
          "paymentType": "Cari Borç", "priceListNo": 1, "description": "Katalog siparişi",
          "lines": [
            { "productCode": "B575", "barcode": "869", "quantity": 2, "listUnitPrice": 400.00,
              "lineDiscountPercent": 10, "customerDiscountPercent": 0, "generalDiscountPercent": 5, "unitPrice": 342.00 }
          ]
        }
        """;

    [Fact]
    public void An_open_sale_becomes_a_document_of_the_company_kind_with_list_prices_and_discounts()
    {
        var result = _sut.Translate("sales_order", "MOB-SO-1", Sale, Context());

        result.Error.Should().BeNull();
        var sale = result.Sale!;
        sale.Kind.Should().Be(SalesDocumentKind.Invoice);
        sale.Settlement.Should().Be(SalesSettlement.Open);
        sale.SettlementAccountCode.Should().BeNull();
        sale.ExtraPayments.Should().BeNull();
        sale.WarehouseNo.Should().Be(1);
        sale.PriceListNo.Should().Be(1);
        sale.Header.Should().Be(new ErpDocumentHeader("MOB-SO-1", new DateTime(2026, 9, 17, 10, 15, 0), "120.001", "PLS01", 1, "T", "Katalog siparişi", 684.00m));
        sale.Lines.Should().ContainSingle().Which.Should().Be(new SalesDocumentLine("B575", 2m, 1, 400.00m, 10m, 0m, 5m));
    }

    [Theory]
    [InlineData("Nakit", SalesSettlement.Cash, "001")]
    [InlineData("Kredi Kartı", SalesSettlement.Card, "14")]
    [InlineData("Banka Kartı", SalesSettlement.Card, "14")]
    [InlineData("EFT / Havale", SalesSettlement.Transfer, "04")]
    [InlineData("Havale / EFT", SalesSettlement.Transfer, "04")]
    public void A_paid_invoice_is_closed_to_the_company_cash_box_or_bank(string paymentType, SalesSettlement settlement, string account)
    {
        var body = Sale.Replace("\"Cari Borç\"", $"\"{paymentType}\"");

        var sale = _sut.Translate("sales_order", "MOB-SO-1", body, Context()).Sale!;

        sale.Settlement.Should().Be(settlement);
        sale.SettlementAccountCode.Should().Be(account);
        sale.ExtraPayments.Should().BeNull();
    }

    [Fact]
    public void The_bank_the_phone_picked_wins_over_the_company_default()
    {
        var body = Sale.Replace("\"Cari Borç\"", "\"Kredi Kartı\", \"bankCode\": \"13\"");

        _sut.Translate("sales_order", "MOB-SO-1", body, Context()).Sale!.SettlementAccountCode.Should().Be("13");
    }

    [Fact]
    public void A_paid_order_stays_open_and_takes_a_receipt_for_the_money()
    {
        var body = Sale.Replace("\"Cari Borç\"", "\"Nakit\"");

        var sale = _sut.Translate("sales_order", "MOB-SO-1", body, Context(kind: "order")).Sale!;

        sale.Kind.Should().Be(SalesDocumentKind.Order);
        sale.Header.Series.Should().Be("S");
        sale.Settlement.Should().Be(SalesSettlement.Open);
        sale.ExtraPaymentsSeries.Should().Be("M");
        sale.ExtraPayments.Should().ContainSingle().Which.Should().Be(new CollectionPayment(CollectionMethod.Cash, 684.00m, new DateTime(2026, 9, 17), "001"));
    }

    [Fact]
    public void Mixed_payments_leave_the_sale_open_with_a_receipt_line_each()
    {
        var body = Sale.Replace("\"paymentType\": \"Cari Borç\",", "\"payments\": [{ \"method\": \"cash\", \"amount\": 300 }, { \"method\": \"card\", \"amount\": 384, \"bankCode\": \"13\", \"installments\": 3, \"surchargeAmount\": 11.52 }],");

        var sale = _sut.Translate("sales_order", "MOB-SO-1", body, Context()).Sale!;

        sale.Settlement.Should().Be(SalesSettlement.Open);
        sale.ExtraPayments.Should().Equal(
            new CollectionPayment(CollectionMethod.Cash, 300m, new DateTime(2026, 9, 17), "001"),
            new CollectionPayment(CollectionMethod.Card, 384m, new DateTime(2026, 9, 17), "13", 3, 11.52m));
    }

    [Fact]
    public void An_old_phone_sale_without_list_prices_is_refused_not_guessed()
    {
        const string old = """{ "mobileDocumentId": "MOB-SO-1", "occurredAt": "17.09.2026 10:15", "customerCode": "120.001", "amount": 684, "paymentType": "Cari Borç", "lines": [ { "productCode": "B575", "quantity": 2, "unitPrice": 342 } ] }""";

        _sut.Translate("sales_order", "MOB-SO-1", old, Context()).Error!.Code.Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    [Theory]
    [InlineData("\"customerCode\": \"120.001\",", "", ErpWriteError.MissingCustomerCodeCode)]
    [InlineData("\"amount\": 684.00,", "\"amount\": 684.00, \"currency\": \"USD\",", ErpWriteError.UnsupportedCurrencyCode)]
    [InlineData("\"occurredAt\": \"17.09.2026 10:15\",", "\"occurredAt\": \"dün\",", ErpWriteError.InvalidDocumentDateCode)]
    [InlineData("\"productCode\": \"B575\",", "", ErpWriteError.MissingStockCodeCode)]
    [InlineData("\"quantity\": 2,", "\"quantity\": 0,", ErpWriteError.InvalidQuantityCode)]
    [InlineData("\"lineDiscountPercent\": 10,", "\"lineDiscountPercent\": 110,", ErpWriteError.InvalidDiscountCode)]
    [InlineData("\"Cari Borç\"", "\"Hediye çeki\"", ErpWriteError.UnsupportedPaymentTypeCode)]
    public void A_sale_with_bad_data_names_the_problem(string replace, string with, string code)
    {
        _sut.Translate("sales_order", "MOB-SO-1", Sale.Replace(replace, with), Context()).Error!.Code.Should().Be(code);
    }

    [Fact]
    public void Missing_company_settings_are_named()
    {
        _sut.Translate("sales_order", "MOB-SO-1", Sale, Context(warehouse: null)).Error!.Message.Should().Contain("depo");
        _sut.Translate("sales_order", "MOB-SO-1", Sale, Context(user: null)).Error!.Message.Should().Contain("ERP kullanıcı numarası");
        _sut.Translate("sales_order", "MOB-SO-1", Sale.Replace("\"Cari Borç\"", "\"Nakit\""), Context(cash: null)).Error!
            .Should().Match<ErpWriteError>(e => e.Code == ErpWriteError.ErpMappingMissingCode && e.Message.Contains("kasa kodu"));
        _sut.Translate("sales_order", "MOB-SO-1", Sale, Context(kind: "proforma")).Error!.Message.Should().Contain("satış belge türü");
        _sut.Translate("sales_order", "MOB-SO-1", Sale, null).Error!.Code.Should().Be(ErpWriteError.ErpContextMissingCode);
    }

    [Fact]
    public void The_company_responsibility_center_project_and_delivery_offset_reach_the_command()
    {
        var context = Context(kind: "order") with { ResponsibilityCenterCode = "SM1", ProjectCode = " ", DeliveryDayOffset = 2 };

        var sale = _sut.Translate("sales_order", "MOB-SO-1", Sale, context).Sale!;

        sale.Header.ResponsibilityCenterCode.Should().Be("SM1");
        sale.Header.ProjectCode.Should().BeNull("a blank code is no code");
        sale.DeliveryDate.Should().Be(new DateTime(2026, 9, 19));
        _sut.Translate("sales_order", "MOB-SO-1", Sale, Context()).Sale!.DeliveryDate.Should().BeNull();
        _sut.Translate("sales_order", "MOB-SO-1", Sale, Context() with { DeliveryDayOffset = 2 }).Sale!.DeliveryDate
            .Should().BeNull("an invoice has no delivery date");
    }

    [Fact]
    public void An_iso_date_keeps_the_phone_wall_clock_time()
    {
        var body = Sale.Replace("\"17.09.2026 10:15\"", "\"2026-09-17T10:15:30+03:00\"");

        _sut.Translate("sales_order", "MOB-SO-1", body, Context()).Sale!.Header.OccurredAt.Should().Be(new DateTime(2026, 9, 17, 10, 15, 30));
    }

    private const string Return = """
        {
          "mobileDocumentId": "MOB-SR-1", "occurredAt": "2026-09-17T11:00:00+03:00", "customerCode": "120.001",
          "amount": 380.00, "settlementMethod": "Cari Alacak", "priceListNo": 1, "warehouseNo": 2,
          "lines": [
            { "productCode": "B575", "quantity": 1, "listUnitPrice": 400, "conditionPercent": 1, "reason": "Sağlam" },
            { "productCode": "XH1300", "quantity": 1, "listUnitPrice": 100, "conditionPercent": 0.3, "reason": "Hasarlı" }
          ]
        }
        """;

    [Theory]
    [InlineData("Cari Alacak", ReturnSettlement.Open, null)]
    [InlineData("Nakit", ReturnSettlement.Cash, "001")]
    [InlineData("Banka İade", ReturnSettlement.Bank, "04")]
    public void A_return_is_open_or_refunded_from_the_cash_box_or_bank(string method, ReturnSettlement settlement, string? account)
    {
        var result = _sut.Translate("sales_return", "MOB-SR-1", Return.Replace("\"Cari Alacak\"", $"\"{method}\""), Context());

        result.Error.Should().BeNull();
        result.Return!.Should().Match<SalesReturnCommand>(r => r.Settlement == settlement && r.SettlementAccountCode == account && r.WarehouseNo == 2 && r.Header.Series == "R");
        result.Return!.Lines.Select(l => l.ConditionRatio).Should().Equal(1m, 0.3m);
    }

    [Fact]
    public void A_condition_given_as_a_percentage_is_read_as_a_share()
    {
        _sut.Translate("sales_return", "MOB-SR-1", Return.Replace("\"conditionPercent\": 0.3", "\"conditionPercent\": 30"), Context())
            .Return!.Lines[1].ConditionRatio.Should().Be(0.3m);
    }

    [Fact]
    public void A_return_without_lines_from_an_old_phone_is_refused()
    {
        const string cashLog = """{ "mobileDocumentId": "K-1", "occurredAt": "17.09.2026 11:00", "transactionType": "İade", "customerCode": "120.001", "amount": 380, "paymentType": "Nakit" }""";

        _sut.Translate("sales_return", "K-1", cashLog, Context()).Error!.Code.Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    private const string Collection = """
        {
          "mobileDocumentId": "MOB-TH-1", "occurredAt": "17.09.2026 12:00", "customerCode": "120.001", "amount": 5000,
          "description": "Eylül tahsilatı",
          "payments": [
            { "method": "cash", "amount": 1000 },
            { "method": "card", "amount": 1500, "bankCode": "13", "installments": 3, "surchargeAmount": 45 },
            { "method": "transfer", "amount": 500 },
            { "method": "cheque", "amount": 1200, "dueDate": "2026-11-30", "cheque": { "no": "27703", "bankName": "Ziraat", "branch": "Fethiye", "accountNo": "123", "drawer": "Ali" } },
            { "method": "note", "amount": 800, "dueDate": "02.10.2026", "note": { "no": "S-5", "debtor": "Ali" } }
          ]
        }
        """;

    [Fact]
    public void A_collection_becomes_one_receipt_with_a_line_per_payment()
    {
        var result = _sut.Translate("collection", "MOB-TH-1", Collection, Context());

        result.Error.Should().BeNull();
        var receipt = result.Collection!;
        receipt.Header.Should().Match<ErpDocumentHeader>(h => h.Series == "M" && h.ExpectedTotal == 5000m && h.Description == "Eylül tahsilatı");
        receipt.Payments.Should().Equal(
            new CollectionPayment(CollectionMethod.Cash, 1000m, new DateTime(2026, 9, 17), "001"),
            new CollectionPayment(CollectionMethod.Card, 1500m, new DateTime(2026, 9, 17), "13", 3, 45m),
            new CollectionPayment(CollectionMethod.Transfer, 500m, new DateTime(2026, 9, 17), "04"),
            new CollectionPayment(CollectionMethod.Cheque, 1200m, new DateTime(2026, 11, 30), "ÇEK", Cheque: new ChequeDetails("27703", "Ziraat", "Fethiye", "123", "Ali")),
            new CollectionPayment(CollectionMethod.Note, 800m, new DateTime(2026, 10, 2), "SENET", Note: new NoteDetails("S-5", "Ali")));
    }

    /// <summary>
    /// The body the phone's cash book already sends (ERP yazım 2): one payment, no <c>payments[]</c> array.
    /// A purchase's cash payment produces exactly this, which is why a purchase used to fail with
    /// "No writer is configured for document type 'disbursement'".
    /// </summary>
    private const string Disbursement = """
        {
          "mobileDocumentId": "KL-9", "revision": 1, "occurredAt": "17.09.2026 12:00",
          "transactionType": "Tediye", "counterparty": "Bakkal Ali", "customerCode": "120.001",
          "amount": 1500.50, "paymentType": "Nakit", "description": "Saha Alış Girişi (A-42)"
        }
        """;

    [Fact]
    public void A_cash_disbursement_becomes_one_receipt_on_the_company_cash_box()
    {
        var result = _sut.Translate("disbursement", "KL-9", Disbursement, Context());

        result.Error.Should().BeNull();
        var payment = result.Disbursement!;
        payment.Method.Should().Be(DisbursementMethod.Cash);
        payment.Amount.Should().Be(1500.50m);
        payment.AccountCode.Should().Be("001", "the company's cash box, since the phone named none");
        payment.Header.Should().Match<ErpDocumentHeader>(h =>
            h.CustomerCode == "120.001" && h.ExpectedTotal == 1500.50m && h.Description == "Saha Alış Girişi (A-42)");
    }

    [Fact]
    public void A_transfer_disbursement_leaves_the_company_bank()
    {
        var body = Disbursement.Replace("\"paymentType\": \"Nakit\"", "\"paymentType\": \"Havale / EFT\"");

        var result = _sut.Translate("disbursement", "KL-9", body, Context());

        result.Disbursement!.Method.Should().Be(DisbursementMethod.Transfer);
        result.Disbursement.AccountCode.Should().Be("04", "the transfer bank from the company settings");
    }

    [Theory]
    // Cheque and note issues are a different Mikro document; refused by name rather than written as cash.
    [InlineData("\"paymentType\": \"Nakit\"", "\"paymentType\": \"Çek\"", ErpWriteError.UnsupportedPaymentTypeCode)]
    [InlineData("\"paymentType\": \"Nakit\"", "\"paymentType\": \"Senet\"", ErpWriteError.UnsupportedPaymentTypeCode)]
    [InlineData("\"amount\": 1500.50", "\"amount\": 0", ErpWriteError.InvalidAmountCode)]
    [InlineData("\"customerCode\": \"120.001\",", "", ErpWriteError.MissingCustomerCodeCode)]
    public void A_disbursement_the_erp_cannot_write_is_refused_by_name(string find, string replacement, string code)
    {
        var body = Disbursement.Replace(find, replacement);

        var result = _sut.Translate("disbursement", "KL-9", body, Context());

        result.Error!.Code.Should().Be(code);
        result.Disbursement.Should().BeNull();
    }

    /// <summary>
    /// The phone stores the bank the user chose as a display name. Posting such a transfer against the
    /// company's default bank would put the money in an account nobody picked, silently — so it is refused
    /// until the phone sends the ERP code (PR #141 Codex).
    /// </summary>
    [Fact]
    public void A_transfer_naming_a_bank_without_its_code_is_refused_rather_than_posted_to_the_default()
    {
        var body = Disbursement
            .Replace("\"paymentType\": \"Nakit\"", "\"paymentType\": \"Havale / EFT\"")
            .Replace("\"description\"", "\"bankName\": \"Ziraat Bankası\", \"description\"");

        var result = _sut.Translate("disbursement", "KL-9", body, Context());

        result.Error!.Code.Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
        result.Disbursement.Should().BeNull();
    }

    [Fact]
    public void A_transfer_that_names_its_bank_code_uses_it()
    {
        var body = Disbursement
            .Replace("\"paymentType\": \"Nakit\"", "\"paymentType\": \"Havale / EFT\"")
            .Replace("\"description\"", "\"bankName\": \"Ziraat Bankası\", \"bankCode\": \"12\", \"description\"");

        var result = _sut.Translate("disbursement", "KL-9", body, Context());

        result.Disbursement!.AccountCode.Should().Be("12", "the account the user actually picked");
    }

    [Fact]
    public void A_disbursement_without_a_cash_box_says_which_setting_is_missing()
    {
        var result = _sut.Translate("disbursement", "KL-9", Disbursement, Context(cash: null));

        result.Error!.Code.Should().Be(ErpWriteError.ErpMappingMissingCode);
        result.Error.Message.Should().Contain("kasa");
    }

    [Theory]
    [InlineData("\"no\": \"27703\", ", "", ErpWriteError.MissingChequeDetailsCode)]
    [InlineData("\"dueDate\": \"2026-11-30\", ", "", ErpWriteError.MissingChequeDetailsCode)]
    [InlineData("\"no\": \"S-5\", ", "", ErpWriteError.MissingNoteDetailsCode)]
    [InlineData("\"amount\": 5000,", "\"amount\": 4999,", ErpWriteError.InvalidAmountCode)]
    [InlineData("\"method\": \"transfer\"", "\"method\": \"gift\"", ErpWriteError.UnsupportedPaymentTypeCode)]
    public void A_collection_missing_what_the_erp_needs_is_refused(string replace, string with, string code)
    {
        _sut.Translate("collection", "MOB-TH-1", Collection.Replace(replace, with), Context()).Error!.Code.Should().Be(code);
    }

    [Fact]
    public void A_cash_log_collection_from_an_old_phone_is_refused_and_cheque_text_is_not_parsed()
    {
        const string cashLog = """{ "mobileDocumentId": "K-9", "occurredAt": "17.09.2026 12:00", "transactionType": "Tahsilat", "customerCode": "120.001", "amount": 1200, "paymentType": "Çek", "description": "Müşteri Çeki (No: 27703, Vade: 30.11.2026)" }""";

        _sut.Translate("collection", "K-9", cashLog, Context()).Error!.Code.Should().Be(ErpWriteError.MobileAppUpdateRequiredCode);
    }

    [Theory]
    [InlineData("\"priceListNo\": 1,", "\"priceListNo\": null,")]
    [InlineData("\"priceListNo\": 1,", "\"priceListNo\": 1.5,")]
    [InlineData("\"priceListNo\": 1,", "\"priceListNo\": \"birinci\",")]
    [InlineData("\"priceListNo\": 1,", "\"priceListNo\": 0,")]
    [InlineData("\"priceListNo\": 1,", "\"priceListNo\": 1, \"warehouseNo\": null,")]
    public void A_bad_list_or_warehouse_number_is_refused_not_replaced_by_the_company_default(string replace, string with)
    {
        _sut.Translate("sales_order", "MOB-SO-1", Sale.Replace(replace, with), Context()).Error!.Code.Should().Be(ErpWriteError.InvalidDocumentCode);
    }

    [Fact]
    public void A_sale_or_return_may_total_zero_but_a_collection_may_not()
    {
        var freeSale = _sut.Translate("sales_order", "MOB-SO-1", Sale.Replace("\"amount\": 684.00", "\"amount\": 0").Replace("\"Cari Borç\"", "\"Nakit\""), Context()).Sale!;
        freeSale.Settlement.Should().Be(SalesSettlement.Open, "nothing was paid");
        freeSale.ExtraPayments.Should().BeNull();
        freeSale.Header.ExpectedTotal.Should().Be(0m);

        _sut.Translate("sales_return", "MOB-SR-1", Return.Replace("\"amount\": 380", "\"amount\": 0"), Context()).Error.Should().BeNull();
        _sut.Translate("collection", "MOB-TH-1", Collection.Replace("\"amount\": 5000,", "\"amount\": 0,"), Context()).Error!.Code
            .Should().Be(ErpWriteError.InvalidAmountCode);
    }

    [Theory]
    [InlineData("sales_order", "\"paymentType\": \"Cari Borç\"", "\"paymentType\": \"Kredi Kartı\", \"bankCode\": 13")]
    [InlineData("sales_order", "\"paymentType\": \"Cari Borç\"", "\"paymentType\": 1")]
    [InlineData("sales_order", "\"amount\": 684.00,", "\"amount\": 684.00, \"currency\": 840,")]
    [InlineData("sales_order", "\"productCode\": \"B575\"", "\"productCode\": 575")]
    [InlineData("sales_return", "\"settlementMethod\": \"Cari Alacak\"", "\"settlementMethod\": [\"Nakit\"]")]
    [InlineData("collection", "\"bankCode\": \"13\"", "\"bankCode\": 13")]
    [InlineData("collection", "\"no\": \"27703\"", "\"no\": 27703")]
    public void A_code_sent_with_the_wrong_json_type_is_refused_not_replaced_by_the_default(string documentType, string replace, string with)
    {
        var (body, id) = documentType switch
        {
            "sales_order" => (Sale, "MOB-SO-1"),
            "sales_return" => (Return, "MOB-SR-1"),
            _ => (Collection, "MOB-TH-1"),
        };
        body.Should().Contain(replace);

        _sut.Translate(documentType, id, body.Replace(replace, with), Context()).Error!.Code.Should().Be(ErpWriteError.InvalidDocumentCode);
    }

    [Fact]
    public void The_smallest_decimal_is_refused_without_overflowing()
    {
        _sut.Translate("sales_order", "MOB-SO-1", Sale.Replace("\"listUnitPrice\": 400.00", "\"listUnitPrice\": -79228162514264337593543950335"), Context())
            .Error!.Code.Should().Be(ErpWriteError.InvalidAmountCode);
    }

    [Fact]
    public void A_zero_total_return_pays_nothing_out()
    {
        var result = _sut.Translate("sales_return", "MOB-SR-1", Return.Replace("\"amount\": 380", "\"amount\": 0").Replace("\"Cari Alacak\"", "\"Nakit\""), Context());

        result.Return!.Settlement.Should().Be(ReturnSettlement.Open);
        result.Return.SettlementAccountCode.Should().BeNull();
    }

    [Theory]
    [InlineData("sales_order", "\"lineDiscountPercent\": 10,", "\"lineDiscountPercent\": \"on\",", ErpWriteError.InvalidDiscountCode)]
    [InlineData("sales_order", "\"quantity\": 2,", "\"quantity\": 2, \"unitPointer\": 1.5,", ErpWriteError.InvalidDocumentCode)]
    [InlineData("sales_order", "\"quantity\": 2,", "\"quantity\": 2, \"unitPointer\": 0,", ErpWriteError.InvalidDocumentCode)]
    [InlineData("sales_return", "\"quantity\": 1, \"listUnitPrice\": 400", "\"quantity\": 1, \"unitPointer\": -1, \"listUnitPrice\": 400", ErpWriteError.InvalidDocumentCode)]
    [InlineData("sales_order", "\"quantity\": 2,", "\"quantity\": 1e20,", ErpWriteError.InvalidQuantityCode)]
    [InlineData("sales_return", "\"conditionPercent\": 0.3", "\"conditionPercent\": \"invalid\"", ErpWriteError.InvalidDiscountCode)]
    [InlineData("collection", "\"installments\": 3", "\"installments\": \"üç\"", ErpWriteError.InvalidDocumentCode)]
    [InlineData("collection", "\"surchargeAmount\": 45", "\"surchargeAmount\": \"kırk beş\"", ErpWriteError.InvalidAmountCode)]
    [InlineData("collection", "{ \"method\": \"cash\", \"amount\": 1000 }", "{ \"method\": \"cash\", \"amount\": 79000000000000000000000000000 }", ErpWriteError.InvalidAmountCode)]
    public void A_number_that_is_there_but_unusable_is_refused_not_defaulted(string documentType, string replace, string with, string code)
    {
        var (body, id) = documentType switch
        {
            "sales_order" => (Sale, "MOB-SO-1"),
            "sales_return" => (Return, "MOB-SR-1"),
            _ => (Collection, "MOB-TH-1"),
        };
        body.Should().Contain(replace);

        _sut.Translate(documentType, id, body.Replace(replace, with), Context()).Error!.Code.Should().Be(code);
    }

    [Fact]
    public void A_body_naming_another_document_than_the_job_is_refused()
    {
        _sut.Translate("sales_order", "MOB-SO-2", Sale, Context()).Error!.Code.Should().Be(ErpWriteError.DocumentIdMismatchCode);
        _sut.Translate("sales_order", "mob-so-1", Sale, Context()).Error!.Code.Should().Be(ErpWriteError.DocumentIdMismatchCode, "keys compare exactly");
        _sut.Translate("sales_order", "MOB-SO-1", Sale.Replace("\"mobileDocumentId\": \"MOB-SO-1\", ", ""), Context()).Error!.Code
            .Should().Be(ErpWriteError.DocumentIdMismatchCode);
    }

    [Theory]
    [InlineData("sales_order", "MOB-SO-1", """{ "mobileDocumentId": "MOB-SO-1", "priceListNo": 1, "lines": [null] }""")]
    [InlineData("sales_order", "MOB-SO-1", """{ "mobileDocumentId": "MOB-SO-1", "priceListNo": 1, "lines": [1, "x"] }""")]
    [InlineData("sales_order", "MOB-SO-1", """{ "mobileDocumentId": "MOB-SO-1", "priceListNo": 1, "lines": {} }""")]
    [InlineData("sales_return", "MOB-SR-1", """{ "mobileDocumentId": "MOB-SR-1", "lines": [[]] }""")]
    [InlineData("collection", "MOB-TH-1", """{ "mobileDocumentId": "MOB-TH-1", "payments": [null] }""")]
    [InlineData("collection", "MOB-TH-1", "[]")]
    [InlineData("collection", "MOB-TH-1", "not json")]
    public void A_malformed_body_is_a_permanent_error_not_a_crash(string documentType, string externalId, string body)
    {
        _sut.Translate(documentType, externalId, body, Context()).Error!.Code.Should().Be(ErpWriteError.InvalidDocumentCode);
    }

    [Fact]
    public void A_sale_whose_payments_are_not_objects_is_malformed()
    {
        var body = Sale.Replace("\"paymentType\": \"Cari Borç\",", "\"paymentType\": \"Cari Borç\", \"payments\": [null],");

        _sut.Translate("sales_order", "MOB-SO-1", body, Context()).Error!.Code.Should().Be(ErpWriteError.InvalidDocumentCode);
    }

    [Fact]
    public void Only_bodies_with_a_mobile_document_id_are_phone_documents()
    {
        MobileDocumentTranslator.IsMobileDocument(Sale).Should().BeTrue();
        MobileDocumentTranslator.IsMobileDocument("""{ "externalId": "x", "customerCode": "120.001" }""").Should().BeFalse();
        MobileDocumentTranslator.IsMobileDocument("not json").Should().BeFalse();
        MobileDocumentTranslator.IsMobileDocument(null).Should().BeFalse();
    }
}
