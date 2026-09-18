using System.Text.Json.Serialization;

namespace ErpBridge.Tools.ForaCatalog;

/// <summary>How the panel should render a parameter, inferred from the control Fora binds it to.</summary>
public static class EditorKinds
{
    /// <summary>DevExpress <c>CheckEdit</c>: stored as "0"/"1".</summary>
    public const string Boolean = "boolean";

    /// <summary>DevExpress <c>SpinEdit</c> read through <c>_GetInt</c>.</summary>
    public const string Integer = "integer";

    /// <summary>DevExpress <c>SpinEdit</c> read through <c>_GetDouble</c>.</summary>
    public const string Decimal = "decimal";

    /// <summary>Single-line free text.</summary>
    public const string Text = "text";

    /// <summary>Multi-line free text (<c>MemoEdit</c>).</summary>
    public const string MultilineText = "multilineText";

    /// <summary>Fixed option list (<c>ComboBox</c>, <c>ListBoxControl</c>).</summary>
    public const string Choice = "choice";

    /// <summary>Colour picker.</summary>
    public const string Color = "color";

    /// <summary>
    /// A code picked from an ERP list rather than typed — Fora uses a bespoke picker control.
    /// The panel offers a picker fed by the agent's lookups (D12); see
    /// <see cref="UiParameter.ReferenceKind"/> for which list.
    /// </summary>
    public const string Reference = "reference";

    /// <summary>
    /// Edited through a screen of its own in Fora (a report definition, for instance), or shown in
    /// different controls depending on another parameter. The panel cannot render it from this
    /// metadata and needs a purpose-built editor.
    /// </summary>
    public const string Composite = "composite";

    /// <summary>
    /// A credential. Fora masks the field in its own designer; the panel must never render or log
    /// it as ordinary text.
    /// </summary>
    public const string Secret = "secret";

    /// <summary>Bound to something the extractor could not classify.</summary>
    public const string Unknown = "unknown";
}

/// <summary>One tab page of Fora's mobile-user editor.</summary>
public sealed record UiTab
{
    /// <summary>Designer field name, e.g. <c>xtraTabPage30</c>.</summary>
    [JsonPropertyOrder(1)] public required string Name { get; init; }

    /// <summary>Caption shown on the tab.</summary>
    [JsonPropertyOrder(2)] public required string Title { get; init; }

    /// <summary>
    /// Titles from the outermost tab down to this one, e.g. <c>["Parametreler", "Tanımlamalar"]</c>.
    /// Fora nests tab controls inside tab pages, so a flat list of 63 tabs would lose the grouping.
    /// </summary>
    [JsonPropertyOrder(3)] public required IReadOnlyList<string> Path { get; init; }

    /// <summary>Position among its siblings, as the designer declares them.</summary>
    [JsonPropertyOrder(4)] public required int Order { get; init; }
}

/// <summary>One parameter as Fora's editor presents it.</summary>
public sealed record UiParameter
{
    /// <summary>Matches <c>ParameterDefault.Name</c> in the defaults catalogue.</summary>
    [JsonPropertyOrder(1)] public required string Parameter { get; init; }

    /// <summary>
    /// Catalogue set this parameter belongs to. Names are not unique across sets — 862 of the
    /// 3,365 distinct names appear in more than one — so a name alone does not identify a
    /// parameter, and one form can edit two sets at once.
    /// </summary>
    [JsonPropertyOrder(2)] public required string CatalogMethod { get; init; }

    /// <summary>Label a person reads next to the field, or null when none was found.</summary>
    [JsonPropertyOrder(3)] public string? Label { get; init; }

    /// <summary>One of <see cref="EditorKinds"/>.</summary>
    [JsonPropertyOrder(4)] public required string Editor { get; init; }

    /// <summary>Tab page field name this control sits on.</summary>
    [JsonPropertyOrder(5)] public string? Tab { get; init; }

    /// <summary>Tab titles from the outermost tab inwards.</summary>
    [JsonPropertyOrder(6)] public required IReadOnlyList<string> TabPath { get; init; }

    /// <summary>Designer field name of the bound control, kept for tracing back into the source.</summary>
    [JsonPropertyOrder(7)] public required string Control { get; init; }

    /// <summary>Designer type of the bound control, e.g. <c>CheckEdit</c>.</summary>
    [JsonPropertyOrder(8)] public required string ControlType { get; init; }

    /// <summary>Reading order inside the tab: top to bottom, then left to right.</summary>
    [JsonPropertyOrder(9)] public required int Order { get; init; }

    /// <summary>Where the label came from, so a doubtful match can be reviewed.</summary>
    [JsonPropertyOrder(10)] public required string LabelSource { get; init; }

