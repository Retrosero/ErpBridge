using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ErpBridge.Tools.ForaCatalog;

/// <summary>
/// Reads Fora's mobile-user editor and works out, for every parameter, which tab it lives on,
/// what label sits next to it and which editor the panel should offer.
///
/// The form is 38k decompiled lines and binds ~1,800 parameters, so none of this is written by
/// hand. Two facts make the reconstruction tractable: every binding has the shape
/// <c>control.Property = _kullaniciparametreleri._GetParametre("Name")._GetX</c>, and every
/// <c>CheckEdit</c> carries its own caption.
/// </summary>
public static class UiExtractor
{
    /// <summary>
    /// 2 — every parameter now carries the catalogue set it belongs to, plus options, alternates
    /// and the secret/reference editor kinds. A version 1 document has none of those fields.
    /// </summary>
    public const int SchemaVersion = 2;
    private const string ParameterAccessor = "_GetParametre";

    /// <summary>Rough row height and column width used only to order table-cell controls.</summary>
    private const int CellHeight = 24;
    private const int CellWidth = 120;

    /// <summary>How far apart two controls' vertical centres may be and still count as one row.</summary>
    private const int SameRowTolerance = 12;

    /// <param name="sourceText">Contents of the decompiled editor.</param>
    /// <param name="sourceFile">Repository-relative path recorded in the output.</param>
    /// <param name="formTypeName">Class that declares the editor.</param>
    /// <param name="sourceBuild">Fora build identifier, stamped on the output.</param>
    /// <param name="declared">
    /// Every parameter the defaults catalogue declares for the sets this form edits, mapped to
    /// its owning set. Anything declared but absent from the editor is reported: a catalogue-driven
    /// panel would otherwise have no way to show it, and the omission would pass unnoticed.
    /// </param>
    /// <param name="required">
    /// The subset of <paramref name="declared"/> this form is the editor for. A screen may also
    /// touch a few parameters belonging to another set without owning that whole set, and the
    /// rest of that set must not then be reported as missing here.
    /// </param>
    public static UiCatalog Extract(
        string sourceText,
        string sourceFile,
        string formTypeName,
        string sourceBuild,
        IReadOnlyDictionary<string, string> declared,
        IReadOnlyCollection<string> required)
    {
        var root = CSharpSyntaxTree.ParseText(sourceText).GetRoot();

        var form = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .SingleOrDefault(c => c.Identifier.ValueText == formTypeName)
            ?? throw new CatalogExtractionException($"class {formTypeName} not found in {sourceFile}.");

        var controls = DesignerReader.Read(form);
        var gaps = new List<UiGap>();
        var bindings = ReadBindings(form, controls, gaps);

        var bound = bindings.Primary.Select(b => b.Parameter).ToHashSet(StringComparer.Ordinal);

        // A form may bind a name the sets it edits do not declare — that means the mapping table
        // points at the wrong set, which is worth failing on rather than shipping a wrong panel.
        var undeclared = bound.Where(b => !declared.ContainsKey(b)).Order(StringComparer.Ordinal).ToList();
        if (undeclared.Count > 0)
        {
            throw new CatalogExtractionException(
                $"{formTypeName} binds {undeclared.Count} parameter(s) none of its catalogue sets declare: "
                + string.Join(", ", undeclared.Take(5)) + (undeclared.Count > 5 ? ", …" : string.Empty));
        }

        foreach (var parameter in required.Except(bound, StringComparer.Ordinal)
                     .Except(gaps.Where(g => g.Kind == "unboundParameter").Select(g => g.Subject), StringComparer.Ordinal)
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            gaps.Add(new UiGap
            {
                Kind = "notInEditor",
                Subject = parameter,
                Detail = "declared in the defaults catalogue but this editor never mentions it; "
                         + "the panel has no tab, label or editor type for it",
            });
        }

        var tabs = BuildTabs(controls);
        var tabByName = tabs.ToDictionary(t => t.Name, StringComparer.Ordinal);

        // Smaller screens have no tab control at all, so a missing tab is only worth reporting
        // on a form that actually uses tabs.
        var tabbed = tabs.Count > 0;

        var placed = bindings.Primary
            .Select(b => Place(
                b,
                declared[b.Parameter],
                bindings.Alternates.GetValueOrDefault(b.Parameter) ?? [],
                tabbed,
                controls,
                tabByName,
                gaps))
            .OrderBy(p => p.TabPath.Count == 0 ? 1 : 0)
            .ThenBy(p => string.Join(" / ", p.TabPath), StringComparer.Ordinal)
            .ThenBy(p => p.SortY)
            .ThenBy(p => p.SortX)
            .ThenBy(p => p.Parameter.Parameter, StringComparer.Ordinal)
            .ToList();

        var parameters = placed
            .Select((p, index) => p.Parameter with { Order = index })
            .ToList();

        return new UiCatalog
        {
            SchemaVersion = SchemaVersion,
            SourceBuild = sourceBuild,
            SourceFile = sourceFile,
            SourceType = formTypeName,
            CatalogMethods = declared.Values.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            TabCount = tabs.Count,
            ParameterCount = parameters.Count,
            UnlabelledCount = parameters.Count(p => p.Label is null),
            Tabs = tabs,
            Parameters = parameters,
            Gaps = gaps.OrderBy(g => g.Kind, StringComparer.Ordinal)
                .ThenBy(g => g.Subject, StringComparer.Ordinal)
                .ToList(),
        };
    }

