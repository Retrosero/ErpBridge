namespace ErpBridge.Erp.Mikro.Versioning;

/// <summary>
/// V16 identity strategy. The header Guid is generated in the application layer
/// via <see cref="Guid.NewGuid"/> and supplied at INSERT time; child rows are
/// linked through the single <c>sth_sip_uid</c> column.
/// </summary>
public sealed class GuidStrategy : IMikroIdentityStrategy
{
    /// <summary>Column on child rows that links to the parent sipariş Guid.</summary>
    public const string HeaderLinkColumn = "sth_sip_uid";

    /// <summary>V16 does not need a SCOPE_IDENTITY() round-trip.</summary>
    public string SelectScopeIdentitySql() => string.Empty;

    /// <summary>Generation in app — server <c>NEWID()</c> left as an alternate path.</summary>
    public string GenerateGuidSql() => "SELECT NEWID();";

    /// <summary>Hand out a fresh Guid at INSERT time.</summary>
    public object GenerateNewId() => Guid.NewGuid();

    /// <summary>Set <c>sth_sip_uid</c> on the child row.</summary>
    public void ApplyHeaderLinkFields(IDictionary<string, object?> row, object parentId)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (parentId is not Guid guid)
            throw new InvalidOperationException(
                $"GuidStrategy expected parentId to be a Guid; got {parentId.GetType().Name}.");
        row[HeaderLinkColumn] = guid;
    }

    /// <inheritdoc />
    public string DisplayName => "V16/Guid";
}
