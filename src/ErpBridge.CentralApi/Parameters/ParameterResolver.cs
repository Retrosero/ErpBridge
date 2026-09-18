using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Parameters;

/// <summary>
/// Turns the catalogue and the stored deviations into the values that actually apply, and writes
/// changes back the way Fora does.
///
/// Fora keeps its defaults inside the application and stores only deviations: a value set back to
/// its default is deleted rather than written down. Reproducing that exactly is what lets the
/// agent mirror into Mikro without translating anything, and lets "what has this customer
/// changed?" stay a single query.
/// </summary>
public sealed class ParameterResolver(CentralApiDbContext db)
{
    /// <summary>What a write did, mirroring Fora's four branches.</summary>
    public enum WriteOutcome
    {
        /// <summary>Already the default and nothing stored: nothing to do.</summary>
        Unchanged,

        /// <summary>First deviation from the default.</summary>
        Inserted,

        /// <summary>A different deviation.</summary>
        Updated,

        /// <summary>Back to the default, so the row is removed.</summary>
        Deleted,
    }

    /// <summary>One parameter as it currently applies to a scope.</summary>
    public sealed record Effective
    {
        public required ParameterCatalogEntry Entry { get; init; }

        /// <summary>The value in force: the stored deviation when there is one, else the default.</summary>
        public required string Value { get; init; }

        /// <summary>True when a row exists, i.e. someone moved this away from its default.</summary>
        public required bool IsOverridden { get; init; }

        /// <summary>When the deviation was last written; null when the default applies.</summary>
        public DateTimeOffset? OverriddenAtUtc { get; init; }
    }

    /// <summary>
    /// Who is making a change and why. Required on every write: a parameter can decide whether a
    /// plasiyer may edit a price or which warehouse a document leaves from, so an unattributed
    /// change is the kind that costs money before anyone notices.
    /// </summary>
    /// <param name="Source">One of <see cref="ParameterChangeSources"/>.</param>
    /// <param name="AdminUserId">Admin behind the change, when it came from the panel.</param>
    /// <param name="Actor">Readable actor when no admin is behind it, e.g. an import batch.</param>
    public readonly record struct ChangeContext(string Source, Guid? AdminUserId = null, string Actor = "");

    /// <summary>Raised when a scope cannot address the parameter it is used with.</summary>
    public sealed class ScopeMismatchException(string message) : InvalidOperationException(message);

    /// <summary>
    /// Every parameter of one catalogue set as it applies to <paramref name="scope"/>, in the
    /// order Fora's own editor lists them. Deprecated parameters are included: a customer may
    /// still hold a value for one, and hiding it would make that value unexplainable.
    /// </summary>
    public async Task<IReadOnlyList<Effective>> ResolveAsync(
        ParameterScope scope, string catalogMethod, CancellationToken ct = default)
    {
        var entries = await db.ParameterCatalog
            .AsNoTracking()
            .Where(e => e.CatalogMethod == catalogMethod)
            .OrderBy(e => e.EditorOrder ?? int.MaxValue)
            .ThenBy(e => e.ParametreId)
            .ToListAsync(ct);

        var overrides = await ScopedValues(scope)
            .AsNoTracking()
            .Where(v => entries.Select(e => e.Id).Contains(v.ParameterCatalogEntryId))
            .ToDictionaryAsync(v => v.ParameterCatalogEntryId, ct);

        return entries.Select(entry =>
        {
            var stored = overrides.GetValueOrDefault(entry.Id);

            return new Effective
            {
                Entry = entry,
                Value = stored?.Value ?? entry.DefaultValue,
                IsOverridden = stored is not null,
                OverriddenAtUtc = stored?.UpdatedAtUtc,
            };
        }).ToList();
    }

    /// <summary>One catalogue set as it applies to a scope, for the bulk read.</summary>
    public sealed record ScopeValues(ParameterScope Scope, IReadOnlyList<Effective> Values);

