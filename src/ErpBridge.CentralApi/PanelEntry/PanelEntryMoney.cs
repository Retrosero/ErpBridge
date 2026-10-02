using System.Globalization;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>
/// The money documents the panel enters (GOAL_PANEL_GIRIS P3b–d), each the body Sipariş Cepte sends for it:
/// <list type="bullet">
/// <item>collection — an ERP company's one receipt with a line per method (<c>ErpCollectionDocument.payload</c>); a company
/// without an ERP's cash-book entry per method (<c>kasaLogPayload</c>, written together: all or none);</item>
/// <item>tediye — the cash book's <c>disbursement</c> entry, with the transfer's ERP bank code the phone does not send yet
/// (docs/mobil-belge-sozlesmesi.md: a bank named without its code is refused);</item>
/// <item>expense — an ERP company's <c>expense</c> on an ERP expense card with the VAT the user typed and its Mikro
/// pointer; a company without an ERP's customer-less <c>disbursement</c> "Gider: …", which is kept as a record only.</item>
/// </list>
/// </summary>
public static class PanelEntryMoney
{
    public const string CollectionType = "collection";
    public const string DisbursementType = "disbursement";
    public const string ExpenseType = "expense";

    /// <summary>The phone's expense categories for a company without an ERP (<c>ExpensesModule</c>).</summary>
    public static readonly IReadOnlyList<string> ExpenseCategories = ["Yemek", "Kırtasiye", "Kargo", "Yol/Yakıt", "Bakım", "Diğer"];

    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    // ---- collection -------------------------------------------------------------------------

    public static async Task<(PanelEntryPlan? Plan, PanelEntryInvalid? Invalid)> CollectionAsync(
        CentralApiDbContext db, IMemoryCache cache, PanelEntryCaller caller, MobileUser owner, PortalEntryCollectionRequest body,
        string baseKey, DateTimeOffset now, CancellationToken ct)
    {
        var (wall, dateError) = PanelEntryDates.Resolve(body.Date, now);
        if (dateError is not null) return (null, new PanelEntryInvalid("INVALID_DOCUMENT_DATE", dateError));
        var (customer, unknown) = await CustomerAsync(db, cache, caller, body.CustomerCode, ct);
        if (unknown is not null) return (null, unknown);
        if (body.Payments.Count == 0) return (null, PanelEntryInvalid.Body("En az bir ödeme girin."));
        if (body.Payments.Count > 10) return (null, PanelEntryInvalid.Body("Bir tahsilatta en çok 10 ödeme olabilir."));
        IReadOnlyList<PortalErpLookupItem> banks = caller.IsNative ? [] : await PanelEntryLookups.OfAsync(db, caller.Tenant.Id, "bank", ct);

        var payments = new List<Payment>();
        for (var i = 0; i < body.Payments.Count; i++)
        {
            var (payment, invalid) = ReadPayment(body.Payments[i], i + 1, caller.IsNative, banks);
            if (invalid is not null) return (null, invalid);
            payments.Add(payment!);
        }

        var occurredAt = PanelEntryDates.Local(wall!.Value);
        var total = CustomerCatalog.CatalogPricing.R2(payments.Sum(p => p.Amount));
        var description = Blank(body.Description);
        List<PanelEntryDocument> documents;
        if (!caller.IsNative)
        {
            var lines = new JsonArray();
            foreach (var payment in payments) lines.Add(ErpPaymentJson(payment));
            var payload = new JsonObject
            {
                ["mobileDocumentId"] = baseKey,
                ["revision"] = 1,
                ["occurredAt"] = occurredAt,
                ["customerCode"] = customer!.Code,
                ["counterparty"] = customer.Title,
                // The principal: a card's bank surcharge is not the customer's debt.
                ["amount"] = total,
                ["currency"] = "TL",
            };
            if (description is not null) payload["description"] = description;
            payload["transactionType"] = "Tahsilat";
            payload["paymentType"] = payments.Count == 1 ? payments[0].ErpLabel : "Çoklu Tahsilat";
            payload["payments"] = lines;
            documents = [new PanelEntryDocument(CollectionType, baseKey, payload.ToJsonString())];
        }
        else
        {
            var summary = description ?? "Tahsilat";
            documents = [.. payments.Select((payment, index) =>
            {
                var key = $"{baseKey}-{index + 1}";
                var payload = CashBook(key, occurredAt, "Tahsilat", customer!.Title, customer.Code, payment.Amount, payment.CashBookLabel,
                    payment.BankName, $"{payment.CashBookLabel} Tahsilat{(Detail(payment) is { } detail ? $" ({detail})" : string.Empty)} - {summary}");
                return new PanelEntryDocument(CollectionType, key, payload.ToJsonString());
            })];
        }

        var preview = Preview(PanelEntryKinds.Collection, caller, owner, occurredAt, total, customer);
        preview.Payments = [.. payments.Select(p => new PortalEntryPaymentDto { Method = p.Method, Label = p.ErpLabel, Amount = p.Amount, Detail = Detail(p) })];
        preview.Refusal = PanelEntryChecks.Permission(caller.Permissions, documents) ?? await PanelEntryChecks.ErpAsync(db, caller, owner, documents, ct);
        return (new PanelEntryPlan(preview, documents, $"Tahsilat: {Money(total)} TL — {customer!.Code} {customer.Title}"), null);
    }

