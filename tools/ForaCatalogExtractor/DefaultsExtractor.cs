using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ErpBridge.Tools.ForaCatalog;

/// <summary>Raised when the decompiled source does not match the shape the extractor relies on.</summary>
public sealed class CatalogExtractionException(string message) : Exception(message);

/// <summary>
/// Reads <c>Fora.Mikro.ParametreTanimlari.ParametrelerDefault</c> and turns every
/// <c>new Parametre(...)</c> call into a catalogue entry.
///
/// Parsing goes through Roslyn rather than regular expressions on purpose: four of the
/// label templates end in an escaped quote (<c>…,N,\"</c>), which silently derails
/// line-based matching. Roslyn hands back the unescaped literal value.
/// </summary>
public static class DefaultsExtractor
{
    public const int SchemaVersion = 2;
    private const string EntryTypeName = "Parametre";
    private const int EntryArity = 7;

    // Argument positions of Parametre(Program, User, AnaGrubu, AltGrubu, ParametreID, ParametreAdi, DefaultDegeri).
    private const int ArgProgram = 0;
    private const int ArgUser = 1;
    private const int ArgAnaGrubu = 2;
    private const int ArgAltGrubu = 3;
    private const int ArgId = 4;
    private const int ArgName = 5;
    private const int ArgDefault = 6;

    /// <summary>
    /// What each factory method's scope value stands for. Fora carries no such marker,
    /// so the mapping is ours; an unlisted method fails the extraction rather than
    /// silently landing in a wrong bucket.
    /// </summary>
    private static readonly Dictionary<string, string> ScopeKindByMethod = new(StringComparer.Ordinal)
    {
        ["ForaMikro"] = ScopeKinds.None,
        ["ForaMikroKullanici"] = ScopeKinds.DesktopUser,
        ["MobilKullanici"] = ScopeKinds.MobileUser,
        ["B2B"] = ScopeKinds.None,
        ["ComarchEdiGenelParametreler"] = ScopeKinds.None,
        ["ComarchEdiIliskiParametreleri"] = ScopeKinds.EdiRelation,
        ["MobilRaporStokSatis"] = ScopeKinds.ReportCode,
        ["MobilRaporStokEnvanter"] = ScopeKinds.ReportCode,
        ["MobilRaporStokSiparis"] = ScopeKinds.ReportCode,
        ["MobilRaporYapilacakTahsilatlar"] = ScopeKinds.ReportCode,
        ["BankaAktarim"] = ScopeKinds.ImportTemplate,
        ["BankaAktarimGenel"] = ScopeKinds.ImportTemplate,
        ["BankaAktarimKriter"] = ScopeKinds.CriteriaName,
        ["GenelAktarimSqlSablon"] = ScopeKinds.ImportTemplate,
        ["GenelAktarimTxtCsvSablon"] = ScopeKinds.ImportTemplate,
        ["GenelAktarimKriter"] = ScopeKinds.CriteriaName,
        ["TahsilatAktarimSqlSablon"] = ScopeKinds.ImportTemplate,
        ["TahsilatAktarimTxtCsvSablon"] = ScopeKinds.ImportTemplate,
        ["TahsilatAktarimKriter"] = ScopeKinds.CriteriaName,

        // Printer templates are not declared in ParametrelerDefault: YaziciAyarlari builds them
        // imperatively, one set of page settings per template plus sixteen parameters per field.
        ["genelayarlaritanimla"] = ScopeKinds.PrinterTemplate,
        ["alanekle"] = ScopeKinds.PrinterTemplate,
    };

    /// <summary>One decompiled file that declares parameter sets.</summary>
    /// <param name="File">Repository-relative path, recorded in the output.</param>
    /// <param name="TypeName">Class that declares the sets.</param>
    /// <param name="Text">File contents.</param>
    public sealed record CatalogSource(string File, string TypeName, string Text);

