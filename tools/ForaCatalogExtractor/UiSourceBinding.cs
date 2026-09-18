namespace ErpBridge.Tools.ForaCatalog;

/// <summary>
/// Works out which catalogue parameters a given editor screen is expected to show.
///
/// Parameter names are not unique across sets — 862 of the 3,365 distinct names appear in more
/// than one — so every name has to be tied to the set it came from before the layout can be read.
/// </summary>
public static class UiSourceBinding
{
    /// <param name="Declared">Parameter name to the catalogue set that declares it.</param>
    /// <param name="Required">
    /// Names the screen is the editor for, and which must therefore be placed or reported.
    /// </param>
    public sealed record Result(
        IReadOnlyDictionary<string, string> Declared,
        IReadOnlyCollection<string> Required);

    public static Result Describe(DefaultsCatalog defaults, ForaCatalogPaths.UiSource source)
    {
        var bySet = defaults.Sets.ToDictionary(
            s => s.CatalogMethod,
            s => s.Parameters.Select(p => p.Name).ToList(),
            StringComparer.Ordinal);

        var declared = new Dictionary<string, string>(StringComparer.Ordinal);
        var required = new HashSet<string>(StringComparer.Ordinal);

        foreach (var method in source.CatalogMethods.Concat(source.AlsoEdits ?? []))
        {
            if (!bySet.TryGetValue(method, out var names))
            {
                throw new CatalogExtractionException(
                    $"{source.TypeName}: catalogue set '{method}' does not exist.");
            }

            var owns = source.CatalogMethods.Contains(method, StringComparer.Ordinal);

            foreach (var name in names)
            {
                // Two sets edited by one screen must not both claim a name, or the panel could not
                // tell which parameter a field writes to.
                if (declared.TryGetValue(name, out var owner) && owner != method)
                {
                    throw new CatalogExtractionException(
                        $"{source.TypeName}: '{name}' is declared by both {owner} and {method}.");
                }

                declared[name] = method;

                if (owns)
                {
                    required.Add(name);
                }
            }
        }

        return new Result(declared, required);
    }
}
