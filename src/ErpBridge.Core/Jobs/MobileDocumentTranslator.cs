using System.Globalization;
using System.Text.Json;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Shared;

namespace ErpBridge.Core.Jobs;

/// <summary>A phone document turned into an ERP command, or the reason it cannot be.</summary>
public sealed record MobileTranslation(
    SalesDocumentCommand? Sale = null,
    SalesReturnCommand? Return = null,
    CollectionCommand? Collection = null,
    DisbursementCommand? Disbursement = null,
    ErpWriteError? Error = null)
{
    public bool Ok => Error is null;

    public static MobileTranslation Fail(ErpWriteError error) => new(Error: error);
}

/// <summary>
/// Turns a Sipariş Cepte document body plus the job's <see cref="ErpWriteContext"/> into an
/// ERP-independent command (goal GOAL_ERP_YAZIM Y2b). The body contract is
/// <c>docs/mobil-belge-sozlesmesi.md</c>. Nothing here knows an ERP: amounts are the phone's,
/// codes come from the body or the company settings, and every ERP computation (VAT, discount
/// amounts, columns) belongs to the adapter.
///
/// <para>
/// Refusals follow two rules. A body from an older phone that lacks what the ERP needs (list
/// prices, structured payments) is <see cref="ErpWriteError.MobileAppUpdateRequiredCode"/> —
/// discounts are never reverse-engineered from a net price and cheque details are never parsed
/// out of free text. A missing company setting is <see cref="ErpWriteError.ErpMappingMissingCode"/>
/// naming the setting.
/// </para>
/// </summary>
public sealed class MobileDocumentTranslator
{
    public const string SalesOrderType = "sales_order";
    public const string SalesReturnType = "sales_return";
    public const string CollectionType = "collection";

    /// <summary>The phone's cash book calls money going out "Tediye"; a purchase's cash payment is one too.</summary>
    public const string DisbursementType = "disbursement";

    private const decimal AmountTolerance = 0.01m;
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly string[] LocalDateFormats = ["dd.MM.yyyy HH:mm", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy"];

    /// <summary>Whether a job's body is a Sipariş Cepte document (it carries <c>mobileDocumentId</c>).</summary>
    public static bool IsMobileDocument(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson)) return false;
        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("mobileDocumentId", out var id)
                && id.ValueKind == JsonValueKind.String;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public MobileTranslation Translate(string documentType, string externalId, string payloadJson, ErpWriteContext? context)
    {
        if (context is null) return MobileTranslation.Fail(ErpWriteError.ErpContextMissing());

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(payloadJson);
        }
        catch (JsonException)
        {
            return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
        }

