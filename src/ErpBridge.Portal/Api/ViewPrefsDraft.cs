using System.Text.Json;
using System.Text.Json.Nodes;

namespace ErpBridge.Portal.Api;

public enum ViewPrefsMode
{
    /// <summary>One person: <see cref="ViewPrefsDraft.Values"/> is their own nested document.</summary>
    User,

    /// <summary>A role template: <see cref="ViewPrefsDraft.Values"/> is a flat path → value object.</summary>
    Role
}

/// <summary>
/// Edits one person's or one role's view settings the way the phone reads them (Siparis_Cepte KB kural 59, ErpBridge KB
/// kural 35): factory &lt; role default &lt; the person's own document &lt; lock.
///
/// In <see cref="ViewPrefsMode.User"/> mode <see cref="Values"/> is the person's own document, nested like the phone's
/// profile; fields the panel does not know stay as they are. The role layers come from the server, already merged.
/// In <see cref="ViewPrefsMode.Role"/> mode <see cref="Values"/> is the role's flat object. <see cref="Locks"/> is always flat:
/// setting key (the path, or <c>path#member</c> for one list member) → value.
/// </summary>
public sealed class ViewPrefsDraft
{
    private readonly ViewSettingsCatalogDto _catalog;
    private readonly JsonObject _roleValues;
    private readonly JsonObject _roleLocks;

    public ViewPrefsDraft(ViewSettingsCatalogDto catalog, ViewPrefsMode mode, JsonObject values, JsonObject locks,
        JsonObject? roleValues = null, JsonObject? roleLocks = null)
    {
        _catalog = catalog;
        Mode = mode;
        Values = values;
        Locks = locks;
        _roleValues = roleValues ?? new JsonObject();
        _roleLocks = roleLocks ?? new JsonObject();
    }

    public ViewPrefsMode Mode { get; }

    public JsonObject Values { get; }

    public JsonObject Locks { get; }

    /// <summary>How many edits since loading; the save button shows it.</summary>
    public int Changes { get; private set; }

    /// <summary>A JSON object element as an editable node; anything else as an empty object.</summary>
    public static JsonObject ObjectOf(JsonElement? element) =>
        element is { ValueKind: JsonValueKind.Object } value ? JsonNode.Parse(value.GetRawText())!.AsObject() : new JsonObject();

    // ------------------------------------------------------------------ reading

    public JsonNode? Factory(ViewSettingDto s) =>
        s.Type == "member" ? JsonValue.Create(FactoryList(s.Path).Contains(s.Member!)) : Node(s.Default);

    /// <summary>The value set at this level (the person's own, or the role's); null when not set here.</summary>
    public JsonNode? Own(ViewSettingDto s)
    {
        if (Mode == ViewPrefsMode.Role) return Values[s.Key];
        var node = Nested(Values, s.Path);
        if (s.Type != "member") return node;
        return node is JsonArray list ? JsonValue.Create(Strings(list).Contains(s.Member!)) : null;
    }

    /// <summary>What applies without <see cref="Own"/>: the roles' default for a person, otherwise the factory value.</summary>
    public JsonNode? Inherited(ViewSettingDto s)
    {
        if (Mode == ViewPrefsMode.User)
        {
            if (s.Type == "member") return JsonValue.Create(InheritedList(s.Path).Contains(s.Member!));
            if (_roleValues[s.Key] is { } fromRole) return fromRole;
        }
        return Factory(s);
    }

    public bool LockedHere(ViewSettingDto s) => Locks.ContainsKey(s.Key);

    /// <summary>A person's setting a role locked: shown, not editable here (it is changed on the role's template).</summary>
    public bool LockedByRole(ViewSettingDto s) => !LockedHere(s) && _roleLocks.ContainsKey(s.Key);

    public JsonNode? Effective(ViewSettingDto s) => Locks[s.Key] ?? _roleLocks[s.Key] ?? Own(s) ?? Inherited(s);

    public bool IsOn(ViewSettingDto s) => Effective(s) is JsonValue value && value.TryGetValue(out bool on) && on;

    public IReadOnlyList<string> List(ViewSettingDto s) => Effective(s) is JsonArray list ? Strings(list).ToList() : [];

    public bool Same(ViewSettingDto s, JsonElement option) => JsonNode.DeepEquals(Effective(s), Node(option));

    /// <summary>Where the shown value comes from, as the editor labels it.</summary>
    public string Source(ViewSettingDto s)
    {
        if (LockedHere(s)) return Mode == ViewPrefsMode.User ? "Kişiye kilitli" : "Kilitli";
        if (LockedByRole(s)) return "Rol kilidi";
        if (Own(s) is not null) return Mode == ViewPrefsMode.User ? "Kişisel" : "Rol ayarı";
        if (Mode == ViewPrefsMode.User && (_roleValues.ContainsKey(s.Key) || (s.Type == "member" && _roleValues.ContainsKey(s.Path)))) return "Rolden";
        return "Varsayılan";
    }

    /// <summary>Not locked by a role, and the setting it depends on (koli düğmesi → adet çubuğu) is on.</summary>
    public bool Enabled(ViewSettingDto s) =>
        !LockedByRole(s) && (s.Requires is null || _catalog.Settings.FirstOrDefault(x => x.Path == s.Requires) is not { } owner || IsOn(owner));

    // ------------------------------------------------------------------ writing