    /// <summary>
    /// Reads every parameter set Fora declares. Most live in <c>ParametrelerDefault</c> as
    /// <c>Parametreler</c> factories; the printer templates are built imperatively in
    /// <c>YaziciAyarlari</c> instead, so both shapes are read here.
    /// </summary>
    /// <param name="sources">Files to read, in the order their sets should appear.</param>
    /// <param name="sourceBuild">Fora build identifier, or "unknown".</param>
    public static DefaultsCatalog Extract(IReadOnlyList<CatalogSource> sources, string sourceBuild)
    {
        var sets = new List<CatalogSet>();

        foreach (var source in sources)
        {
            var root = CSharpSyntaxTree.ParseText(source.Text).GetRoot();

            var type = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .SingleOrDefault(c => c.Identifier.ValueText == source.TypeName)
                ?? throw new CatalogExtractionException($"class {source.TypeName} not found in {source.File}.");

            var methods = type.Members.OfType<MethodDeclarationSyntax>()
                .Where(m => m.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
                    .Any(o => (o.Type as IdentifierNameSyntax)?.Identifier.ValueText == EntryTypeName))
                .ToList();

            if (methods.Count == 0)
            {
                throw new CatalogExtractionException(
                    $"{source.TypeName} in {source.File} declares no {EntryTypeName} entries.");
            }

            sets.AddRange(methods.Select(m => ReadSet(m) with { SourceFile = source.File }));
        }

        return new DefaultsCatalog
        {
            SchemaVersion = SchemaVersion,
            SourceBuild = sourceBuild,
            Sources = sources.Select(s => s.File).ToList(),
            SetCount = sets.Count,
            ParameterCount = sets.Sum(s => s.Parameters.Count),
            ShadowedCount = sets.Sum(s => s.Parameters.Count(p => p.Shadowed)),
            Sets = sets,
        };
    }

