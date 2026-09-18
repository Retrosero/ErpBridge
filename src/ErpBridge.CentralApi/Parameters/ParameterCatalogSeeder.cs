using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Parameters;

/// <summary>
/// Brings <c>parameter_catalog_entries</c> in line with the catalogue shipped in this build.
///
/// Rows are never deleted. A parameter Fora has withdrawn is marked deprecated instead, so values
/// stored against it stay traceable rather than turning into orphans nobody can explain (D2).
/// </summary>
public static class ParameterCatalogSeeder
{
    /// <summary>What a seeding run changed, for the startup log and the tests.</summary>
    public readonly record struct Result(int Added, int Updated, int Deprecated, int Revived)
    {
        public bool Changed => Added > 0 || Updated > 0 || Deprecated > 0 || Revived > 0;
    }

    /// <summary>Idempotent: running it twice over the same catalogue changes nothing the second time.</summary>
    public static Result Seed(CentralApiDbContext db, IReadOnlyList<ParameterCatalogFile.CatalogRow> catalog)
    {
        var existing = db.ParameterCatalog
            .ToList()
            .ToDictionary(e => (e.CatalogMethod, e.ParametreId), CatalogKeyComparer.Instance);

        var seen = new HashSet<(string, int)>(CatalogKeyComparer.Instance);
        var now = DateTimeOffset.UtcNow;
        var added = 0;
        var updated = 0;
        var revived = 0;

        foreach (var row in catalog)
        {
            var key = (row.CatalogMethod, row.ParametreId);
            seen.Add(key);

            if (!existing.TryGetValue(key, out var entry))
            {
                db.ParameterCatalog.Add(ToEntry(row, now));
                added++;
                continue;
            }

            if (entry.IsDeprecated)
            {
                entry.IsDeprecated = false;
                revived++;
            }

            if (Apply(entry, row))
            {
                entry.UpdatedAtUtc = now;
                updated++;
            }
        }

        var deprecated = 0;
        foreach (var pair in existing)
        {
            if (seen.Contains(pair.Key) || pair.Value.IsDeprecated)
            {
                continue;
            }

            pair.Value.IsDeprecated = true;
            pair.Value.UpdatedAtUtc = now;
            deprecated++;
        }

        if (added + updated + deprecated + revived > 0)
        {
            db.SaveChanges();
        }

        return new Result(added, updated, deprecated, revived);
    }

    private static ParameterCatalogEntry ToEntry(ParameterCatalogFile.CatalogRow row, DateTimeOffset now)
    {
        var entry = new ParameterCatalogEntry
        {
            Program = row.Program,
            CatalogMethod = row.CatalogMethod,
            ParametreId = row.ParametreId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        Apply(entry, row);
        return entry;
    }

    /// <summary>Copies the catalogue's fields onto an entry; returns true when anything moved.</summary>
    private static bool Apply(ParameterCatalogEntry entry, ParameterCatalogFile.CatalogRow row)
    {
        // IsImplemented does not come from the catalogue — it records what Sipariş Cepte honours,
        // which the catalogue knows nothing about — but it is still set here, from the list this
        // product maintains (D16). Set on every seed rather than only on insert, so a feature that
        // lands in a later release turns the panel's "inert in this version" badge off without
        // anyone touching the database.
        entry.IsImplemented = ImplementedParameters.Honours(row.CatalogMethod, row.ParametreId);

        // Program is copied too: a regenerated catalogue can correct it for an existing
        // (CatalogMethod, ParametreID), and a stale program would address the wrong Mikro rows.
        return Replace(v => entry.Program = v!, entry.Program, row.Program)
               | Replace(v => entry.Name = v!, entry.Name, row.Name)
               | Replace(v => entry.DefaultValue = v!, entry.DefaultValue, row.DefaultValue)
               | Replace(v => entry.DefaultSource = v, entry.DefaultSource, row.DefaultSource)
               | Replace(v => entry.ScopeKind = v!, entry.ScopeKind, row.ScopeKind)
               | Replace(v => entry.ScopeFields = v!, entry.ScopeFields, row.ScopeFields)
               | Replace(v => entry.User = v!, entry.User, row.User)
               | Replace(v => entry.AnaGrubu = v!, entry.AnaGrubu, row.AnaGrubu)
               | Replace(v => entry.AltGrubu = v!, entry.AltGrubu, row.AltGrubu)
               | Replace(v => entry.Editor = v, entry.Editor, row.Editor)
               | Replace(v => entry.ReferenceKind = v, entry.ReferenceKind, row.ReferenceKind)
               | Replace(v => entry.SecretSource = v, entry.SecretSource, row.SecretSource)
               | Replace(v => entry.Label = v, entry.Label, row.Label)
               | Replace(v => entry.TabPath = v, entry.TabPath, row.TabPath)
               | Replace(v => entry.OptionsJson = v, entry.OptionsJson, row.OptionsJson)
               | Replace(v => entry.SourceBuild = v!, entry.SourceBuild, row.SourceBuild)
               | ReplaceOrder(entry, row.EditorOrder);
    }

    private static bool Replace(Action<string?> set, string? current, string? next)
    {
        if (string.Equals(current, next, StringComparison.Ordinal))
        {
            return false;
        }

        set(next);
        return true;
    }

    private static bool ReplaceOrder(ParameterCatalogEntry entry, int? next)
    {
        if (entry.EditorOrder == next)
        {
            return false;
        }

        entry.EditorOrder = next;
        return true;
    }

    private sealed class CatalogKeyComparer : IEqualityComparer<(string, int)>
    {
        public static readonly CatalogKeyComparer Instance = new();

        public bool Equals((string, int) x, (string, int) y) =>
            x.Item2 == y.Item2 && string.Equals(x.Item1, y.Item1, StringComparison.Ordinal);

        public int GetHashCode((string, int) obj) =>
            HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.Item1), obj.Item2);
    }
}
