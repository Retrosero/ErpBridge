using System.Security.Claims;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Writes a company's <c>tenant_modules</c> set for the two admin endpoints that own a part of it:
/// phone add-ons (<c>/admin/tenants/{id}/mobile/modules</c>) and Go modules
/// (<c>/admin/licenses/{id}/go-modules</c>). Each endpoint replaces only its own part, so saving
/// one never switches the other product's modules off.
/// </summary>
internal static class TenantModuleSets
{
    /// <summary>Trimmed, lowercase, de-duplicated keys as the admin sent them.</summary>
    public static List<string> Normalize(IEnumerable<string?> modules) =>
        modules.Select(m => m?.Trim().ToLowerInvariant() ?? string.Empty).Distinct(StringComparer.Ordinal).ToList();

    /// <summary>
    /// Makes the tenant's modules inside <paramref name="scope"/> exactly <paramref name="wanted"/>;
    /// rows outside the scope are untouched. Keys already on keep their original enable date and
    /// operator. Does not save.
    /// </summary>
    public static async Task ReplaceAsync(
        CentralApiDbContext db,
        HttpContext http,
        Guid tenantId,
        IReadOnlyCollection<string> wanted,
        Func<string, bool> scope,
        CancellationToken ct)
    {
        var existing = (await db.TenantModules.Where(m => m.TenantId == tenantId).ToListAsync(ct))
            .Where(m => scope(m.ModuleKey))
            .ToList();
        db.TenantModules.RemoveRange(existing.Where(m => !wanted.Contains(m.ModuleKey)));

        var added = wanted.Where(k => existing.All(m => m.ModuleKey != k)).ToList();
        if (added.Count == 0) return;
        var enabledBy = await AdminEmailAsync(db, http, ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var key in added)
            db.TenantModules.Add(new TenantModule { TenantId = tenantId, ModuleKey = key, EnabledAtUtc = now, EnabledBy = enabledBy });
    }

    /// <summary>The calling admin's e-mail for <see cref="TenantModule.EnabledBy"/>; null when unknown.</summary>
    private static async Task<string?> AdminEmailAsync(CentralApiDbContext db, HttpContext http, CancellationToken ct)
    {
        if (!Guid.TryParse(http.User.FindFirstValue("sub"), out var adminId)) return null;
        var email = await db.AdminUsers.AsNoTracking().Where(a => a.Id == adminId).Select(a => a.Email).FirstOrDefaultAsync(ct);
        // Audit only: an admin email may be longer (255) than the column (128).
        return email is { Length: > TenantModule.EnabledByMaxLength } ? email[..TenantModule.EnabledByMaxLength] : email;
    }

    /// <summary>The tenants' Go module keys, sorted, keyed by tenant (tenants without any are absent).</summary>
    public static async Task<Dictionary<Guid, string[]>> GoModulesByTenantAsync(CentralApiDbContext db, IReadOnlyCollection<Guid> tenantIds, CancellationToken ct)
    {
        if (tenantIds.Count == 0) return new Dictionary<Guid, string[]>();
        var ids = tenantIds.ToArray();
        var rows = await db.TenantModules.AsNoTracking()
            .Where(m => ids.Contains(m.TenantId) && m.ModuleKey.StartsWith(TenantModules.GoPrefix))
            .Select(m => new { m.TenantId, m.ModuleKey })
            .ToListAsync(ct);
        return rows.GroupBy(r => r.TenantId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.ModuleKey).Order(StringComparer.Ordinal).ToArray());
    }
}
