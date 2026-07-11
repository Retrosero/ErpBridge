using System.Globalization;

namespace ErpBridge.Shared;

/// <summary>
/// Pure validation logic for the agent settings form. Lives in <c>Shared</c>
/// (not in the WPF assembly) so the WPF view-model can be unit-tested from
/// a headless <c>net8.0</c> test project that does not load
/// <c>PresentationCore</c>. The view-model is responsible for mirroring
/// the form fields into this helper.
/// </summary>
/// <remarks>
/// Rules enforced:
/// <list type="number">
///   <item><c>SqlServer</c>, <c>SqlUserName</c>, <c>MikroDatabaseName</c> — required, whitespace counts as missing.</item>
///   <item><c>CompanyNo</c>, <c>BranchNo</c> — must parse as a non-negative
///         32-bit integer; the form passes them as text so we tolerate locale
///         settings explicitly via <see cref="CultureInfo.InvariantCulture"/>.</item>
///   <item>The agent never validates the password here — empty passwords are
///         legitimate for trusted-auth / Windows-integrated SQL connections
///         and the adapter handles the resulting <see cref="Exception"/>.</item>
/// </list>
/// </remarks>
public static class AgentSettingsValidation
{
    /// <summary>
    /// Run the validation rules against the form fields. Returns <c>true</c>
    /// when every required field is present; populates <paramref name="error"/>
    /// with a Turkish user-visible message otherwise.
    /// </summary>
    /// <param name="sqlServer">SQL Server host or instance name (required).</param>
    /// <param name="sqlUserName">SQL login (required only when <paramref name="useWindowsAuth"/> is false).</param>
    /// <param name="mikroDatabaseName">Mikro database (required).</param>
    /// <param name="useWindowsAuth">True for Windows-auth / Trusted_Connection mode (SQL user/password are optional).</param>
    /// <param name="error">Populated with a Turkish user-visible error message when the method returns <c>false</c>.</param>
    public static bool TryValidate(
        string? sqlServer,
        string? sqlUserName,
        string? mikroDatabaseName,
        bool useWindowsAuth,
        out string error)
    {
        if (string.IsNullOrWhiteSpace(sqlServer))
        {
            error = "SQL Server boş olamaz.";
            return false;
        }

        if (!useWindowsAuth && string.IsNullOrWhiteSpace(sqlUserName))
        {
            error = "SQL kullanıcı adı boş olamaz (Windows Auth kapalıyken).";
            return false;
        }

        if (string.IsNullOrWhiteSpace(mikroDatabaseName))
        {
            error = "Mikro database adı boş olamaz.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryParseNonNegativeInt(string fieldDisplayName, string? value, out string error)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            error = fieldDisplayName + " boş olamaz.";
            return false;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            error = fieldDisplayName + " geçerli bir tamsayı olmalı.";
            return false;
        }

        if (parsed < 0)
        {
            error = fieldDisplayName + " negatif olamaz.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
