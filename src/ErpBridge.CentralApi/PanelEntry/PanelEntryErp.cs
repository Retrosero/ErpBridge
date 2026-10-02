using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.ErpWrite;
using ErpBridge.Core.Jobs;
using ErpBridge.Shared;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>
/// What the ERP agent will make of a panel entry, asked before the job is written (GOAL_PANEL_GIRIS K5): the agent's own
/// translator over the owner's context — the one the job will be leased with (<see cref="ErpWriteContextBuilder"/>) —
/// so a missing mapping (no ERP user number, warehouse, kind, cash account …) stops the form instead of failing in the
/// queue, and a body the server built wrong never reaches Mikro.
/// </summary>
public static class PanelEntryErp
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    /// <summary>The context the agent will lease the owner's job with, as the agent reads it.</summary>
    public static async Task<ErpWriteContext> ContextAsync(CentralApiDbContext db, Guid tenantId, MobileUser owner, CancellationToken ct)
    {
        var settings = await db.ErpWriteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        var mapping = await db.MobileUserErpMappings.AsNoTracking().FirstOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == owner.Id, ct);
        var leased = ErpWriteContextBuilder.Build(settings, mapping, owner.Username);
        return JsonSerializer.Deserialize<ErpWriteContext>(JsonSerializer.Serialize(leased, Web), Web)!;
    }

    /// <summary>Why the agent would refuse the document, or null when it would write it.</summary>
    public static ErpWriteError? Refusal(string documentType, string externalId, string payloadJson, ErpWriteContext context) =>
        new MobileDocumentTranslator().Translate(documentType, externalId, payloadJson, context).Error;
}