    private sealed record Binding(string Parameter, string Control, string Accessor);

    /// <summary>Controls, besides the primary one, that Fora shows the same parameter in.</summary>
    private static Dictionary<string, List<Binding>> AlternatesByParameter(
        IEnumerable<Binding> all, IReadOnlyDictionary<string, Binding> primary)
    {
        // A control that already owns a parameter of its own is not an alternate view of another:
        // that is the load/save defect, reported separately, and listing it here would suggest
        // the panel should render the same value twice.
        var owned = primary.Values.Select(b => b.Control).ToHashSet(StringComparer.Ordinal);

        return all.Where(b => primary.TryGetValue(b.Parameter, out var p)
                              && !string.Equals(p.Control, b.Control, StringComparison.Ordinal)
                              && !owned.Contains(b.Control))
            .GroupBy(b => b.Parameter, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.DistinctBy(b => b.Control, StringComparer.Ordinal)
                    .OrderBy(b => b.Control, StringComparer.Ordinal)
                    .ToList(),
                StringComparer.Ordinal);
    }

    private sealed record Bindings(List<Binding> Primary, Dictionary<string, List<Binding>> Alternates);

    private static Bindings ReadBindings(
        ClassDeclarationSyntax form, Dictionary<string, DesignerControl> controls, List<UiGap> gaps)
    {
        // Load and save are collected separately so they can be compared. They should agree, and
        // where they do not, Fora has a copy-paste defect worth surfacing rather than inheriting.
        var loads = new Dictionary<string, Binding>(StringComparer.Ordinal);
        var saves = new Dictionary<string, Binding>(StringComparer.Ordinal);
        var everyBinding = new List<Binding>();

        // Every parameter the form mentions at all. A parameter is also read in places that are not
        // assignments — guards, validation, enabling other fields — so "seen but not bound" has to be
        // worked out once at the end rather than toggled as the walk goes.
        var seen = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var invocation in form.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax call
                || call.Name.Identifier.ValueText != ParameterAccessor
                || invocation.ArgumentList.Arguments.Count != 1
                || invocation.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax nameLiteral
                || !nameLiteral.IsKind(SyntaxKind.StringLiteralExpression))
            {
                continue;
            }

            var parameter = nameLiteral.Token.ValueText;
            seen.Add(parameter);

            // The accessor that follows tells the value's type: _GetBoolean, _GetInt, _GetDouble, _GetString.
            var accessor = (invocation.Parent as MemberAccessExpressionSyntax)?.Name.Identifier.ValueText ?? string.Empty;

            var assignment = invocation.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
            if (assignment is null)
            {
                continue;
            }

