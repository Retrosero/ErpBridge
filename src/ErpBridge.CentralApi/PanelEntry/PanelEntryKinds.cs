using ErpBridge.CentralApi.Permissions;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>
/// One kind of document the panel enters (GOAL_PANEL_GIRIS): the phone module that opens its page and the key prefix of
/// its jobs. The key is <c>{Prefix}{operationId}</c>, so a retried save finds the job its first try wrote.
/// </summary>
/// <param name="Name">The route segment and the name the panel uses.</param>
public sealed record PanelEntryKind(string Name, string ModuleKey, string Prefix, string Label);

public static class PanelEntryKinds
{
    public static readonly PanelEntryKind Sale = new("sale", PermissionKeys.ModuleSales, "PNL-SO-", "Satış");
    public static readonly PanelEntryKind Collection = new("collection", PermissionKeys.ModuleCollection, "PNL-TH-", "Tahsilat");
    public static readonly PanelEntryKind Purchase = new("purchase", PermissionKeys.ModulePurchase, "PNL-PR-", "Alış");
    public static readonly PanelEntryKind Return = new("return", PermissionKeys.ModuleReturns, "PNL-SR-", "İade");
    public static readonly PanelEntryKind Disbursement = new("disbursement", PermissionKeys.ModuleDisbursement, "PNL-TD-", "Tediye");
    public static readonly PanelEntryKind Expense = new("expense", PermissionKeys.ModuleExpenses, "PNL-GD-", "Gider");

    public static readonly IReadOnlyList<PanelEntryKind> All = [Sale, Collection, Purchase, Return, Disbursement, Expense];

    /// <summary>The kind whose prefix a panel entry's key carries; null for any other key.</summary>
    public static PanelEntryKind? OfKey(string? externalId) =>
        externalId is null ? null : All.FirstOrDefault(k => externalId.StartsWith(k.Prefix, StringComparison.Ordinal));

    /// <summary>The document key of one save attempt; null when <paramref name="operationId"/> is not a GUID.</summary>
    public static string? ExternalId(PanelEntryKind kind, string? operationId) =>
        Guid.TryParse(operationId, out var id) ? kind.Prefix + id.ToString("D") : null;
}