    private static CatalogSet ReadSet(MethodDeclarationSyntax method)
    {
        var methodName = method.Identifier.ValueText;

        var entries = method.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
            .Where(o => (o.Type as IdentifierNameSyntax)?.Identifier.ValueText == EntryTypeName)
            .ToList();

        if (entries.Count == 0)
        {
            throw new CatalogExtractionException($"{methodName}: no {EntryTypeName} entries found.");
        }

        if (!ScopeKindByMethod.TryGetValue(methodName, out var scopeKind))
        {
            throw new CatalogExtractionException(
                $"{methodName}: no scope kind mapped. Add it to {nameof(ScopeKindByMethod)} after deciding what its scope means.");
        }

        // The first four arguments must be identical across every entry of a method:
        // they are the addressing columns, and a set that disagrees with itself cannot
        // be stored under one key.
        var program = UniformConstant(methodName, entries, ArgProgram, nameof(CatalogSet.Program));
        var user = UniformField(methodName, entries, ArgUser, ScopeFields.User);
        var anaGrubu = UniformField(methodName, entries, ArgAnaGrubu, ScopeFields.AnaGrubu);
        var altGrubu = UniformField(methodName, entries, ArgAltGrubu, ScopeFields.AltGrubu);

        // Most sets are addressed through one column, but a printer template needs two: the
        // template name in ParametreUser and the field name in ParametreAltGrubu.
        var scopes = new[] { user, anaGrubu, altGrubu }
            .Where(f => f.IsScope)
            .Select(f => new ScopeColumn { Field = f.Field, Source = f.Value })
            .ToList();

        // Fora's own catalogue is not free of id collisions: TahsilatAktarimTxtCsvSablon gives
        // belge_tarihi_yil/ay/gun_baslangic the same id 16, where the general import template
        // correctly uses 16, 18 and 20. Because _GetParametre(int) returns the first match, the
        // later two can never be read back. Record them instead of dropping or rejecting them —
        // losing entries would make the catalogue lie about what Fora declares.
        var claimed = new HashSet<int>();
        var parameters = entries
            .Select(e => ReadParameter(methodName, e))
            .Select(p => claimed.Add(p.Id) ? p : p with { Shadowed = true })
            .ToList();

        var duplicateIds = parameters
            .GroupBy(p => p.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Order()
            .ToList();

        // _GetParametre(string) also returns the first match, so a name declared twice under two
        // ids leaves the later copy unreachable: the editor binds the first and the second keeps
        // its default forever. Record it for the same reason as the id collisions.
        var duplicateNames = parameters
            .GroupBy(p => p.Name, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new CatalogSet
        {
            CatalogMethod = methodName,
            Program = program,
            ScopeKind = scopeKind,
            Scopes = scopes,
            User = user.IsScope ? string.Empty : user.Value,
            AnaGrubu = anaGrubu.IsScope ? string.Empty : anaGrubu.Value,
            AltGrubu = altGrubu.IsScope ? string.Empty : altGrubu.Value,
            DuplicateIds = duplicateIds,
            DuplicateNames = duplicateNames,
            Parameters = parameters,
        };
    }

    private static ParameterDefault ReadParameter(string methodName, ObjectCreationExpressionSyntax entry)
    {
        var args = Arguments(methodName, entry);

        if (args[ArgId] is not LiteralExpressionSyntax idLiteral || !idLiteral.IsKind(SyntaxKind.NumericLiteralExpression))
        {
            throw new CatalogExtractionException(
                $"{methodName}: ParametreID is not a numeric literal at {Location(entry)} — got '{args[ArgId]}'.");
        }

        // A default is normally a literal, but a printer template field defaults its caption to
        // the field name it is created with, so an identifier has to be accepted there too.
        var (defaultValue, defaultSource) = args[ArgDefault] switch
        {
            LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression)
                => (literal.Token.ValueText, (string?)null),
            IdentifierNameSyntax identifier => (string.Empty, identifier.Identifier.ValueText),
            _ => throw new CatalogExtractionException(
                $"{methodName}: default is neither a string literal nor an identifier at {Location(entry)} — got '{args[ArgDefault]}'."),
        };

        return new ParameterDefault
        {
            Id = (int)idLiteral.Token.Value!,
            Name = StringLiteral(methodName, args[ArgName], nameof(ParameterDefault.Name), entry),
            Default = defaultValue,
            DefaultSource = defaultSource,
        };
    }

    private sealed record FieldValue(string Field, string Value, bool IsScope);

    private static FieldValue UniformField(
        string methodName, List<ObjectCreationExpressionSyntax> entries, int position, string field)
    {
        var first = Arguments(methodName, entries[0])[position];
        var isScope = first is IdentifierNameSyntax;

        foreach (var entry in entries)
        {
            var arg = Arguments(methodName, entry)[position];
            var sameShape = isScope ? arg is IdentifierNameSyntax : arg is LiteralExpressionSyntax;
            if (!sameShape || Describe(arg) != Describe(first))
            {
                throw new CatalogExtractionException(
                    $"{methodName}: column '{field}' is not uniform — '{Describe(first)}' at the first entry but '{Describe(arg)}' at {Location(entry)}.");
            }
        }

        var value = isScope
            ? ((IdentifierNameSyntax)first).Identifier.ValueText
            : StringLiteral(methodName, first, field, entries[0]);

        return new FieldValue(field, value, isScope);
    }

    private static string UniformConstant(
        string methodName, List<ObjectCreationExpressionSyntax> entries, int position, string field)
    {
        var value = UniformField(methodName, entries, position, field);
        if (value.IsScope)
        {
            throw new CatalogExtractionException($"{methodName}: column '{field}' must be a constant, not the method parameter '{value.Value}'.");
        }

        return value.Value;
    }

    private static IReadOnlyList<ExpressionSyntax> Arguments(string methodName, ObjectCreationExpressionSyntax entry)
    {
        var args = entry.ArgumentList?.Arguments;
        if (args is null || args.Value.Count != EntryArity)
        {
            throw new CatalogExtractionException(
                $"{methodName}: expected {EntryArity} arguments at {Location(entry)}, found {args?.Count ?? 0}.");
        }

        return args.Value.Select(a => a.Expression).ToList();
    }

    private static string StringLiteral(
        string methodName, ExpressionSyntax expression, string field, ObjectCreationExpressionSyntax entry)
    {
        if (expression is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            throw new CatalogExtractionException(
                $"{methodName}: '{field}' is not a string literal at {Location(entry)} — got '{expression}'.");
        }

        // ValueText is the decoded value, so "…,N,\"" arrives as …,N," rather than the raw escape.
        return literal.Token.ValueText;
    }

    private static string Describe(ExpressionSyntax expression) => expression switch
    {
        IdentifierNameSyntax identifier => $"param:{identifier.Identifier.ValueText}",
        LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) => $"const:{literal.Token.ValueText}",
        _ => $"other:{expression}",
    };

    private static int Location(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
}
