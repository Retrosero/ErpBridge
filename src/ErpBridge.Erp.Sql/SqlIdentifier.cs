namespace ErpBridge.Erp.Sql;

/// <summary>
/// Guard for the few places where a SQL identifier (table or column name) has to
/// be interpolated into generated DDL/DML rather than bound as a parameter.
///
/// <para>
/// Identifiers always originate from an <see cref="Abstractions.ChangeLog.IErpTrackedTableCatalog"/>
/// — compiled-in constants, never user or payload data — so this is a
/// defence-in-depth assertion rather than the primary control. Every *value*
/// still travels as a Dapper parameter.
/// </para>
/// </summary>
public static class SqlIdentifier
{
    /// <summary>Maximum length of a SQL Server regular identifier.</summary>
    public const int MaxLength = 128;

    /// <summary>
    /// Return <paramref name="identifier"/> unchanged when it is a plain
    /// identifier (letters, digits, <c>_</c>, <c>#</c>, <c>$</c>, first char not
    /// a digit); throw otherwise.
    /// </summary>
    /// <exception cref="ArgumentException">The identifier is empty, too long, or contains an unsafe character.</exception>
    public static string Validate(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("SQL identifier must not be null or blank.", nameof(identifier));
        }

        if (identifier.Length > MaxLength)
        {
            throw new ArgumentException(
                $"SQL identifier '{identifier}' exceeds {MaxLength} characters.", nameof(identifier));
        }

        if (char.IsDigit(identifier[0]))
        {
            throw new ArgumentException(
                $"SQL identifier '{identifier}' must not start with a digit.", nameof(identifier));
        }

        foreach (var c in identifier)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '#' && c != '$')
            {
                throw new ArgumentException(
                    $"SQL identifier '{identifier}' contains the unsafe character '{c}'.", nameof(identifier));
            }
        }

        return identifier;
    }

    /// <summary>True when <paramref name="identifier"/> would pass <see cref="Validate"/>.</summary>
    public static bool IsValid(string? identifier)
    {
        try
        {
            Validate(identifier);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
