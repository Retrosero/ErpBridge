using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Permissions;

/// <summary>
/// What a phone document says about the user's limits: its amount (gross when the payload carries it) and the highest
/// line and order discounts. A field the payload does not carry is unknown and checks nothing: an older phone's
/// document is never refused for a value it did not send.
/// </summary>
public sealed record DocumentLimitFacts(decimal? Amount, decimal? MaxLineDiscountPercent, decimal? MaxGeneralDiscountPercent)
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
            return new DocumentLimitFacts(amount, line, general);
        }
        catch (JsonException)
        {
            // Unreadable documents are refused later by their own validation; the limits check nothing here.
            return None;
        }
    }

    private static decimal? Number(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number) ? number : null;

    private static decimal? Max(decimal? a, decimal? b) => a is null ? b : b is null ? a : Math.Max(a.Value, b.Value);
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
    public static string? Refusal(EffectivePermissions permissions, string approvalKind, DocumentLimitFacts facts)
    {
        if (permissions.IsAdmin) return null;
        if (ModuleOf(approvalKind) is { } module && !permissions.Can(module))
            return $"Bu işlem için yetkiniz yok ({PermissionCatalog.Find(module)?.Label}); belge onaya gönderilmeli.";

        return approvalKind switch
        {
            ApprovalKinds.Sale =>
                Over(permissions, PermissionKeys.LimitSaleLineDiscountPct, facts.MaxLineDiscountPercent, "Satır iskontosu", "%")
                ?? Over(permissions, PermissionKeys.LimitSaleGeneralDiscountPct, facts.MaxGeneralDiscountPercent, "Sipariş iskontosu", "%")
                ?? Over(permissions, PermissionKeys.LimitSaleAmount, facts.Amount, "Satış tutarı", "TL"),
            ApprovalKinds.Return => Over(permissions, PermissionKeys.LimitReturnAmount, facts.Amount, "İade tutarı", "TL"),
            ApprovalKinds.Purchase => Over(permissions, PermissionKeys.LimitPurchaseAmount, facts.Amount, "Alış tutarı", "TL"),
            ApprovalKinds.Disbursement => Over(permissions, PermissionKeys.LimitDisbursementAmount, facts.Amount, "Tediye tutarı", "TL"),
            _ => null,
        };
    }

    private static string? Over(EffectivePermissions permissions, string key, decimal? actual, string what, string unit)
    {
        if (actual is not { } value || permissions.Limit(key) is not { } limit || value <= limit) return null;
        return unit == "%"
            ? $"{what} %{Show(value)}, sınır %{Show(limit)}; belge onaya gönderilmeli."
            : $"{what} {Show(value)} TL, onaysız sınır {Show(limit)} TL; belge onaya gönderilmeli.";
    }

    private static string Show(decimal value) => value.ToString("#,0.##", CultureInfo.GetCultureInfo("tr-TR"));
}