    /// <summary>
    /// One catalogue set as it applies to many scopes at once.
    ///
    /// The Android endpoint asks for every active user of every company in one call. Resolving
    /// them one at a time would read the 1,801 catalogue entries once per user, so the entries and
    /// the stored deviations are each read once and composed in memory.
    /// </summary>
    public async Task<IReadOnlyList<ScopeValues>> ResolveManyAsync(
        IReadOnlyCollection<ParameterScope> scopes, string catalogMethod, CancellationToken ct = default)
    {
        if (scopes.Count == 0)
        {
            return [];
        }

        var entries = await db.ParameterCatalog.AsNoTracking()
            .Where(e => e.CatalogMethod == catalogMethod)
            .OrderBy(e => e.EditorOrder ?? int.MaxValue)
            .ThenBy(e => e.ParametreId)
            .ToListAsync(ct);

        var entryIds = entries.Select(e => e.Id).ToHashSet();
        var tenantIds = scopes.Select(s => s.TenantId).Distinct().ToList();
        var companyIds = scopes.Select(s => s.ErpCompanyId).Distinct().ToList();

        // Widened to the tenants and companies asked for, then narrowed in memory to the exact
        // scopes: every dimension still has to match, or one user's settings reach another.
        var wanted = scopes.ToHashSet();

        var stored = await db.ParameterValues.AsNoTracking()
            .Where(v => tenantIds.Contains(v.TenantId) && companyIds.Contains(v.ErpCompanyId))
            .ToListAsync(ct);

        var byScope = stored
            .Where(v => entryIds.Contains(v.ParameterCatalogEntryId))
            .Select(v => (Scope: new ParameterScope(
                v.TenantId, v.ErpCompanyId, v.MobileUserId, v.Scope1, v.Scope2), Value: v))
            .Where(x => wanted.Contains(x.Scope))
            .GroupBy(x => x.Scope)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Value.ParameterCatalogEntryId, x => x.Value));

        return scopes.Select(scope =>
        {
            var overrides = byScope.GetValueOrDefault(scope);

            return new ScopeValues(scope, entries.Select(entry =>
            {
                var row = overrides?.GetValueOrDefault(entry.Id);

                return new Effective
                {
                    Entry = entry,
                    Value = row?.Value ?? entry.DefaultValue,
                    IsOverridden = row is not null,
                    OverriddenAtUtc = row?.UpdatedAtUtc,
                };
            }).ToList());
        }).ToList();
    }

    /// <summary>
    /// The change counter of many scopes in one read; a scope that has never been touched is
    /// absent rather than zero.
    /// </summary>
    public async Task<IReadOnlyDictionary<ParameterScope, long>> RevisionsAsync(
        IReadOnlyCollection<ParameterScope> scopes, CancellationToken ct = default)
    {
        if (scopes.Count == 0)
        {
            return new Dictionary<ParameterScope, long>();
        }

        var tenantIds = scopes.Select(s => s.TenantId).Distinct().ToList();
        var companyIds = scopes.Select(s => s.ErpCompanyId).Distinct().ToList();
        var wanted = scopes.ToHashSet();

        var rows = await db.ParameterRevisions.AsNoTracking()
            .Where(r => tenantIds.Contains(r.TenantId) && companyIds.Contains(r.ErpCompanyId))
            .ToListAsync(ct);

        return rows
            .Select(r => (Scope: new ParameterScope(
                r.TenantId, r.ErpCompanyId, r.MobileUserId, r.Scope1, r.Scope2), r.Revision))
            .Where(x => wanted.Contains(x.Scope))
            .ToDictionary(x => x.Scope, x => x.Revision);
    }

    /// <summary>The value in force for one parameter, or null when the catalogue has no such entry.</summary>
    public async Task<Effective?> ResolveOneAsync(
        ParameterScope scope, Guid catalogEntryId, CancellationToken ct = default)
    {
        var entry = await db.ParameterCatalog.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == catalogEntryId, ct);

        if (entry is null)
        {
            return null;
        }

        var stored = await ScopedValues(scope).AsNoTracking()
            .FirstOrDefaultAsync(v => v.ParameterCatalogEntryId == catalogEntryId, ct);

        return new Effective
        {
            Entry = entry,
            Value = stored?.Value ?? entry.DefaultValue,
            IsOverridden = stored is not null,
            OverriddenAtUtc = stored?.UpdatedAtUtc,
        };
    }

    /// <summary>
    /// Writes a value the way Fora does: equal to the default means no row should exist, so an
    /// existing one is deleted and a missing one is not created.
    /// </summary>
    /// <exception cref="ScopeMismatchException">The scope cannot address this parameter.</exception>
    public async Task<WriteOutcome> SetAsync(
        ParameterScope scope, Guid catalogEntryId, string value, ChangeContext by,
        CancellationToken ct = default)
    {
        var entry = await db.ParameterCatalog.FirstOrDefaultAsync(e => e.Id == catalogEntryId, ct)
            ?? throw new ScopeMismatchException($"No catalogue entry {catalogEntryId}.");

        if (scope.Reject(entry) is { } reason)
        {
            throw new ScopeMismatchException(reason);
        }

        var stored = await ScopedValues(scope)
            .FirstOrDefaultAsync(v => v.ParameterCatalogEntryId == catalogEntryId, ct);

        var isDefault = string.Equals(value, entry.DefaultValue, StringComparison.Ordinal);

        var previous = stored?.Value;
        WriteOutcome outcome;

        switch (stored, isDefault)
        {
            case (null, true):
                return WriteOutcome.Unchanged;

            case (not null, true):
                db.ParameterValues.Remove(stored);
                outcome = WriteOutcome.Deleted;
                break;

            case (null, false):
                db.ParameterValues.Add(new ParameterValue
                {
                    TenantId = scope.TenantId,
                    ErpCompanyId = scope.ErpCompanyId,
                    ParameterCatalogEntryId = catalogEntryId,
                    MobileUserId = scope.MobileUserId,
                    Scope1 = scope.Scope1,
                    Scope2 = scope.Scope2,
                    Value = value,
                });
                outcome = WriteOutcome.Inserted;
                break;

            case (not null, false) when string.Equals(stored.Value, value, StringComparison.Ordinal):
                return WriteOutcome.Unchanged;

            default:
                stored!.Value = value;
                stored.UpdatedAtUtc = DateTimeOffset.UtcNow;
                outcome = WriteOutcome.Updated;
                break;
        }

        // The counter and the trail move with the value in one save: a client that sees an
        // unchanged revision has to be able to trust that nothing moved.
        await BumpRevisionAsync(scope, ct);
        Record(scope, entry, outcome, previous, outcome == WriteOutcome.Deleted ? null : value, by);

        await db.SaveChangesAsync(ct);
        return outcome;
    }

    /// <summary>
    /// Raises the scope counter, creating it on first use. Clients compare this one number
    /// instead of pulling a set that runs to 1,801 parameters for a single mobile user (D9).
    /// </summary>
    private async Task BumpRevisionAsync(ParameterScope scope, CancellationToken ct)
    {
        var revision = await db.ParameterRevisions.FirstOrDefaultAsync(r =>
            r.TenantId == scope.TenantId
            && r.ErpCompanyId == scope.ErpCompanyId
            && r.MobileUserId == scope.MobileUserId
            && r.Scope1 == scope.Scope1
            && r.Scope2 == scope.Scope2, ct);

        if (revision is null)
        {
            db.ParameterRevisions.Add(new ParameterRevision
            {
                TenantId = scope.TenantId,
                ErpCompanyId = scope.ErpCompanyId,
                MobileUserId = scope.MobileUserId,
                Scope1 = scope.Scope1,
                Scope2 = scope.Scope2,
                Revision = 1,
            });
            return;
        }

        revision.Revision++;
        revision.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Appends the change to the trail. A credential value is replaced by a placeholder: an audit
    /// trail that records passwords in clear is worse than no audit trail.
    /// </summary>
    private void Record(
        ParameterScope scope, ParameterCatalogEntry entry, WriteOutcome outcome,
        string? oldValue, string? newValue, ChangeContext by)
    {
        var masked = entry.Editor == SecretEditor;

        db.ParameterAudit.Add(new ParameterAuditEntry
        {
            TenantId = scope.TenantId,
            ErpCompanyId = scope.ErpCompanyId,
            ParameterCatalogEntryId = entry.Id,
            MobileUserId = scope.MobileUserId,
            Scope1 = scope.Scope1,
            Scope2 = scope.Scope2,
            Outcome = outcome.ToString(),
            OldValue = masked ? Mask(oldValue) : oldValue,
            NewValue = masked ? Mask(newValue) : newValue,
            IsMasked = masked,
            Source = by.Source,
            AdminUserId = by.AdminUserId,
            Actor = by.Actor,
        });
    }

    /// <summary>Editor kind the catalogue uses for a credential.</summary>
    private const string SecretEditor = "secret";

    /// <summary>Keeps "was empty" and "held something" apart without revealing the secret.</summary>
    private static string? Mask(string? value) =>
        value is null ? null : value.Length == 0 ? string.Empty : "\u2022\u2022\u2022\u2022\u2022\u2022";

    /// <summary>
    /// The scope counter, or 0 when nothing has ever changed in it. A client holding this number
    /// knows its copy is current.
    /// </summary>
    public async Task<long> RevisionAsync(ParameterScope scope, CancellationToken ct = default) =>
        await db.ParameterRevisions.AsNoTracking()
            .Where(r => r.TenantId == scope.TenantId
                        && r.ErpCompanyId == scope.ErpCompanyId
                        && r.MobileUserId == scope.MobileUserId
                        && r.Scope1 == scope.Scope1
                        && r.Scope2 == scope.Scope2)
            .Select(r => r.Revision)
            .FirstOrDefaultAsync(ct);

    /// <summary>Puts a parameter back to its catalogue default by removing any stored deviation.</summary>
    public async Task<WriteOutcome> ResetAsync(
        ParameterScope scope, Guid catalogEntryId, ChangeContext by, CancellationToken ct = default)
    {
        var entry = await db.ParameterCatalog.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == catalogEntryId, ct)
            ?? throw new ScopeMismatchException($"No catalogue entry {catalogEntryId}.");

        return await SetAsync(scope, catalogEntryId, entry.DefaultValue, by, ct);
    }

    /// <summary>
    /// Everything this scope has moved away from its default. This is what the mirror writes into
    /// Mikro and what "show only what has been changed" lists.
    /// </summary>
    public async Task<IReadOnlyList<ParameterValue>> OverridesAsync(
        ParameterScope scope, CancellationToken ct = default) =>
        await ScopedValues(scope).AsNoTracking().ToListAsync(ct);

    /// <summary>
    /// The stored rows of one scope. Every dimension is compared, including the ones that are
    /// null or empty: leaving any of them out is how one company's settings leak into another's.
    /// </summary>
    private IQueryable<ParameterValue> ScopedValues(ParameterScope scope) =>
        db.ParameterValues.Where(v =>
            v.TenantId == scope.TenantId
            && v.ErpCompanyId == scope.ErpCompanyId
            && v.MobileUserId == scope.MobileUserId
            && v.Scope1 == scope.Scope1
            && v.Scope2 == scope.Scope2);
}