            // Loading reads into a control (control.Text = …_GetString); saving writes back
            // (…_SetString = control.Text). Prefer the load direction, which names the control on
            // the left, but accept the save direction when that is all there is.
            var readsIntoControl = assignment.Right.Contains(invocation);
            var side = readsIntoControl ? assignment.Left : assignment.Right;

            if (LeftmostField(side) is not { } controlName || !controls.TryGetValue(controlName, out var control))
            {
                continue;
            }

            var target = readsIntoControl ? loads : saves;
            var binding = new Binding(
                parameter,
                control.Name,
                readsIntoControl ? accessor : accessor.Replace("_Set", "_Get", StringComparison.Ordinal));

            target[parameter] = binding;
            everyBinding.Add(binding);
        }

        // Saving is what actually reaches the database, so where the two directions disagree the
        // save side names the control that truly owns the parameter.
        var byParameter = new Dictionary<string, Binding>(saves, StringComparer.Ordinal);

        foreach (var (parameter, load) in loads)
        {
            byParameter.TryAdd(parameter, load);
        }

        // A parameter reaching several controls is normal — a printer field's value is shown in
        // one of three editors depending on its data type. The real defect is the other way
        // round: a control that displays one parameter but writes back a different one. Opening
        // and saving such a screen silently overwrites the second parameter with the first's value.
        var loadedByControl = loads.Values
            .GroupBy(b => b.Control, StringComparer.Ordinal)
            .Where(g => g.Select(b => b.Parameter).Distinct(StringComparer.Ordinal).Count() == 1)
            .ToDictionary(g => g.Key, g => g.First().Parameter, StringComparer.Ordinal);

        foreach (var save in saves.Values)
        {
            if (loadedByControl.TryGetValue(save.Control, out var shown)
                && !string.Equals(shown, save.Parameter, StringComparison.Ordinal))
            {
                gaps.Add(new UiGap
                {
                    Kind = "bindingMismatch",
                    Subject = save.Parameter,
                    Detail = $"control '{save.Control}' displays '{shown}' but saves to '{save.Parameter}'; "
                             + "opening and saving this screen in Fora overwrites the second with the first's "
                             + "value. The save side is taken as authoritative here.",
                });
            }
        }

        foreach (var parameter in seen.Except(byParameter.Keys, StringComparer.Ordinal))
        {
            // Sifre is the known case: Fora decrypts it through a local before showing it, so the
            // value never flows straight into a control. It is excluded from our model anyway (D6).
            gaps.Add(new UiGap
            {
                Kind = "unboundParameter",
                Subject = parameter,
                Detail = "read in the form but never assigned straight to or from a designer control",
            });
        }

        return new Bindings(
            byParameter.Values.OrderBy(b => b.Parameter, StringComparer.Ordinal).ToList(),
            AlternatesByParameter(everyBinding, byParameter));
    }

    private static List<UiTab> BuildTabs(Dictionary<string, DesignerControl> controls)
    {
        var tabs = new List<UiTab>();

        foreach (var page in controls.Values.Where(c => c.IsTabPage))
        {
            var path = TabTitles(page, controls);
            tabs.Add(new UiTab
            {
                Name = page.Name,
                Title = page.Text ?? page.Name,
                Path = path,
                Order = page.TabOrder ?? 0,
            });
        }

        return tabs
            .OrderBy(t => string.Join(" / ", t.Path), StringComparer.Ordinal)
            .ThenBy(t => t.Order)
            .ThenBy(t => t.Name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Tab captions from the outermost tab down to <paramref name="from"/>, inclusive.</summary>
    private static List<string> TabTitles(DesignerControl from, Dictionary<string, DesignerControl> controls)
    {
        var titles = new List<string>();

        for (var current = from; current is not null; current = Parent(current, controls))
        {
            if (current.IsTabPage)
            {
                titles.Add(current.Text ?? current.Name);
            }
        }

        titles.Reverse();
        return titles;
    }

    private static DesignerControl? Parent(DesignerControl control, Dictionary<string, DesignerControl> controls) =>
        control.Parent is { } parent && controls.TryGetValue(parent, out var found) ? found : null;

    private sealed record Placed(UiParameter Parameter, IReadOnlyList<string> TabPath, int SortY, int SortX);

    private static Placed Place(
        Binding binding,
        string catalogMethod,
        IReadOnlyList<Binding> alternates,
        bool tabbed,
        Dictionary<string, DesignerControl> controls,
        Dictionary<string, UiTab> tabsByName,
        List<UiGap> gaps)
    {
        var control = controls[binding.Control];

        var alternateViews = alternates
            .Where(a => controls.ContainsKey(a.Control))
            .Select(a => new UiAlternate
            {
                Control = a.Control,
                ControlType = controls[a.Control].Type,
                Editor = EditorFor(controls[a.Control], a.Accessor, binding.Parameter),
            })
            .ToList();

        // Fora swaps the editor for some values depending on another parameter — a printer field
        // moves between a text box and two combo boxes according to its data type. Picking one of
        // them would silently drop the other modes, so the value is marked as needing an editor
        // of its own.
        var editor = alternateViews.Select(a => a.Editor)
            .Append(EditorFor(control, binding.Accessor, binding.Parameter))
            .Distinct(StringComparer.Ordinal)
            .Count() > 1
            ? EditorKinds.Composite
            : EditorFor(control, binding.Accessor, binding.Parameter);

        var tabPage = NearestTabPage(control, controls);
        var (y, x) = AbsolutePosition(control, controls);
        var (label, labelSource) = FindLabel(control, controls);

        if (tabPage is null && tabbed)
        {
            gaps.Add(new UiGap
            {
                Kind = "noTab",
                Subject = binding.Parameter,
                Detail = $"control '{control.Name}' is not inside any tab page",
            });
        }

        if (label is null)
        {
            gaps.Add(new UiGap
            {
                Kind = "noLabel",
                Subject = binding.Parameter,
                Detail = $"no caption or neighbouring label found for '{control.Name}' ({control.Type})",
            });
        }

        var path = tabPage is not null && tabsByName.TryGetValue(tabPage.Name, out var tab)
            ? tab.Path
            : [];

        return new Placed(
            new UiParameter
            {
                Parameter = binding.Parameter,
                CatalogMethod = catalogMethod,
                Label = label,
                Editor = editor,
                ReferenceKind = ReferenceKindFor(control),
                SecretSource = editor != EditorKinds.Secret
                    ? null
                    : control.IsSecret ? "designer" : "name",
                Options = control.Options
                    .Select(o => new UiOption { Value = o.Value, Label = o.Label })
                    .ToList(),
                Alternates = alternateViews,
                Tab = tabPage?.Name,
                TabPath = path,
                Control = control.Name,
                ControlType = control.Type,
                Order = 0, // assigned after sorting
                LabelSource = labelSource,
            },
            path,
            y,
            x);
    }

    private static DesignerControl? NearestTabPage(
        DesignerControl control, Dictionary<string, DesignerControl> controls)
    {
        for (var current = Parent(control, controls); current is not null; current = Parent(current, controls))
        {
            if (current.IsTabPage)
            {
                return current;
            }
        }

        return null;
    }

    /// <summary>
    /// Position relative to the enclosing tab page. Controls inside a <c>TableLayoutPanel</c>
    /// have no <c>Location</c>, so their cell is turned into an approximate offset — good enough
    /// to put fields in reading order, which is all this is used for.
    /// </summary>
    private static (int Y, int X) AbsolutePosition(
        DesignerControl control, Dictionary<string, DesignerControl> controls)
    {
        var y = 0;
        var x = 0;

        for (var current = control; current is not null && !current.IsTabPage; current = Parent(current, controls))
        {
            if (current.Row is { } row && current.Column is { } column)
            {
                y += row * CellHeight;
                x += column * CellWidth;
            }
            else
            {
                y += current.Y ?? 0;
                x += current.X ?? 0;
            }
        }

        return (y, x);
    }

    private static (string? Label, string Source) FindLabel(
        DesignerControl control, Dictionary<string, DesignerControl> controls)
    {
        // A CheckEdit states its own meaning; that is the best label available and needs no guessing.
        if (!string.IsNullOrWhiteSpace(control.Text) && control.Type == "CheckEdit")
        {
            return (control.Text, LabelSources.Caption);
        }

        var siblings = controls.Values
            .Where(c => c.Parent is not null
                        && string.Equals(c.Parent, control.Parent, StringComparison.Ordinal)
                        && !ReferenceEquals(c, control)
                        && !string.IsNullOrWhiteSpace(c.Text))
            .ToList();

        var labels = siblings.Where(c => c.IsLabel).ToList();

        if (siblings.Count == 0)
        {
            return (null, LabelSources.None);
        }

        // Inside a table the label sits in an earlier cell of the same row.
        if (control.Row is { } row && control.Column is { } column)
        {
            var cellLabel = labels
                .Where(c => c.Row == row && c.Column < column)
                .OrderByDescending(c => c.Column)
                .FirstOrDefault();

            return cellLabel is null ? (null, LabelSources.None) : (cellLabel.Text, LabelSources.TableCell);
        }

        if (control.X is not { } controlX || control.Y is not { } controlY)
        {
            return (null, LabelSources.None);
        }

        var height = control.Height ?? 0;

        // A label is aligned with its field's first line, not with the field's centre, so a tall
        // MemoEdit needs the whole vertical span considered rather than a band around the middle.
        var firstLine = controlY + Math.Min(height, SameRowTolerance * 2) / 2;

        var left = NearestOnTheLeft(labels, controlX, controlY, height, firstLine);
        if (left is not null)
        {
            return (left.Text, LabelSources.LabelLeft);
        }

        // Some fields qualify the check box next to them (a unit selector beside
        // "show remaining / total quantity"). That caption is the only label they have.
        var checkBox = NearestOnTheLeft(
            siblings.Where(c => c.Type == "CheckEdit").ToList(), controlX, controlY, height, firstLine);

        if (checkBox is not null)
        {
            return (checkBox.Text, LabelSources.CheckBoxLeft);
        }

        var above = labels
            .Where(c => c.Y is { } ly && ly < controlY && c.X is { } lx && Math.Abs(lx - controlX) <= CellWidth)
            .OrderByDescending(c => c.Y)
            .FirstOrDefault();

        return above is null ? (null, LabelSources.None) : (above.Text, LabelSources.LabelAbove);
    }

    /// <summary>
    /// Closest candidate to the left whose vertical centre falls within the field's span.
    /// Ties within one row are settled by proximity, so the nearest caption wins.
    /// </summary>
    private static DesignerControl? NearestOnTheLeft(
        List<DesignerControl> candidates, int controlX, int controlY, int height, int firstLine)
    {
        return candidates
            .Where(c => c.X is { } x && x < controlX && c.Y is not null)
            .Where(c =>
            {
                var centre = c.Y!.Value + (c.Height ?? 0) / 2;
                return centre >= controlY - SameRowTolerance
                       && centre <= controlY + height + SameRowTolerance;
            })
            .OrderBy(c => Math.Abs(c.Y!.Value + (c.Height ?? 0) / 2 - firstLine) / SameRowTolerance)
            .ThenByDescending(c => c.X)
            .FirstOrDefault();
    }

    /// <summary>
    /// Fora's bespoke picker controls, and the ERP list each one chooses from. The suffix
    /// "Secimi" is the giveaway; the list is named explicitly so a new picker fails the
    /// extraction rather than landing in a wrong bucket.
    /// </summary>
    private static readonly Dictionary<string, string> ReferenceKindByControl = new(StringComparer.Ordinal)
    {
        ["CariSecimi"] = "cari",
        ["DepoSecimi"] = "depo",
        ["KargoSecimi"] = "kargo",
        ["EkipKoduSecimi"] = "ekipKodu",
    };

    /// <summary>Controls that carry a value only a purpose-built screen can edit.</summary>
    private static bool IsComposite(string controlType) =>
        controlType.StartsWith("Rapor", StringComparison.Ordinal)
        && controlType.EndsWith("Secenekleri", StringComparison.Ordinal);

    /// <summary>
    /// Names that mean a credential. Used only as a fallback: Fora masks some of its password
    /// fields and not others, and a credential shown as plain text is the worse mistake.
    /// </summary>
    private static readonly System.Text.RegularExpressions.Regex CredentialName =
        new("sifre|password|parola", System.Text.RegularExpressions.RegexOptions.IgnoreCase
            | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    /// <summary>A credential only if the field actually holds one — not a "ask for a password?" flag.</summary>
    private static bool LooksLikeCredential(DesignerControl control, string parameter, string accessor) =>
        CredentialName.IsMatch(parameter)
        && BuiltInEditorFor(control.Type, accessor) is EditorKinds.Text or EditorKinds.MultilineText;

    private static string EditorFor(DesignerControl control, string accessor, string parameter = "") => control switch
    {
        // Fora masks the field itself, which beats guessing a credential from its name.
        { IsSecret: true } => EditorKinds.Secret,

        _ when LooksLikeCredential(control, parameter, accessor) => EditorKinds.Secret,

        // Either a bespoke picker control or a combo filled from an ERP table: both are codes
        // chosen from a live list, not free text (D12).
        _ when ReferenceKindFor(control) is not null => EditorKinds.Reference,

        _ when IsComposite(control.Type) => EditorKinds.Composite,
        _ => BuiltInEditorFor(control.Type, accessor),
    };

    /// <summary>
    /// Which ERP list backs this control, from its own type or from the data call that fills it.
    /// </summary>
    private static string? ReferenceKindFor(DesignerControl control)
    {
        if (ReferenceKindByControl.TryGetValue(control.Type, out var byType))
        {
            return byType;
        }

        if (control.ErpDataSource is not { } source)
        {
            return null;
        }

        return ErpDataSourceKinds.TryGetValue(source, out var kind)
            ? kind
            : throw new CatalogExtractionException(
                $"control '{control.Name}' is filled from '{source}Data', which is not mapped to an ERP list. "
                + $"Add it to {nameof(ErpDataSourceKinds)} after deciding what it selects.");
    }

    /// <summary>ERP data classes Fora fills combo boxes from, and the list each one returns.</summary>
    private static readonly Dictionary<string, string> ErpDataSourceKinds = new(StringComparer.Ordinal)
    {
        ["Depo"] = "depo",
        ["Cari"] = "cari",
        ["Kargo"] = "kargo",
        ["Ekip"] = "ekipKodu",
        ["MikroKullanici"] = "mikroKullanici",
        ["YaziciAyarlari"] = "yaziciSablonu",
    };

    private static string BuiltInEditorFor(string controlType, string accessor) => controlType switch
    {
        "CheckEdit" => EditorKinds.Boolean,
        "SpinEdit" or "CalcEdit" => accessor == "_GetDouble" ? EditorKinds.Decimal : EditorKinds.Integer,
        "ComboBox" or "ComboBoxEdit" or "LookUpEdit" or "ListBoxControl" => EditorKinds.Choice,
        "MemoEdit" => EditorKinds.MultilineText,
        "ColorPickEdit" => EditorKinds.Color,
        "TextEdit" or "TextBox" => accessor switch
        {
            "_GetInt" => EditorKinds.Integer,
            "_GetDouble" => EditorKinds.Decimal,
            _ => EditorKinds.Text,
        },
        _ => EditorKinds.Unknown,
    };

    /// <summary>Walks <c>a.b.c</c> down to <c>a</c>, so <c>CariKodu.Text</c> yields <c>CariKodu</c>.</summary>
    private static string? LeftmostField(ExpressionSyntax expression)
    {
        var current = expression;

        while (true)
        {
            switch (current)
            {
                case MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } member:
                    return member.Name.Identifier.ValueText;
                case MemberAccessExpressionSyntax member:
                    current = member.Expression;
                    continue;
                case InvocationExpressionSyntax invocation:
                    current = invocation.Expression;
                    continue;
                case IdentifierNameSyntax identifier:
                    return identifier.Identifier.ValueText;
                default:
                    return null;
            }
        }
    }
}
