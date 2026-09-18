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
        ParameterScope scope, Guid catalogEntryId, string value, CancellationToken ct = default)
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

        switch (stored, isDefault)
        {
            case (null, true):
                return WriteOutcome.Unchanged;

            case (not null, true):
                db.ParameterValues.Remove(stored);
                await db.SaveChangesAsync(ct);
                return WriteOutcome.Deleted;

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
                await db.SaveChangesAsync(ct);
                return WriteOutcome.Inserted;

            case (not null, false) when string.Equals(stored.Value, value, StringComparison.Ordinal):
                return WriteOutcome.Unchanged;

            default:
                stored!.Value = value;
                stored.UpdatedAtUtc = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
                return WriteOutcome.Updated;
        }
    }

    /// <summary>Puts a parameter back to its catalogue default by removing any stored deviation.</summary>
    public async Task<WriteOutcome> ResetAsync(
        ParameterScope scope, Guid catalogEntryId, CancellationToken ct = default)
    {
        var entry = await db.ParameterCatalog.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == catalogEntryId, ct)
            ?? throw new ScopeMismatchException($"No catalogue entry {catalogEntryId}.");

        return await SetAsync(scope, catalogEntryId, entry.DefaultValue, ct);
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