        using var parsed = document;
        var body = parsed.RootElement;
        if (body.ValueKind != JsonValueKind.Object) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());

        // The job key is what ERP idempotency trusts; a body naming another document must not pass
        // under a fresh key as a second financial document (PR #81 Codex).
        if (!body.TryGetProperty("mobileDocumentId", out var id) || id.ValueKind != JsonValueKind.String
            || !string.Equals(id.GetString(), externalId, StringComparison.Ordinal))
            return MobileTranslation.Fail(ErpWriteError.DocumentIdMismatch());
        if (HasNonText(body, BodyTextFields)) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());

        return documentType.Trim().ToLowerInvariant() switch
        {
            SalesOrderType => TranslateSale(externalId, body, context),
            SalesReturnType => TranslateReturn(externalId, body, context),
            CollectionType => TranslateCollection(externalId, body, context),
            DisbursementType => TranslateDisbursement(externalId, body, context),
            _ => throw new ArgumentException($"'{documentType}' is not a phone document the ERP writes.", nameof(documentType)),
        };
    }

    // ---- sale ---------------------------------------------------------------------------------

    private static MobileTranslation TranslateSale(string externalId, JsonElement body, ErpWriteContext context)
    {
        if (Objects(body, "lines") is not { } lines) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
        if (!body.TryGetProperty("priceListNo", out _) || lines.Count == 0 || lines.Any(l => !l.TryGetProperty("listUnitPrice", out _)))
            return MobileTranslation.Fail(ErpWriteError.MobileAppUpdateRequired());

        if (Header(externalId, body, context, SeriesFor(context, out var kind), ZeroTotalAllowed) is not { } header)
            return MobileTranslation.Fail(HeaderError(body, context, ZeroTotalAllowed) ?? ErpWriteError.InvalidAmount());
        if (kind is null) return MobileTranslation.Fail(ErpWriteError.ErpMappingMissing("satış belge türü"));

        // A number the phone sends must be a real one: its prices came from that list, so a bad value
        // never silently falls back to the company's (PR #81 Codex).
        if (!OptionalNumber(body, "warehouseNo", out var phoneWarehouse) || !OptionalNumber(body, "priceListNo", out var priceList) || priceList is null)
            return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
        var warehouse = phoneWarehouse ?? context.WarehouseNo;
        if (warehouse is null) return MobileTranslation.Fail(ErpWriteError.ErpMappingMissing("depo"));

        var saleLines = new List<SalesDocumentLine>(lines.Count);
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (HasNonText(line, LineTextFields)) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
            var stockCode = Text(line, "productCode") ?? Text(line, "stockCode");
            if (stockCode is null) return MobileTranslation.Fail(ErpWriteError.MissingStockCode(i + 1));
            if (Decimal(line, "quantity") is not > 0) return MobileTranslation.Fail(ErpWriteError.InvalidQuantity(i + 1));
            if (MalformedNumber(line, "lineDiscountPercent") || MalformedNumber(line, "customerDiscountPercent") || MalformedNumber(line, "generalDiscountPercent"))
                return MobileTranslation.Fail(ErpWriteError.InvalidDiscount(i + 1));
            if (MalformedNumber(line, "unitPointer", integer: true) || Int(line, "unitPointer") is <= 0) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
            var discounts = new[] { Decimal(line, "lineDiscountPercent") ?? 0m, Decimal(line, "customerDiscountPercent") ?? 0m, Decimal(line, "generalDiscountPercent") ?? 0m };
            if (discounts.Any(d => d is < 0 or > 100)) return MobileTranslation.Fail(ErpWriteError.InvalidDiscount(i + 1));
            if (Decimal(line, "listUnitPrice") is not >= 0) return MobileTranslation.Fail(ErpWriteError.InvalidAmount());
            saleLines.Add(new SalesDocumentLine(
                stockCode,
                Decimal(line, "quantity")!.Value,
                Int(line, "unitPointer") ?? 1,
                Decimal(line, "listUnitPrice")!.Value,
                discounts[0], discounts[1], discounts[2],
                Text(line, "note")));
        }

        var approval = string.Equals(context.OrderApprovalMode, "pending", StringComparison.OrdinalIgnoreCase)
            ? OrderApprovalMode.Pending
            : OrderApprovalMode.Approved;

        // A split or part payment is written as a receipt next to an open document.
        if (Objects(body, "payments") is not { } payments) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
        if (payments.Count > 0)
        {
            var parsed = ParsePayments(payments, header.OccurredAt, context);
            if (parsed.Error is { } paymentError) return MobileTranslation.Fail(paymentError);
            if (parsed.Payments!.Sum(p => p.Amount) > header.ExpectedTotal + AmountTolerance) return MobileTranslation.Fail(ErpWriteError.InvalidAmount());
            return new MobileTranslation(Sale: new SalesDocumentCommand(
                header, kind.Value, warehouse.Value, priceList.Value, approval, SalesSettlement.Open, null, saleLines,
                parsed.Payments, context.Series.Collection, Delivery(header, context, kind.Value)));
        }

        var settlement = SaleSettlement(Text(body, "paymentType"));
        if (settlement is null) return MobileTranslation.Fail(ErpWriteError.UnsupportedPaymentType());
        if (settlement == SalesSettlement.Open || header.ExpectedTotal == 0m)
        {
            return new MobileTranslation(Sale: new SalesDocumentCommand(
                header, kind.Value, warehouse.Value, priceList.Value, approval, SalesSettlement.Open, null, saleLines,
                DeliveryDate: Delivery(header, context, kind.Value)));
        }

        var method = settlement switch
        {
            SalesSettlement.Cash => CollectionMethod.Cash,
            SalesSettlement.Card => CollectionMethod.Card,
            _ => CollectionMethod.Transfer,
        };
        var account = AccountFor(method, body, context);
        if (account.Error is { } accountError) return MobileTranslation.Fail(accountError);

        // Only an invoice can be closed to a cash box or bank (Mikro's kapalı fatura). An order or a
        // dispatch note paid on the spot stays open and gets a receipt for the money.
        if (kind != SalesDocumentKind.Invoice)
        {
            var payment = new CollectionPayment(method, header.ExpectedTotal, header.OccurredAt.Date, account.Code!);
            return new MobileTranslation(Sale: new SalesDocumentCommand(
                header, kind.Value, warehouse.Value, priceList.Value, approval, SalesSettlement.Open, null, saleLines,
                [payment], context.Series.Collection, Delivery(header, context, kind.Value)));
        }

        return new MobileTranslation(Sale: new SalesDocumentCommand(
            header, kind.Value, warehouse.Value, priceList.Value, approval, settlement.Value, account.Code, saleLines,
            DeliveryDate: Delivery(header, context, kind.Value)));
    }

    /// <summary>
    /// The company's delivery offset applied to the document day, for an order or dispatch note only;
    /// an invoice has no delivery date (PR #81 Codex). Null when the company sets none.
    /// </summary>
    private static DateTime? Delivery(ErpDocumentHeader header, ErpWriteContext context, SalesDocumentKind kind) =>
        kind is SalesDocumentKind.Order or SalesDocumentKind.Dispatch && context.DeliveryDayOffset is { } days
            ? header.OccurredAt.Date.AddDays(days)
            : null;

    private static string SeriesFor(ErpWriteContext context, out SalesDocumentKind? kind)
    {
        kind = context.SalesDocumentKind.Trim().ToLowerInvariant() switch
        {
            "order" => SalesDocumentKind.Order,
            "dispatch" => SalesDocumentKind.Dispatch,
            "invoice" => SalesDocumentKind.Invoice,
            _ => null,
        };
        return kind switch
        {
            SalesDocumentKind.Dispatch => context.Series.Dispatch,
            SalesDocumentKind.Invoice => context.Series.Invoice,
            _ => context.Series.Order,
        };
    }

    private static SalesSettlement? SaleSettlement(string? paymentType) => Normalize(paymentType) switch
    {
        "" or "cari borç" or "açık hesap" or "veresiye" or "cari" => SalesSettlement.Open,
        "nakit" => SalesSettlement.Cash,
        "kredi kartı" or "banka kartı" or "kart" or "pos" => SalesSettlement.Card,
        "eft / havale" or "havale / eft" or "havale" or "eft" => SalesSettlement.Transfer,
        _ => null,
    };

    // ---- return -------------------------------------------------------------------------------

    private static MobileTranslation TranslateReturn(string externalId, JsonElement body, ErpWriteContext context)
    {
        if (Objects(body, "lines") is not { } lines) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
        if (lines.Count == 0 || lines.Any(l => !l.TryGetProperty("listUnitPrice", out _)))
            return MobileTranslation.Fail(ErpWriteError.MobileAppUpdateRequired());

        if (Header(externalId, body, context, context.Series.Return, ZeroTotalAllowed) is not { } header)
            return MobileTranslation.Fail(HeaderError(body, context, ZeroTotalAllowed) ?? ErpWriteError.InvalidAmount());

        if (!OptionalNumber(body, "warehouseNo", out var phoneWarehouse) || !OptionalNumber(body, "priceListNo", out var phonePriceList))
            return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
        var warehouse = phoneWarehouse ?? context.WarehouseNo;
        if (warehouse is null) return MobileTranslation.Fail(ErpWriteError.ErpMappingMissing("depo"));
        var priceList = phonePriceList ?? context.PriceListNo;
        if (priceList is null) return MobileTranslation.Fail(ErpWriteError.ErpMappingMissing("fiyat listesi"));

        var returnLines = new List<SalesReturnLine>(lines.Count);
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (HasNonText(line, LineTextFields)) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
            var stockCode = Text(line, "productCode") ?? Text(line, "stockCode");
            if (stockCode is null) return MobileTranslation.Fail(ErpWriteError.MissingStockCode(i + 1));
            if (Decimal(line, "quantity") is not > 0) return MobileTranslation.Fail(ErpWriteError.InvalidQuantity(i + 1));
            if (Decimal(line, "listUnitPrice") is not >= 0) return MobileTranslation.Fail(ErpWriteError.InvalidAmount());
            // The phone sends the refunded share as 0..1; tolerate a percentage. Absent is a full refund,
            // a value that is not a number is refused rather than read as one (PR #81 Codex).
            if (MalformedNumber(line, "conditionPercent")) return MobileTranslation.Fail(ErpWriteError.InvalidDiscount(i + 1));
            if (MalformedNumber(line, "unitPointer", integer: true) || Int(line, "unitPointer") is <= 0) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
            var condition = Decimal(line, "conditionPercent") ?? 1m;
            if (condition > 1m) condition /= 100m;
            if (condition is < 0 or > 1) return MobileTranslation.Fail(ErpWriteError.InvalidDiscount(i + 1));
            returnLines.Add(new SalesReturnLine(stockCode, Decimal(line, "quantity")!.Value, Int(line, "unitPointer") ?? 1,
                Decimal(line, "listUnitPrice")!.Value, condition, Text(line, "reason")));
        }

        var settlementText = Text(body, "settlementMethod") ?? Text(body, "paymentType");
        var settlementKey = Normalize(settlementText);
        // A return refunding nothing still returns the goods but pays nothing out (PR #81 Codex).
        if (header.ExpectedTotal == 0m && ReturnSettlementKeys.Contains(settlementKey)) settlementKey = string.Empty;
        switch (settlementKey)
        {
            case "" or "cari alacak" or "açık hesap" or "cari":
                return new MobileTranslation(Return: new SalesReturnCommand(header, warehouse.Value, priceList.Value, ReturnSettlement.Open, null, returnLines));
            case "nakit":
                var cash = AccountFor(CollectionMethod.Cash, body, context);
                return cash.Error is { } cashError
                    ? MobileTranslation.Fail(cashError)
                    : new MobileTranslation(Return: new SalesReturnCommand(header, warehouse.Value, priceList.Value, ReturnSettlement.Cash, cash.Code, returnLines));
            case "banka iade" or "eft / havale" or "havale / eft" or "havale" or "banka":
                var bank = AccountFor(CollectionMethod.Transfer, body, context);
                return bank.Error is { } bankError
                    ? MobileTranslation.Fail(bankError)
                    : new MobileTranslation(Return: new SalesReturnCommand(header, warehouse.Value, priceList.Value, ReturnSettlement.Bank, bank.Code, returnLines));
            default:
                return MobileTranslation.Fail(ErpWriteError.UnsupportedPaymentType());
        }
    }

    private static readonly HashSet<string> ReturnSettlementKeys =
        ["", "cari alacak", "açık hesap", "cari", "nakit", "banka iade", "eft / havale", "havale / eft", "havale", "banka"];

    // ---- collection ---------------------------------------------------------------------------

    private static MobileTranslation TranslateCollection(string externalId, JsonElement body, ErpWriteContext context)
    {
        if (Objects(body, "payments") is not { } payments) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
        if (payments.Count == 0) return MobileTranslation.Fail(ErpWriteError.MobileAppUpdateRequired());

        if (Header(externalId, body, context, context.Series.Collection, zeroTotalAllowed: false) is not { } header)
            return MobileTranslation.Fail(HeaderError(body, context, zeroTotalAllowed: false) ?? ErpWriteError.InvalidAmount());

        var parsed = ParsePayments(payments, header.OccurredAt, context);
        if (parsed.Error is { } error) return MobileTranslation.Fail(error);
        if (Math.Abs(parsed.Payments!.Sum(p => p.Amount) - header.ExpectedTotal) > AmountTolerance)
            return MobileTranslation.Fail(ErpWriteError.InvalidAmount());

        return new MobileTranslation(Collection: new CollectionCommand(header, parsed.Payments!));
    }

    // ---- disbursement -------------------------------------------------------------------------

    /// <summary>
    /// The phone's cash-book entry for money paid out (ERP yazım 2). Unlike a collection this body carries a
    /// single payment and no <c>payments[]</c> array: it is the cash book's own row, which is also what a
    /// purchase's cash payment produces. Series: the collection's, until a disbursement series exists (Z1c).
    /// </summary>
    private static MobileTranslation TranslateDisbursement(string externalId, JsonElement body, ErpWriteContext context)
    {
        if (HasNonText(body, PaymentTextFields)) return MobileTranslation.Fail(ErpWriteError.InvalidDocument());
        if (Header(externalId, body, context, context.Series.Collection, zeroTotalAllowed: false) is not { } header)
            return MobileTranslation.Fail(HeaderError(body, context, zeroTotalAllowed: false) ?? ErpWriteError.InvalidAmount());

        // Cash and transfer are this goal's scope (D3). A cheque or note issued from the portfolio is a
        // different Mikro document, so it is refused by name rather than written as something it is not.
        var methodText = Text(body, "paymentType") ?? Text(body, "method");
        DisbursementMethod? method = Normalize(methodText) switch
        {
            "" or "cash" or "nakit" => DisbursementMethod.Cash,
            "transfer" or "havale" or "havale / eft" or "eft / havale" or "eft" or "banka" => DisbursementMethod.Transfer,
            _ => null,
        };
        if (method is null) return MobileTranslation.Fail(ErpWriteError.UnsupportedPaymentType());

        // The phone of today names the bank it chose only as `bankName`, a display name (PR #141 Codex). Falling
        // back to the company's default bank would post the money to an account nobody picked and say nothing,
        // so a named bank without its ERP code is refused until the phone sends `bankCode` (Z4c).
        if (method == DisbursementMethod.Transfer
            && Text(body, "bankCode") is null
            && !string.IsNullOrWhiteSpace(Text(body, "bankName")))
            return MobileTranslation.Fail(ErpWriteError.MobileAppUpdateRequired());

        var (picked, fallback, missing) = method == DisbursementMethod.Cash
            ? (Text(body, "cashCode"), context.CashCode, "kasa kodu")
            : (Text(body, "bankCode"), context.TransferBankCode, "havale bankası");
        var account = picked ?? (string.IsNullOrWhiteSpace(fallback) ? null : fallback.Trim());
        if (account is null) return MobileTranslation.Fail(ErpWriteError.ErpMappingMissing(missing));

        return new MobileTranslation(Disbursement: new DisbursementCommand(header, method.Value, header.ExpectedTotal, account));
    }

    private static (IReadOnlyList<CollectionPayment>? Payments, ErpWriteError? Error) ParsePayments(
        IReadOnlyList<JsonElement> payments, DateTime occurredAt, ErpWriteContext context)
    {
        var result = new List<CollectionPayment>(payments.Count);
        foreach (var payment in payments)
        {
            if (HasNonText(payment, PaymentTextFields)
                || (payment.TryGetProperty("cheque", out var chequeBody) && chequeBody.ValueKind == JsonValueKind.Object && HasNonText(chequeBody, DetailTextFields))
                || (payment.TryGetProperty("note", out var noteBody) && noteBody.ValueKind == JsonValueKind.Object && HasNonText(noteBody, DetailTextFields)))
                return (null, ErpWriteError.InvalidDocument());
            var methodText = Text(payment, "method");
            CollectionMethod? method = Normalize(methodText) switch
            {
                "cash" or "nakit" => CollectionMethod.Cash,
                "card" or "kredi kartı" or "banka kartı" => CollectionMethod.Card,
                "transfer" or "havale / eft" or "eft / havale" or "havale" => CollectionMethod.Transfer,
                "cheque" or "çek" => CollectionMethod.Cheque,
                "note" or "senet" => CollectionMethod.Note,
                _ => null,
            };
            if (method is null) return (null, ErpWriteError.UnsupportedPaymentType());
            if (Decimal(payment, "amount") is not > 0 || MalformedNumber(payment, "surchargeAmount")) return (null, ErpWriteError.InvalidAmount());
            if (MalformedNumber(payment, "installments", integer: true)) return (null, ErpWriteError.InvalidDocument());

            var account = AccountFor(method.Value, payment, context);
            if (account.Error is { } accountError) return (null, accountError);

            ChequeDetails? cheque = null;
            NoteDetails? note = null;
            var dueDate = occurredAt.Date;
            if (method is CollectionMethod.Cheque or CollectionMethod.Note)
            {
                var details = payment.TryGetProperty(method == CollectionMethod.Cheque ? "cheque" : "note", out var d) && d.ValueKind == JsonValueKind.Object ? d : (JsonElement?)null;
                var number = details is { } dd ? Text(dd, "no") : null;
                var due = ParseDate(Text(payment, "dueDate") ?? (details is { } dv ? Text(dv, "dueDate") : null));
                if (number is null || due is null)
                    return (null, method == CollectionMethod.Cheque ? ErpWriteError.MissingChequeDetails() : ErpWriteError.MissingNoteDetails());
                dueDate = due.Value.Date;
                if (method == CollectionMethod.Cheque)
                    cheque = new ChequeDetails(number, Text(details!.Value, "bankName"), Text(details.Value, "branch"), Text(details.Value, "accountNo"), Text(details.Value, "drawer"));
                else
                    note = new NoteDetails(number, Text(details!.Value, "debtor"));
            }

            result.Add(new CollectionPayment(
                method.Value, Decimal(payment, "amount")!.Value, dueDate, account.Code!,
                Int(payment, "installments"), Decimal(payment, "surchargeAmount"), cheque, note));
        }

        return (result, null);
    }

    // ---- shared -------------------------------------------------------------------------------

    /// <summary>The ERP account a payment lands in: the one the phone picked, else the company's.</summary>
    private static (string? Code, ErpWriteError? Error) AccountFor(CollectionMethod method, JsonElement source, ErpWriteContext context)
    {
        var (picked, fallback, missing) = method switch
        {
            CollectionMethod.Cash => (Text(source, "cashCode"), context.CashCode, "kasa kodu"),
            CollectionMethod.Card => (Text(source, "bankCode"), context.CardBankCode, "kart bankası"),
            CollectionMethod.Transfer => (Text(source, "bankCode"), context.TransferBankCode, "havale bankası"),
            CollectionMethod.Cheque => (null, context.ChequePortfolioCode, "çek portföy kasası"),
            _ => (null, context.NotePortfolioCode, "senet portföy kasası"),
        };
        var code = picked ?? (string.IsNullOrWhiteSpace(fallback) ? null : fallback.Trim());
        return code is null ? (null, ErpWriteError.ErpMappingMissing(missing)) : (code, null);
    }

    /// <summary>
    /// A sale or return may total zero (full discount, a return refunding nothing) and still moves stock;
    /// a collection of nothing is not a document (PR #81 Codex).
    /// </summary>
    private const bool ZeroTotalAllowed = true;

    private static ErpDocumentHeader? Header(string externalId, JsonElement body, ErpWriteContext context, string series, bool zeroTotalAllowed)
    {
        var customer = Text(body, "customerCode");
        var occurredAt = ParseDate(Text(body, "occurredAt"));
        var amount = Decimal(body, "amount");
        if (customer is null || occurredAt is null || context.ErpUserNo is null || amount is not { } total || !ValidTotal(total, zeroTotalAllowed) || !IsTurkishLira(body)) return null;
        return new ErpDocumentHeader(
            externalId,
            occurredAt.Value,
            customer,
            Text(body, "salespersonCode") ?? context.SalespersonCode,
            context.ErpUserNo.Value,
            series,
            Text(body, "description"),
            total,
            Blank(context.ResponsibilityCenterCode),
            Blank(context.ProjectCode));
    }

    /// <summary>Why <see cref="Header"/> returned null, in the order a person would fix it.</summary>
    private static ErpWriteError? HeaderError(JsonElement body, ErpWriteContext context, bool zeroTotalAllowed)
    {
        if (Text(body, "customerCode") is null) return ErpWriteError.MissingCustomerCode();
        if (!IsTurkishLira(body)) return ErpWriteError.UnsupportedCurrency();
        if (ParseDate(Text(body, "occurredAt")) is null) return ErpWriteError.InvalidDocumentDate();
        if (!ValidTotal(Decimal(body, "amount"), zeroTotalAllowed)) return ErpWriteError.InvalidAmount();
        if (context.ErpUserNo is null) return ErpWriteError.ErpMappingMissing("ERP kullanıcı numarası");
        return null;
    }

    private static bool ValidTotal(decimal? amount, bool zeroTotalAllowed) =>
        amount is > 0 || (zeroTotalAllowed && amount == 0m);

    /// <summary>
    /// An optional positive whole number: absent is fine (<paramref name="number"/> null); present but
    /// null, fractional, text or not above zero is a malformed body (<c>false</c>).
    /// </summary>
    private static bool OptionalNumber(JsonElement body, string name, out int? number)
    {
        number = null;
        if (!body.TryGetProperty(name, out _)) return true;
        number = Int(body, name);
        return number is > 0;
    }

    private static bool IsTurkishLira(JsonElement body) =>
        Normalize(Text(body, "currency")) is "" or "tl" or "try" or "₺";

    private static DateTime? ParseDate(string? value)
    {
        if (value is null) return null;
        if (DateTime.TryParseExact(value, LocalDateFormats, Turkish, DateTimeStyles.None, out var local)) return local;
        // ISO with an offset: keep the phone's wall-clock time, which is the document date.
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var withOffset)) return withOffset.DateTime;
        return null;
    }

    private static string? Blank(string? code) => string.IsNullOrWhiteSpace(code) ? null : code.Trim();

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLower(Turkish);

    private static string? Text(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            && value.GetString() is { } text && !string.IsNullOrWhiteSpace(text)
            ? text.Trim()
            : null;

    /// <summary>
    /// No phone document amount, quantity or rate comes near this; larger values are malformed, and
    /// keeping them out means sums of a document's values can never overflow (PR #81 Codex).
    /// </summary>
    private const decimal MaxNumber = 999_999_999_999m;

    private static decimal? Decimal(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value)) return null;
        decimal? number = value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var n) => n,
            JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => null,
        };
        return number is { } checkedNumber && checkedNumber >= -MaxNumber && checkedNumber <= MaxNumber ? checkedNumber : null;
    }

    // Text fields that choose where money goes or what it is (codes, currency, payment kind) must never
    // fall back to a default because the phone sent them with the wrong JSON type (PR #81 Codex).
    private static readonly string[] BodyTextFields =
        ["customerCode", "currency", "paymentType", "settlementMethod", "cashCode", "bankCode", "salespersonCode", "occurredAt", "description"];

    private static readonly string[] LineTextFields = ["productCode", "stockCode", "note", "reason"];

    private static readonly string[] PaymentTextFields = ["method", "cashCode", "bankCode", "dueDate"];

    private static readonly string[] DetailTextFields = ["no", "bankName", "branch", "accountNo", "drawer", "debtor", "dueDate"];

    /// <summary>Whether any of the fields is present with a JSON type other than string or null.</summary>
    private static bool HasNonText(JsonElement element, IEnumerable<string> fields) =>
        fields.Any(f => element.TryGetProperty(f, out var value) && value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null));

    /// <summary>An optional number that is there (and not JSON null) but is not a usable number.</summary>
    private static bool MalformedNumber(JsonElement element, string name, bool integer = false) =>
        element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null
        && (integer ? Int(element, name) is null : Decimal(element, name) is null);

    private static int? Int(JsonElement element, string name) =>
        Decimal(element, name) is { } number && number == Math.Truncate(number) && number is >= int.MinValue and <= int.MaxValue ? (int)number : null;

    /// <summary>
    /// The objects of an array property: empty when the property is absent (an older body), <c>null</c>
    /// when it is there but not an array of objects: a malformed body, never probed further (PR #81 Codex).
    /// </summary>
    private static IReadOnlyList<JsonElement>? Objects(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null) return [];
        if (value.ValueKind != JsonValueKind.Array) return null;
        var items = value.EnumerateArray().ToList();
        return items.All(i => i.ValueKind == JsonValueKind.Object) ? items : null;
    }
}