    // ---- tediye -----------------------------------------------------------------------------

    public static async Task<(PanelEntryPlan? Plan, PanelEntryInvalid? Invalid)> DisbursementAsync(
        CentralApiDbContext db, IMemoryCache cache, PanelEntryCaller caller, MobileUser owner, PortalEntryDisbursementRequest body,
        string key, DateTimeOffset now, CancellationToken ct)
    {
        var (wall, dateError) = PanelEntryDates.Resolve(body.Date, now);
        if (dateError is not null) return (null, new PanelEntryInvalid("INVALID_DOCUMENT_DATE", dateError));
        var (customer, unknown) = await CustomerAsync(db, cache, caller, body.CustomerCode, ct);
        if (unknown is not null) return (null, unknown);
        if (body.Amount <= 0) return (null, PanelEntryInvalid.Body("Tutar sıfırdan büyük olmalı."));
        var paymentType = body.PaymentType?.Trim() switch
        {
            null or "" or "Nakit" => "Nakit",
            "EFT / Havale" or "Havale / EFT" or "Havale" or "EFT" => "EFT / Havale",
            _ => null,
        };
        if (paymentType is null) return (null, PanelEntryInvalid.Body("Ödeme şekli Nakit ya da EFT / Havale olmalı."));

        string? bankCode = null, bankName = null;
        if (paymentType != "Nakit")
        {
            if (caller.IsNative) bankName = Blank(body.BankName);
            else
            {
                var (code, name, invalid) = await BankAsync(db, caller, body.BankCode, ct);
                if (invalid is not null) return (null, invalid);
                (bankCode, bankName) = (code, name);
            }
        }

        var occurredAt = PanelEntryDates.Local(wall!.Value);
        var amount = CustomerCatalog.CatalogPricing.R2(body.Amount);
        var payload = CashBook(key, occurredAt, "Tediye", customer!.Title, customer.Code, amount, paymentType, bankName, Blank(body.Description) ?? "Tediye");
        // The phone names the bank only; the ERP needs its code to know which account pays (contract: tediye).
        if (bankCode is not null) payload["bankCode"] = bankCode;
        var documents = new List<PanelEntryDocument> { new(DisbursementType, key, payload.ToJsonString()) };

        var preview = Preview(PanelEntryKinds.Disbursement, caller, owner, occurredAt, amount, customer);
        preview.Payments = [new PortalEntryPaymentDto { Method = paymentType == "Nakit" ? "cash" : "transfer", Label = paymentType, Amount = amount, Detail = bankName }];
        preview.Refusal = PanelEntryChecks.Permission(caller.Permissions, documents) ?? await PanelEntryChecks.ErpAsync(db, caller, owner, documents, ct);
        return (new PanelEntryPlan(preview, documents, $"Tediye: {Money(amount)} TL — {customer.Code} {customer.Title}"), null);
    }

    // ---- expense ----------------------------------------------------------------------------

