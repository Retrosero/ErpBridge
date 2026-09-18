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
    public const int SchemaVersion = 1;
    private const string TypeName = "ParametrelerDefault";
    private const string FactoryReturnType = "Parametreler";
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
    };

    /// <param name="sourceText">Contents of <c>ParametrelerDefault.cs</c>.</param>
    /// <param name="sourceFile">Repository-relative path recorded in the output.</param>
    /// <param name="sourceBuild">Fora build identifier, or "unknown".</param>
    public static DefaultsCatalog Extract(string sourceText, string sourceFile, string sourceBuild)
    {
        var root = CSharpSyntaxTree.ParseText(sourceText).GetRoot();

        var type = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .SingleOrDefault(c => c.Identifier.ValueText == TypeName)
            ?? throw new CatalogExtractionException($"class {TypeName} not found in {sourceFile}.");

        var sets = type.Members.OfType<MethodDeclarationSyntax>()
            .Where(m => (m.ReturnType as IdentifierNameSyntax)?.Identifier.ValueText == FactoryReturnType)
            .Select(ReadSet)
            .ToList();

        if (sets.Count == 0)
        {
            throw new CatalogExtractionException($"no {FactoryReturnType} factory methods found in {TypeName}.");
        }

        return new DefaultsCatalog
        {
            SchemaVersion = SchemaVersion,
            SourceBuild = sourceBuild,
            SourceType = $"Fora.Mikro.ParametreTanimlari.{TypeName}",
            SourceFile = sourceFile,
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

        var scoped = new[] { user, anaGrubu, altGrubu }.Where(f => f.IsScope).ToList();
        if (scoped.Count > 1)
        {
            throw new CatalogExtractionException(
                $"{methodName}: {scoped.Count} columns carry a method parameter ({string.Join(", ", scoped.Select(s => s.Field))}); exactly one or none is supported.");
        }

        var scope = scoped.Count == 1 ? scoped[0] : null;

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
            ScopeField = scope?.Field ?? ScopeFields.None,
            ScopeParameter = scope?.Value,
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

        return new ParameterDefault
        {
            Id = (int)idLiteral.Token.Value!,
            Name = StringLiteral(methodName, args[ArgName], nameof(ParameterDefault.Name), entry),
            Default = StringLiteral(methodName, args[ArgDefault], nameof(ParameterDefault.Default), entry),
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
