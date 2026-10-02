using System.Text.Json;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Preferences;

/// <summary>
/// The layers under a user's own view preferences (Siparis_Cepte KB kural 59, ErpBridge KB kural 35): their roles' defaults
/// and locks, and the locks an administrator set for them. All are flat JSON objects of setting path → value; the server
/// never interprets a path, it only merges.
///
/// Roles merge from the lowest precedence to the highest (<see cref="MobileUserRoles.All"/> is highest first), so when two
/// roles disagree the higher one wins; the person's own locks come last and win over role locks. Pure: no database.
/// </summary>
public static class ViewPreferenceLayers
{
    public sealed record Merged(JsonObject Data, JsonObject Locks, string Stamp);

    /// <param name="roles">The user's roles.</param>
    /// <param name="templates">The company's role templates (any roles; others are ignored).</param>
    /// <param name="userLocksJson">The person's own locks, or null to merge roles only.</param>
    /// <param name="userLocksVersion">Counted into the stamp.</param>
    public static Merged Merge(IEnumerable<string> roles, IEnumerable<TenantRoleViewPreference> templates, string? userLocksJson, long userLocksVersion)
    {
        var held = roles.ToHashSet(StringComparer.Ordinal);
        var byRole = templates.ToDictionary(t => t.Role, StringComparer.Ordinal);
        var lowestFirst = MobileUserRoles.All.Where(held.Contains).Reverse().ToList();

        var data = new JsonObject();
        var locks = new JsonObject();
        foreach (var role in lowestFirst)
        {
            if (!byRole.TryGetValue(role, out var template)) continue;
            Overlay(data, template.Json);
            Overlay(locks, template.LocksJson);
        }
        if (userLocksJson is not null) Overlay(locks, userLocksJson);

        // Highest first, like the role list everywhere else; a role without a template counts as version 0.
        var stamp = string.Join(",", MobileUserRoles.All.Where(held.Contains)
            .Select(r => $"{r}:{(byRole.TryGetValue(r, out var t) ? t.Version : 0)}")) + $"|u:{userLocksVersion}";
        return new Merged(data, locks, stamp);
    }

    /// <summary>Copies every top-level property of <paramref name="json"/> onto <paramref name="target"/>; a broken document adds nothing.</summary>
    private static void Overlay(JsonObject target, string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        JsonObject? source;
        try
        {
            source = JsonNode.Parse(json) as JsonObject;
        }
        catch (JsonException)
        {
            return;
        }
        if (source is null) return;
        foreach (var (key, value) in source) target[key] = value?.DeepClone();
    }
}