    public static async Task<(PanelEntryPlan? Plan, PanelEntryInvalid? Invalid)> ExpenseAsync(
        CentralApiDbContext db, PanelEntryCaller caller, MobileUser owner, PortalEntryExpenseRequest body,
        string key, DateTimeOffset now, CancellationToken ct)
    {
        var (wall, dateError) = PanelEntryDates.Resolve(body.Date, now);
        if (dateError is not null) return (null, new PanelEntryInvalid("INVALID_DOCUMENT_DATE", dateError));
        if (body.Amount <= 0) return (null, PanelEntryInvalid.Body("Tutar sıfırdan büyük olmalı."));
        if (Blank(body.Description) is not { } description) return (null, PanelEntryInvalid.Body("Açıklama girin."));
        var paymentType = body.PaymentType?.Trim() switch
        {
            null or "" or "Nakit" => "Nakit",
            "Banka" or "Havale" or "EFT / Havale" or "Havale / EFT" => "Banka",
            "Kredi Kartı" => "Kredi Kartı",
            _ => null,
        };
        if (paymentType is null) return (null, PanelEntryInvalid.Body("Ödeme şekli Nakit, Banka ya da Kredi Kartı olmalı."));
        var occurredAt = PanelEntryDates.Local(wall!.Value);
        var amount = CustomerCatalog.CatalogPricing.R2(body.Amount);

        PanelEntryDocument document;
        string name;
        if (caller.IsNative)
        {
            if (Blank(body.Category) is not { } category || !ExpenseCategories.Contains(category))
                return (null, PanelEntryInvalid.Body($"Gider türü şunlardan biri olmalı: {string.Join(", ", ExpenseCategories)}."));
            name = category;
            // The phone sends a company without an ERP's expense as a customer-less cash-book entry: kept as a record.
            var payload = CashBook(key, occurredAt, "Tediye", $"Gider: {category}", customerCode: null, amount, paymentType, bankName: null, description);
            document = new PanelEntryDocument(DisbursementType, key, payload.ToJsonString());
        }
        else
        {
            var lookups = await PanelEntryLookups.AllAsync(db, caller.Tenant.Id, ct, "expense_card", "vat_rate", "cash", "bank");
            var cardCode = Blank(body.ExpenseCardCode);
            if (cardCode is null) return (null, PanelEntryInvalid.Body("Gider kartını seçin."));
            var card = lookups["expense_card"].FirstOrDefault(c => c.Code == cardCode);
            if (card is null && lookups["expense_card"].Count > 0) return (null, PanelEntryInvalid.Body($"'{cardCode}' kodlu gider kartı ERP'de yok."));
            name = string.IsNullOrWhiteSpace(card?.Name) ? cardCode : card!.Name;
            var vat = CustomerCatalog.CatalogPricing.R2(body.VatAmount);
            if (vat < 0 || vat > amount) return (null, PanelEntryInvalid.Body("KDV sıfırdan küçük ya da tutardan büyük olamaz."));
            if (vat > 0 && body.VatPointer is not (>= 1 and <= 10)) return (null, PanelEntryInvalid.Body("KDV oranını seçin."));
            if (vat > 0 && lookups["vat_rate"].Count > 0 && lookups["vat_rate"].All(r => r.Code != body.VatPointer!.Value.ToString(CultureInfo.InvariantCulture)))
                return (null, PanelEntryInvalid.Body("Seçilen KDV oranı ERP'de yok."));
            var accountCode = Blank(body.AccountCode);
            var accounts = lookups[paymentType == "Nakit" ? "cash" : "bank"];
            if (accountCode is not null && accounts.Count > 0 && accounts.All(a => a.Code != accountCode))
                return (null, PanelEntryInvalid.Body(paymentType == "Nakit" ? $"'{accountCode}' kodlu kasa ERP'de yok." : $"'{accountCode}' kodlu banka ERP'de yok."));

            var payload = CashBook(key, occurredAt, "Tediye", $"Gider: {name}", customerCode: null, amount, paymentType, bankName: null, description);
            payload["expenseCardCode"] = cardCode;
            payload["vatAmount"] = vat;
            if (vat > 0) payload["vatPointer"] = body.VatPointer;
            if (accountCode is not null) payload[paymentType == "Nakit" ? "cashCode" : "bankCode"] = accountCode;
            document = new PanelEntryDocument(ExpenseType, key, payload.ToJsonString());
        }

        var preview = Preview(PanelEntryKinds.Expense, caller, owner, occurredAt, amount, customer: null);
        preview.Vat = caller.IsNative ? 0m : CustomerCatalog.CatalogPricing.R2(body.VatAmount);
        preview.Payments = [new PortalEntryPaymentDto { Method = paymentType == "Nakit" ? "cash" : paymentType == "Banka" ? "transfer" : "card", Label = paymentType, Amount = amount, Detail = name }];
        preview.Refusal = PanelEntryChecks.Permission(caller.Permissions, [document]) ?? await PanelEntryChecks.ErpAsync(db, caller, owner, [document], ct);
        return (new PanelEntryPlan(preview, [document], $"Gider: {Money(amount)} TL — {name}"), null);
    }

