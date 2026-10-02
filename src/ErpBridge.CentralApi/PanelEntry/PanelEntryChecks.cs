using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.Shared;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>An entry ready to write: what the form shows, the documents it becomes and the audit line.</summary>
public sealed record PanelEntryPlan(PortalEntryPreviewResponse Preview, IReadOnlyList<PanelEntryDocument> Documents, string Summary);

/// <summary>
/// What stops a built entry (GOAL_PANEL_GIRIS K2/K4/K5): the user's own permissions and limits over each document — the
/// ingest's check, but a refusal here, since a panel entry has no approval path — and, for an ERP company, the agent's
/// translator over the owner's context. The company's approval rules are not asked.
/// </summary>
public static class PanelEntryChecks
{
    public const string ModuleDenied = "ENTRY_MODULE_DENIED";
    public const string LimitExceeded = "ENTRY_LIMIT_EXCEEDED";
    public const string NegativeStock = "ENTRY_NEGATIVE_STOCK";
    public const string MappingMissing = ErpWriteError.ErpMappingMissingCode;
    public const string DocumentInvalid = "ENTRY_DOCUMENT_INVALID";

    /// <summary>The first permission one of the documents goes over, as the form's refusal.</summary>
    public static PortalEntryRefusalDto? Permission(EffectivePermissions permissions, IEnumerable<PanelEntryDocument> documents)
    {
        foreach (var document in documents)
        {
            if (ApprovalKinds.ForDocument(document.DocumentType, document.PayloadJson) is not { } kind) continue;
            if (DocumentPermissionCheck.Check(permissions, kind, DocumentLimitFacts.Read(document.PayloadJson)) is not { } violation) continue;
            return new PortalEntryRefusalDto
            {
                Code = violation.Kind == PermissionViolationKind.Limit ? LimitExceeded : ModuleDenied,
                Message = violation.Message,
                Key = violation.Key,
            };
        }
        return null;
    }

    /// <summary>For an ERP company: why the agent would not write one of the documents for <paramref name="owner"/>.</summary>
    public static async Task<PortalEntryRefusalDto?> ErpAsync(CentralApiDbContext db, PanelEntryCaller caller, MobileUser owner,
        IEnumerable<PanelEntryDocument> documents, CancellationToken ct)
    {
        if (caller.IsNative) return null;
        var context = await PanelEntryErp.ContextAsync(db, caller.Tenant.Id, owner, ct);
        foreach (var document in documents)
        {
            if (PanelEntryErp.Refusal(document.DocumentType, document.ExternalId, document.PayloadJson, context) is not { } error) continue;
            return error.Code == ErpWriteError.ErpMappingMissingCode
                ? new PortalEntryRefusalDto { Code = MappingMissing, Message = $"Belge {PanelEntryAccess.NameOf(owner)} adına yazılacak. {error.Message}" }
                : new PortalEntryRefusalDto { Code = DocumentInvalid, Message = error.Message };
        }
        return null;
    }

    /// <summary>The HTTP status a save answers a refusal with.</summary>
    public static int StatusOf(string code) => code switch
    {
        ModuleDenied => StatusCodes.Status403Forbidden,
        DocumentInvalid => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status409Conflict,
    };
}