    /// <summary>
    /// Which ERP list a <see cref="EditorKinds.Reference"/> value is chosen from
    /// (<c>cari</c>, <c>depo</c>, <c>kargo</c>, <c>ekipKodu</c>). Null for every other editor.
    /// </summary>
    [JsonPropertyOrder(11)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReferenceKind { get; init; }

    /// <summary>
    /// Fixed options for a <see cref="EditorKinds.Choice"/> field, when the designer builds the
    /// list in code. Empty when Fora fills it at run time and the panel must supply its own.
    /// </summary>
    [JsonPropertyOrder(12)] public required IReadOnlyList<UiOption> Options { get; init; }

    /// <summary>
    /// Other controls the same value is shown in. Fora sometimes swaps the editor depending on a
    /// second parameter — a printer field's value moves between a text box and two combo boxes
    /// according to its data type — and the panel has to reproduce that rather than pick one.
    /// </summary>
    [JsonPropertyOrder(13)] public required IReadOnlyList<UiAlternate> Alternates { get; init; }

    /// <summary>
    /// How a <see cref="EditorKinds.Secret"/> was recognised: <c>designer</c> when Fora masks the
    /// field itself, <c>name</c> when only the parameter's name says so. Fora leaves some
    /// credentials unmasked, and rendering those as plain text would be worse than a false
    /// positive here.
    /// </summary>
    [JsonPropertyOrder(14)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SecretSource { get; init; }
}

/// <summary>One fixed option of a choice field.</summary>
public sealed record UiOption
{
    [JsonPropertyOrder(1)] public required string Value { get; init; }
    [JsonPropertyOrder(2)] public required string Label { get; init; }
}

/// <summary>Another control the same parameter is shown in.</summary>
public sealed record UiAlternate
{
    [JsonPropertyOrder(1)] public required string Control { get; init; }
    [JsonPropertyOrder(2)] public required string ControlType { get; init; }
    [JsonPropertyOrder(3)] public required string Editor { get; init; }
}

/// <summary>Why a parameter or control could not be placed, so nothing disappears silently.</summary>
public sealed record UiGap
{
    [JsonPropertyOrder(1)] public required string Kind { get; init; }
    [JsonPropertyOrder(2)] public required string Subject { get; init; }
    [JsonPropertyOrder(3)] public required string Detail { get; init; }
}

/// <summary>Where a label came from.</summary>
public static class LabelSources
{
    /// <summary>The control's own caption (every <c>CheckEdit</c> carries one).</summary>
    public const string Caption = "caption";

    /// <summary>Nearest <c>LabelControl</c> to the left on the same row.</summary>
    public const string LabelLeft = "labelLeft";

    /// <summary>Nearest <c>LabelControl</c> directly above.</summary>
    public const string LabelAbove = "labelAbove";

    /// <summary>
    /// Caption of the check box to the left: some fields exist only to qualify that
    /// check box, and its caption is the only wording they have.
    /// </summary>
    public const string CheckBoxLeft = "checkBoxLeft";

    /// <summary>Label in the neighbouring cell of the same table row.</summary>
    public const string TableCell = "tableCell";

    /// <summary>Nothing suitable was found.</summary>
    public const string None = "none";
}

/// <summary>The extracted editor layout, written to <c>catalog/parameters/ui.akilli.json</c>.</summary>
public sealed record UiCatalog
{
    [JsonPropertyOrder(1)] public required int SchemaVersion { get; init; }

    /// <summary>
    /// Fora build the layout was read from. Must match the defaults catalogue's value:
    /// a layout and a set of defaults from different Fora releases cannot be trusted together.
    /// </summary>
    [JsonPropertyOrder(2)] public required string SourceBuild { get; init; }

    [JsonPropertyOrder(3)] public required string SourceFile { get; init; }
    [JsonPropertyOrder(4)] public required string SourceType { get; init; }

    /// <summary>Catalogue sets this form edits; usually one, two when a screen combines them.</summary>
    [JsonPropertyOrder(5)] public required IReadOnlyList<string> CatalogMethods { get; init; }

    [JsonPropertyOrder(6)] public required int TabCount { get; init; }
    [JsonPropertyOrder(7)] public required int ParameterCount { get; init; }

    /// <summary>Parameters whose label could not be determined; expected to stay small.</summary>
    [JsonPropertyOrder(8)] public required int UnlabelledCount { get; init; }

    [JsonPropertyOrder(9)] public required IReadOnlyList<UiTab> Tabs { get; init; }
    [JsonPropertyOrder(10)] public required IReadOnlyList<UiParameter> Parameters { get; init; }

    /// <summary>Everything the extractor could not resolve, listed rather than dropped.</summary>
    [JsonPropertyOrder(11)] public required IReadOnlyList<UiGap> Gaps { get; init; }
}