    /// <summary>A new value: into the lock when the setting is locked here, otherwise into this level.</summary>
    public void Set(ViewSettingDto s, JsonNode? value)
    {
        if (LockedByRole(s)) return;
        if (LockedHere(s)) Locks[s.Key] = value?.DeepClone();
        else if (Mode == ViewPrefsMode.Role) Values[s.Key] = value?.DeepClone();
        else if (s.Type == "member") SetMember(s, value is JsonValue v && v.TryGetValue(out bool on) && on);
        else SetNested(Values, s.Path, value?.DeepClone());
        Changes++;
    }

    /// <summary>
    /// Back to what the roles (or the factory) give: this level's value goes. For a list member the person keeps their own
    /// list, with the member set back to the inherited value.
    /// </summary>
    public void Reset(ViewSettingDto s)
    {
        if (Mode == ViewPrefsMode.Role)
        {
            if (Values.Remove(s.Key)) Changes++;
            return;
        }
        if (s.Type == "member")
        {
            if (Nested(Values, s.Path) is not JsonArray) return;
            var inherited = InheritedList(s.Path).Contains(s.Member!);
            if (Own(s) is JsonValue own && own.GetValue<bool>() == inherited) return;
            SetMember(s, inherited);
            Changes++;
            return;
        }
        if (RemoveNested(Values, s.Path)) Changes++;
    }

    public void ResetPage(ViewSettingsPageDto page)
    {
        foreach (var s in page.Settings)
        {
            Reset(s);
            if (s.SwitchPath is not null)
                foreach (var option in s.Options) Reset(SwitchOf(s, option));
        }
    }

    /// <summary>Locks the shown value (for a person, or on the role's template), or unlocks it.</summary>
    public void ToggleLock(ViewSettingDto s)
    {
        if (LockedByRole(s)) return;
        if (!Locks.Remove(s.Key)) Locks[s.Key] = Effective(s)?.DeepClone();
        Changes++;
    }

    /// <summary>The switch of one option of an order setting (hızlı işlem görünürlüğü), as a member setting of its own.</summary>
    public static ViewSettingDto SwitchOf(ViewSettingDto order, ViewSettingOptionDto option) => new()
    {
        Path = order.SwitchPath!,
        Type = "member",
        Title = option.Label,
        Member = Text(option.Value)
    };

    // ------------------------------------------------------------------ lists

    private void SetMember(ViewSettingDto s, bool on)
    {
        var list = (Nested(Values, s.Path) is JsonArray own ? Strings(own) : InheritedList(s.Path)).ToList();
        list.Remove(s.Member!);
        if (on) list.Add(s.Member!);
        SetNested(Values, s.Path, new JsonArray(list.Select(x => (JsonNode?)JsonValue.Create(x)).ToArray()));
    }

    /// <summary>The factory list at a path: the switch default of the order setting that lists it, or its own default.</summary>
    private IEnumerable<string> FactoryList(string path)
    {
        var owner = _catalog.Settings.FirstOrDefault(x => x.SwitchPath == path);
        if (owner?.SwitchDefault is { ValueKind: JsonValueKind.Array } switches) return switches.EnumerateArray().Select(e => e.ToString());
        var plain = _catalog.Settings.FirstOrDefault(x => x.Path == path && x.Type != "member");
        if (plain?.Default is { ValueKind: JsonValueKind.Array } list) return list.EnumerateArray().Select(e => e.ToString());
        return _catalog.Settings.Where(x => x.Type == "member" && x.Path == path && x.Default is { ValueKind: JsonValueKind.True }).Select(x => x.Member!);
    }

    /// <summary>A person's list from the roles: the factory list, a role's whole list, then a role's member switches.</summary>
    private HashSet<string> InheritedList(string path)
    {
        var list = (_roleValues[path] is JsonArray whole ? Strings(whole) : FactoryList(path)).ToHashSet(StringComparer.Ordinal);
        foreach (var (key, value) in _roleValues)
        {
            if (!key.StartsWith(path + "#", StringComparison.Ordinal)) continue;
            var member = key[(path.Length + 1)..];
            if (value is JsonValue v && v.TryGetValue(out bool on) && on) list.Add(member);
            else list.Remove(member);
        }
        return list;
    }

    // ------------------------------------------------------------------ JSON helpers

    private static IEnumerable<string> Strings(JsonArray list) => list.Select(x => x is null ? string.Empty : Text(x));

    private static string Text(JsonNode node) => node is JsonValue value && value.TryGetValue(out string? text) ? text : node.ToJsonString();

    private static string Text(JsonElement element) => element.ValueKind == JsonValueKind.String ? element.GetString()! : element.GetRawText();

    private static JsonNode? Node(JsonElement? element) =>
        element is { } value && value.ValueKind != JsonValueKind.Null ? JsonNode.Parse(value.GetRawText()) : null;

    private static JsonNode? Nested(JsonObject root, string path)
    {
        JsonNode? node = root;
        foreach (var part in path.Split('.'))
        {
            node = (node as JsonObject)?[part];
            if (node is null) return null;
        }
        return node;
    }

    private static void SetNested(JsonObject root, string path, JsonNode? value)
    {
        var parts = path.Split('.');
        var node = root;
        foreach (var part in parts[..^1])
        {
            if (node[part] is not JsonObject child)
            {
                child = new JsonObject();
                node[part] = child;
            }
            node = child;
        }
        node[parts[^1]] = value;
    }

    private static bool RemoveNested(JsonObject root, string path)
    {
        var parts = path.Split('.');
        var node = root;
        foreach (var part in parts[..^1])
        {
            if (node[part] is not JsonObject child) return false;
            node = child;
        }
        return node.Remove(parts[^1]);
    }
}
