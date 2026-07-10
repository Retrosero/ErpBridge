namespace ErpBridge.Erp.Mikro.Versioning;

/// <summary>
/// V15 identity strategy. The header row's RECno comes from SQL Server's
/// <c>SCOPE_IDENTITY()</c>; child rows use the <c>sth_sip_RECid_RECno</c> link
/// column (the corresponding <c>sth_sip_RECid_DBCno</c> is filled by the writer
/// based on the active database number).
/// </summary>
public sealed class RecnoStrategy : IMikroIdentityStrategy
{
    /// <summary>Column on child rows that links to the parent sipariş RECno.</summary>
    public const string HeaderLinkColumn = "sth_sip_RECid_RECno";

    /// <summary>Cast SCOPE_IDENTITY to INT for typed access.</summary>
    public string SelectScopeIdentitySql() => "SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>Generate a placeholder int — real value arrives from SQL Server.</summary>
    public object GenerateNewId() => 0;

    /// <summary>Set <c>sth_sip_RECid_RECno</c> on the child row.</summary>
    public void ApplyHeaderLinkFields(IDictionary<string, object?> row, object parentId)
    {
        ArgumentNullException.ThrowIfNull(row);
        row[HeaderLinkColumn] = parentId;
    }

    /// <inheritdoc />
    public string DisplayName => "V15/RECno";
}
