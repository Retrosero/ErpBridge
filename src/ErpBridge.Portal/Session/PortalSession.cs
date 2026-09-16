namespace ErpBridge.Portal.Session;

/// <summary>
/// The signed-in company user of one browser tab.
///
/// <para><b>Scoped, never singleton.</b> Blazor Server gives every circuit its own scope, so
/// each manager holds only their own token. A process-wide holder — as the operator console
/// can afford with a single operator — would hand one company's data to every visitor.</para>
/// </summary>
public sealed class PortalSession
{
    private readonly TimeProvider _time;

    public PortalSession(TimeProvider time) => _time = time;

    public string? Token { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public string TenantName { get; private set; } = string.Empty;
    public string TenantCode { get; private set; } = string.Empty;
    public string DataSource { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool CanApprove { get; private set; }

    public bool IsSignedIn => Token is not null && ExpiresAtUtc > _time.GetUtcNow();
    public bool IsAdmin => Role == Roles.Admin;

    /// <summary>Raised on sign-in and sign-out so the layout redraws.</summary>
    public event Action? Changed;

    public static class Roles
    {
        public const string Admin = "ADMIN";
        public const string Manager = "MANAGER";
        public const string Sales = "SALES";

        public static bool MayUsePortal(string role) => role is Admin or Manager;
    }

    public void SignIn(PortalSessionState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        Token = state.Token;
        ExpiresAtUtc = state.ExpiresAtUtc;
        TenantName = state.TenantName;
        TenantCode = state.TenantCode;
        DataSource = state.DataSource;
        Username = state.Username;
        FullName = state.FullName;
        Role = state.Role;
        CanApprove = state.CanApprove;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        Token = null;
        ExpiresAtUtc = default;
        TenantName = TenantCode = DataSource = Username = FullName = Role = string.Empty;
        CanApprove = false;
        Changed?.Invoke();
    }

    public PortalSessionState? Snapshot() => IsSignedIn
        ? new PortalSessionState(Token!, ExpiresAtUtc, TenantName, TenantCode, DataSource, Username, FullName, Role, CanApprove)
        : null;
}

/// <summary>What a tab keeps of its session between page reloads.</summary>
public sealed record PortalSessionState(
    string Token,
    DateTimeOffset ExpiresAtUtc,
    string TenantName,
    string TenantCode,
    string DataSource,
    string Username,
    string FullName,
    string Role,
    bool CanApprove);

/// <summary>Keeps a tab's session across page reloads without exposing the token to scripts.</summary>
public interface ISessionPersistence
{
    Task SaveAsync(PortalSessionState state);
    Task<PortalSessionState?> LoadAsync();
    Task ClearAsync();
}
