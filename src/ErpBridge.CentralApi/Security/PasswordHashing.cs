namespace ErpBridge.CentralApi.Security;

/// <summary>Shared by the staff and the catalog sign-ins.</summary>
internal static class PasswordHashing
{
    /// <summary>
    /// Verified against when the username or company is unknown, so a failed sign-in costs the same BCrypt work
    /// either way and does not reveal which part was wrong.
    /// </summary>
    public static readonly string Dummy = BCrypt.Net.BCrypt.HashPassword("erpbridge-timing-equalizer");
}
