using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Endpoints;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>A refusal of the request itself (a field missing or wrong), answered 400 before anything is priced.</summary>
public sealed record PanelEntryInvalid(string Code, string Message)
{
    public const string BodyCode = "ENTRY_INVALID";

    public static PanelEntryInvalid Body(string message) => new(BodyCode, message);
}

/// <summary>
/// The ERP's codes an entry may carry — warehouses, cash accounts, banks — read from the agent's lookups the way the
/// ERP settings page reads them (<see cref="PortalErpWriteEndpoints"/>), for any user who may enter documents.
/// </summary>
public static class PanelEntryLookups
{
    public static async Task<IReadOnlyList<PortalErpLookupItem>> OfAsync(CentralApiDbContext db, Guid tenantId, string kind, CancellationToken ct) =>
        PortalErpWriteEndpoints.Sorted(await PortalErpWriteEndpoints.ReadLookupsAsync(db, tenantId, ct), kind);

    public static async Task<Dictionary<string, IReadOnlyList<PortalErpLookupItem>>> AllAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct, params string[] kinds)
    {
        var byKind = await PortalErpWriteEndpoints.ReadLookupsAsync(db, tenantId, ct);
        return kinds.ToDictionary(k => k, k => PortalErpWriteEndpoints.Sorted(byKind, k), StringComparer.Ordinal);
    }
}
