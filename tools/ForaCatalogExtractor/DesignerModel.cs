namespace ErpBridge.Tools.ForaCatalog;

/// <summary>
/// One designer field of a WinForms form, with the handful of properties the layout
/// reconstruction needs. Everything here is read straight out of <c>InitializeComponent</c>.
/// </summary>
public sealed class DesignerControl(string name, string type)
{
    public string Name { get; } = name;

    /// <summary>Declared type, e.g. <c>CheckEdit</c> or <c>TableLayoutPanel</c>.</summary>
    public string Type { get; } = type;

    /// <summary><c>Text</c> for most controls, <c>Properties.Caption</c> for a <c>CheckEdit</c>.</summary>
    public string? Text { get; set; }

    /// <summary>Field name of the container this control was added to.</summary>
    public string? Parent { get; set; }

    public int? X { get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? TabIndex { get; set; }

    /// <summary>Cell column when the parent is a <c>TableLayoutPanel</c>.</summary>
    public int? Column { get; set; }

    /// <summary>Cell row when the parent is a <c>TableLayoutPanel</c>.</summary>
    public int? Row { get; set; }

    /// <summary>Position among the tab pages of its tab control.</summary>
    public int? TabOrder { get; set; }

    /// <summary>
    /// The designer masks this field (<c>UseSystemPasswordChar</c> or <c>PasswordChar</c>), so its
    /// value is a credential and must never be rendered as ordinary text.
    /// </summary>
    public bool IsSecret { get; set; }

    /// <summary>ERP list this control's items are loaded from, e.g. <c>Depo</c>. Null otherwise.</summary>
    public string? ErpDataSource { get; set; }

    /// <summary>Fixed options the designer builds inline, in declaration order. Empty otherwise.</summary>
    public List<ControlOption> Options { get; } = [];

    public bool IsLabel => Type is "LabelControl" or "Label";

    public bool IsTabPage => Type == "XtraTabPage";

    public bool IsTabControl => Type == "XtraTabControl";
}

/// <summary>One entry of a fixed option list the designer builds in code.</summary>
/// <param name="Value">Stored value, as it reaches <c>ParametreDegeri</c>.</param>
/// <param name="Label">Text shown to the person.</param>
public sealed record ControlOption(string Value, string Label);
