using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Permissions;

/// <summary>
/// What a phone document says about the user's limits: its amount (gross when the payload carries it), the highest
/// line and order discounts, and whether a sale closes on the customer's account. A field the payload does not carry is
/// unknown and checks nothing: an older phone's document is never refused for a value it did not send.
/// </summary>
public sealed record DocumentLimitFacts(decimal? Amount, decimal? MaxLineDiscountPercent, decimal? MaxGeneralDiscountPercent, bool OnAccount = false)
{
    public static readonly DocumentLimitFacts None = new(null, null, null);

    public static DocumentLimitFacts Read(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson)) return None;
        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return None;
            var amount = Number(root, "grossAmount") ?? Number(root, "amount") ?? Number(root, "total");
            decimal? line = null, general = Number(root, "generalDiscountPercent");
            if (root.TryGetProperty("lines", out var lines) && lines.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in lines.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;
                    line = Max(line, Number(item, "lineDiscountPercent"));
                    general = Max(general, Number(item, "generalDiscountPercent"));
                }
            }
            return new DocumentLimitFacts(amount, line, general, BooksOnAccount(root));
        }
        catch (JsonException)
        {
            // Unreadable documents are refused later by their own validation; the limits check nothing here.
            return None;
        }
    }

    /// <summary>
    /// The mobile document contract: <c>paymentType</c> "Cari Borç" or empty books the sale on account, unless
    /// <c>payments</c> collects it in the same operation.
    /// </summary>
    private static bool BooksOnAccount(JsonElement root)
    {
        if (root.TryGetProperty("payments", out var payments) && payments.ValueKind == JsonValueKind.Array && payments.GetArrayLength() > 0)
            return false;
        var type = root.TryGetProperty("paymentType", out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()?.Trim() : null;
        return string.IsNullOrEmpty(type) || string.Equals(type, "Cari Borç", StringComparison.OrdinalIgnoreCase);
    }

    private static decimal? Number(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number) ? number : null;

    private static decimal? Max(decimal? a, decimal? b) => a is null ? b : b is null ? a : Math.Max(a.Value, b.Value);
}

/// <summary>What stands in the way of a document: the module of its kind, the open-account right, or one of the limits.</summary>
public enum PermissionViolationKind
{
    Module,
    OpenAccount,
    Limit,
}

/// <summary>
/// The first permission a document goes over (<see cref="DocumentPermissionCheck.Check"/>): the key, and for a limit what
/// was asked and the ceiling. The phone's ingest turns it into an approval request (<see cref="ApprovalMessage"/>); the
/// panel, which has no approval path for its own entries (GOAL_PANEL_GIRIS K2/K4), refuses with <see cref="Message"/>.
/// </summary>
public sealed record PermissionViolation(PermissionViolationKind Kind, string Key, string Subject, decimal? Actual = null, decimal? Ceiling = null, bool Percent = false)
{
    /// <summary>The refusal in plain words, without sending anyone to the approval queue.</summary>
    public string Message => Kind switch
    {
        PermissionViolationKind.Module => $"Bu işlem için yetkiniz yok ({Subject}).",
        PermissionViolationKind.OpenAccount => "Açık hesap (veresiye) satış yetkiniz yok.",
        _ => Percent
            ? $"{Subject} %{Show(Actual!.Value)}, sınırınız %{Show(Ceiling!.Value)}."
            : $"{Subject} {Show(Actual!.Value)} TL, sınırınız {Show(Ceiling!.Value)} TL.",
    };

