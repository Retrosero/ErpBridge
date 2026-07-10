namespace ErpBridge.Erp.Mikro.Versioning;

/// <summary>
/// Abstracts the differences between Mikro V15 (RECno identity) and V16 (Guid
/// identity). All V15/V16 dispatcher logic lives behind this interface — the
/// rest of the adapter (writers, validators) is version-agnostic.
/// </summary>
public interface IMikroIdentityStrategy
{
    /// <summary>
    /// The SQL fragment that returns the just-inserted identity (V15:
    /// <c>SELECT CAST(SCOPE_IDENTITY() AS INT)</c>; V16: empty — the application
    /// supplied the Guid before INSERT).
    /// </summary>
    string SelectScopeIdentitySql();

    /// <summary>
    /// SQL used to obtain a new Guid server-side (V16 only). V15 returns an empty
    /// string because the identity column auto-increments.
    /// </summary>
    /// <remarks>Reserved for Phase 6 INSERT statements; present in the skeleton so
    /// the writer does not need to branch on version.</remarks>
    string GenerateGuidSql() => string.Empty;

    /// <summary>
    /// Returns the parameter prefix used for identity parameters. Both versions use
    /// <c>@</c> for SQL Server, but kept here so a future port to a different
    /// provider stays a single-line change.
    /// </summary>
    string ParameterPrefixForIdentity() => "@";

    /// <summary>
    /// Produces a new identity value in the application layer. V15 returns
    /// <c>0</c> (the real value comes from <c>SCOPE_IDENTITY()</c>); V16 returns
    /// <c>Guid.NewGuid()</c>.
    /// </summary>
    object GenerateNewId();

    /// <summary>
    /// On a child row, set the link field that points at the parent. V15 uses the
    /// <c>*_RECid_RECno</c> pair (the DB number is filled by the writer); V16 uses
    /// the single <c>*_uid</c> Guid.
    /// </summary>
    /// <param name="row">Mutable parameter bag passed to the INSERT — values live by name.</param>
    /// <param name="parentId">Parent identifier (int for V15, Guid for V16) that
    /// was obtained from the header insert.</param>
    void ApplyHeaderLinkFields(IDictionary<string, object?> row, object parentId);

    /// <summary>Diagnostic label for logs.</summary>
    string DisplayName { get; }
}