    // ---- shared -----------------------------------------------------------------------------

    /// <summary>One method of a collection, read and checked.</summary>
    private sealed record Payment(string Method, string ErpLabel, string CashBookLabel, decimal Amount, string? BankCode, string? BankName,
        int? Installments, decimal? Surcharge, string? Reference, string? DocumentNo, string? DueDate);

    private static (Payment? Payment, PanelEntryInvalid? Invalid) ReadPayment(PortalEntryPaymentRequest request, int no, bool native, IReadOnlyList<PortalErpLookupItem> banks)
    {
        var method = request.Method?.Trim().ToLowerInvariant();
        (string Erp, string CashBook)? labels = method switch
        {
            "cash" => ("Nakit", "Nakit"),
            "card" => ("Kredi Kartı", "Kredi Kartı"),
            "transfer" => ("Havale / EFT", "EFT / Havale"),
            "cheque" => ("Çek", "Çek"),
            "note" => ("Senet", "Senet"),
            _ => null,
        };
        if (labels is not { } label) return (null, PanelEntryInvalid.Body($"{no}. ödemenin şekli nakit, kart, havale, çek ya da senet olmalı."));
        if (request.Amount <= 0) return (null, PanelEntryInvalid.Body($"{no}. ödemenin tutarı sıfırdan büyük olmalı."));
        if (request.Installments is < 1 or > 36) return (null, PanelEntryInvalid.Body($"{no}. ödemenin taksit sayısı 1 ile 36 arasında olmalı."));
        if (request.SurchargeAmount is < 0) return (null, PanelEntryInvalid.Body($"{no}. ödemenin vade farkı eksi olamaz."));

        string? bankCode = null;
        var bankName = Blank(request.BankName);
        if (method is "card" or "transfer" && !native && Blank(request.BankCode) is { } code)
        {
            var bank = banks.FirstOrDefault(b => b.Code == code);
            if (bank is null && banks.Count > 0) return (null, PanelEntryInvalid.Body($"'{code}' kodlu banka ERP'de yok."));
            bankCode = code;
            bankName = bank?.Name ?? bankName;
        }

        string? documentNo = null, dueDate = null;
        if (method is "cheque" or "note")
        {
            documentNo = Blank(request.DocumentNo);
            if (documentNo is null || !DateOnly.TryParseExact(request.DueDate?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var due))
                return (null, PanelEntryInvalid.Body(method == "cheque"
                    ? "Çek için çek numarası ve vade tarihi gerekli."
                    : "Senet için seri numarası ve vade tarihi gerekli."));
            dueDate = due.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
        return (new Payment(method!, label.Erp, label.CashBook, CustomerCatalog.CatalogPricing.R2(request.Amount), bankCode, bankName,
            method == "card" ? request.Installments : null, method == "card" ? request.SurchargeAmount : null,
            Blank(request.Reference), documentNo, dueDate), null);
    }

    /// <summary>A line of the ERP receipt's <c>payments</c>, as <c>ErpCollectionDocument.paymentJson</c> writes it.</summary>
    private static JsonObject ErpPaymentJson(Payment payment)
    {
        var item = new JsonObject { ["method"] = payment.Method, ["amount"] = payment.Amount };
        switch (payment.Method)
        {
            case "card" or "transfer":
                if (payment.BankCode is not null) item["bankCode"] = payment.BankCode;
                if (payment.Method == "card")
                {
                    if (payment.Installments is > 1) item["installments"] = payment.Installments;
                    if (payment.Surcharge is > 0) item["surchargeAmount"] = payment.Surcharge;
                }
                break;
            case "cheque":
                item["dueDate"] = payment.DueDate;
                var cheque = new JsonObject { ["no"] = payment.DocumentNo };
                if (payment.BankName is not null) cheque["bankName"] = payment.BankName;
                item["cheque"] = cheque;
                break;
            case "note":
                item["dueDate"] = payment.DueDate;
                item["note"] = new JsonObject { ["no"] = payment.DocumentNo };
                break;
        }
        return item;
    }

    /// <summary>The cash book's document (<c>kasaLogPayload</c>): a tediye, an expense, a company without an ERP's collection.</summary>
    private static JsonObject CashBook(string key, string occurredAt, string transactionType, string counterparty, string? customerCode,
        decimal amount, string paymentType, string? bankName, string description)
    {
        var payload = new JsonObject
        {
            ["mobileDocumentId"] = key,
            ["revision"] = 1,
            ["occurredAt"] = occurredAt,
            ["transactionType"] = transactionType,
            ["counterparty"] = counterparty,
        };
        if (customerCode is not null) payload["customerCode"] = customerCode;
        payload["amount"] = amount;
        payload["paymentType"] = paymentType;
        if (bankName is not null) payload["bankName"] = bankName;
        payload["description"] = description;
        return payload;
    }

    /// <summary>What the phone writes next to a method: the bank, installments, slip, cheque/note number and due date.</summary>
    private static string? Detail(Payment payment)
    {
        var parts = new List<string>();
        if (payment.BankName is not null) parts.Add(payment.BankName);
        if (payment.Installments is > 1) parts.Add($"{payment.Installments} taksit");
        if (payment.Surcharge is > 0) parts.Add($"vade farkı {Money(payment.Surcharge.Value)} TL");
        if (payment.Reference is not null) parts.Add($"Ref: {payment.Reference}");
        if (payment.DocumentNo is not null) parts.Add($"No: {payment.DocumentNo}");
        if (payment.DueDate is not null) parts.Add($"Vade: {payment.DueDate}");
        return parts.Count == 0 ? null : string.Join(", ", parts);
    }

    private static async Task<(string? Code, string? Name, PanelEntryInvalid? Invalid)> BankAsync(CentralApiDbContext db, PanelEntryCaller caller, string? bankCode, CancellationToken ct)
    {
        if (Blank(bankCode) is not { } code) return (null, null, null);
        var banks = await PanelEntryLookups.OfAsync(db, caller.Tenant.Id, "bank", ct);
        var bank = banks.FirstOrDefault(b => b.Code == code);
        if (bank is null && banks.Count > 0) return (null, null, PanelEntryInvalid.Body($"'{code}' kodlu banka ERP'de yok."));
        return (code, string.IsNullOrWhiteSpace(bank?.Name) ? null : bank!.Name, null);
    }

    internal static async Task<(PortalLedger.Customer? Customer, PanelEntryInvalid? Invalid)> CustomerAsync(
        CentralApiDbContext db, IMemoryCache cache, PanelEntryCaller caller, string? code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return (null, PanelEntryInvalid.Body("Müşteri seçin."));
        var customers = await PortalLedger.CustomersAsync(db, cache, caller.Tenant.Id, caller.Tenant.DataSource, ct);
        return customers.TryGetValue(code.Trim(), out var customer)
            ? (customer, null)
            : (null, new PanelEntryInvalid("UNKNOWN_CUSTOMER", $"'{code.Trim()}' kodlu müşteri bulunamadı."));
    }

    private static PortalEntryPreviewResponse Preview(PanelEntryKind kind, PanelEntryCaller caller, MobileUser owner, string occurredAt,
        decimal total, PortalLedger.Customer? customer) => new()
        {
            Kind = kind.Name,
            DataSource = caller.Tenant.DataSource,
            OwnerUserId = owner.Id,
            OwnerName = PanelEntryAccess.NameOf(owner),
            CustomerCode = customer?.Code,
            CustomerName = customer?.Title,
            OccurredAt = occurredAt,
            Gross = total,
            Total = total,
        };

    private static string Money(decimal value) => value.ToString("#,0.00", Turkish);

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