    /// <summary>The ingest's text (409 <c>APPROVAL_REQUIRED</c>): the phone sends the document for approval instead.</summary>
    public string ApprovalMessage => Kind switch
    {
        PermissionViolationKind.Module => $"Bu işlem için yetkiniz yok ({Subject}); belge onaya gönderilmeli.",
        PermissionViolationKind.OpenAccount => "Açık hesap (veresiye) satış yetkiniz yok; belge onaya gönderilmeli.",
        _ => Percent
            ? $"{Subject} %{Show(Actual!.Value)}, sınır %{Show(Ceiling!.Value)}; belge onaya gönderilmeli."
            : $"{Subject} {Show(Actual!.Value)} TL, onaysız sınır {Show(Ceiling!.Value)} TL; belge onaya gönderilmeli.",
    };

    private static string Show(decimal value) => value.ToString("#,0.##", CultureInfo.GetCultureInfo("tr-TR"));
}

/// <summary>
/// The sender's permissions over a phone document (GOAL_YETKILER): the module of its kind, and the user's limits.
/// A refusal is not a rejection: the ingest answers <c>409 APPROVAL_REQUIRED</c>, which every phone version turns into an
/// approval request, so nothing entered in the field is lost and an approver decides.
/// </summary>
public static class DocumentPermissionCheck
{
    public static string? ModuleOf(string approvalKind) => approvalKind switch
    {
        ApprovalKinds.Sale => PermissionKeys.ModuleSales,
        ApprovalKinds.Return => PermissionKeys.ModuleReturns,
        ApprovalKinds.Purchase => PermissionKeys.ModulePurchase,
        ApprovalKinds.Collection => PermissionKeys.ModuleCollection,
        ApprovalKinds.Disbursement => PermissionKeys.ModuleDisbursement,
        ApprovalKinds.StockCount => PermissionKeys.ModuleCounting,
        _ => null,
    };

    /// <summary>Why the document needs approval, or null when the user may send it as it is.</summary>
    public static string? Refusal(EffectivePermissions permissions, string approvalKind, DocumentLimitFacts facts) =>
        Check(permissions, approvalKind, facts)?.ApprovalMessage;

    /// <summary>The first permission the document goes over, or null. Administrators have no limits.</summary>
    public static PermissionViolation? Check(EffectivePermissions permissions, string approvalKind, DocumentLimitFacts facts)
    {
        if (permissions.IsAdmin) return null;
        if (ModuleOf(approvalKind) is { } module && !permissions.Can(module))
            return new PermissionViolation(PermissionViolationKind.Module, module, PermissionCatalog.Find(module)?.Label ?? module);

        return approvalKind switch
        {
            ApprovalKinds.Sale =>
                (facts.OnAccount && !permissions.Can(PermissionKeys.SaleOpenAccount)
                    ? new PermissionViolation(PermissionViolationKind.OpenAccount, PermissionKeys.SaleOpenAccount, "Açık hesap")
                    : null)
                ?? Over(permissions, PermissionKeys.LimitSaleLineDiscountPct, facts.MaxLineDiscountPercent, "Satır iskontosu", percent: true)
                ?? Over(permissions, PermissionKeys.LimitSaleGeneralDiscountPct, facts.MaxGeneralDiscountPercent, "Sipariş iskontosu", percent: true)
                ?? Over(permissions, PermissionKeys.LimitSaleAmount, facts.Amount, "Satış tutarı", percent: false),
            ApprovalKinds.Return => Over(permissions, PermissionKeys.LimitReturnAmount, facts.Amount, "İade tutarı", percent: false),
            ApprovalKinds.Purchase => Over(permissions, PermissionKeys.LimitPurchaseAmount, facts.Amount, "Alış tutarı", percent: false),
            ApprovalKinds.Disbursement => Over(permissions, PermissionKeys.LimitDisbursementAmount, facts.Amount, "Tediye tutarı", percent: false),
            _ => null,
        };
    }

    private static PermissionViolation? Over(EffectivePermissions permissions, string key, decimal? actual, string what, bool percent)
    {
        if (actual is not { } value || permissions.Limit(key) is not { } limit || value <= limit) return null;
        return new PermissionViolation(PermissionViolationKind.Limit, key, what, value, limit, percent);
    }
}
