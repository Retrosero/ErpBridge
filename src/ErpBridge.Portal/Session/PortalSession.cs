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

    /// <summary>Every role of the user, in <see cref="PortalRoles.All"/> order.</summary>
    public IReadOnlyList<string> Roles { get; private set; } = [];

    /// <summary>The server's effective right to decide approval requests (admin, approving manager, accounting).</summary>
    public bool CanApprove { get; private set; }

    /// <summary>Whether the browser keeps the session after the tab closes ("Beni hatırla").</summary>
    public bool RememberMe { get; private set; }

    /// <summary>
    /// When the roles were last read from the server. A session restored from the browser may be
    /// days old, and an administrator may have changed the roles since; see <see cref="NeedsRefresh"/>.
    /// </summary>
    public DateTimeOffset RolesReadAtUtc { get; private set; }

    /// <summary>How long the roles a page trusts may be old before they are read again.</summary>
    public static readonly TimeSpan RoleRefreshInterval = TimeSpan.FromMinutes(1);

    public bool NeedsRefresh => _time.GetUtcNow() - RolesReadAtUtc >= RoleRefreshInterval;

    public bool IsSignedIn => Token is not null && ExpiresAtUtc > _time.GetUtcNow();
    public bool IsAdmin => Roles.Contains(PortalRoles.Admin);

    public bool Allows(PortalArea area) => PortalRoles.Allows(Roles, area);

    /// <summary>The first page this user may open; see <see cref="PortalRoles.HomePage"/>.</summary>
    public string HomePage => PortalRoles.HomePage(Roles);

    /// <summary>Raised on sign-in and sign-out so the layout redraws.</summary>
    public event Action? Changed;

    /// <param name="fresh">True right after the server answered the login; false for a session restored from the browser.</param>
    public void SignIn(PortalSessionState state, bool fresh = false)
    {
        ArgumentNullException.ThrowIfNull(state);
        Token = state.Token;
        ExpiresAtUtc = state.ExpiresAtUtc;
        TenantName = state.TenantName;
        TenantCode = state.TenantCode;
        DataSource = state.DataSource;
        Username = state.Username;
        FullName = state.FullName;
        Roles = state.EffectiveRoles();
        CanApprove = state.CanApprove;
        RememberMe = state.RememberMe;
        RolesReadAtUtc = fresh ? _time.GetUtcNow() : DateTimeOffset.MinValue;
        Changed?.Invoke();
    }

    /// <summary>Takes the user's current name, roles and approval right from the server.</summary>
    public void Refresh(string fullName, IEnumerable<string> roles, bool canApprove)
    {
        var current = roles.ToHashSet(StringComparer.Ordinal);
        FullName = fullName;
        Roles = PortalRoles.All.Where(current.Contains).ToArray();
        CanApprove = canApprove;
        RolesReadAtUtc = _time.GetUtcNow();
        Changed?.Invoke();
    }

    public void SignOut()
    {
        Token = null;
        ExpiresAtUtc = default;
        TenantName = TenantCode = DataSource = Username = FullName = string.Empty;
        Roles = [];
        CanApprove = false;
        RememberMe = false;
        RolesReadAtUtc = default;
        Changed?.Invoke();
    }

    public PortalSessionState? Snapshot() => IsSignedIn
        ? new PortalSessionState(Token!, ExpiresAtUtc, TenantName, TenantCode, DataSource, Username, FullName, Legacy(Roles), CanApprove, [.. Roles], RememberMe)
        : null;

    private static string Legacy(IReadOnlyList<string> roles) =>
        roles.Contains(PortalRoles.Admin) ? PortalRoles.Admin : roles.Contains(PortalRoles.Manager) ? PortalRoles.Manager : PortalRoles.Sales;
}

/// <summary>What a browser keeps of its session between page reloads.</summary>
/// <param name="Role">The single role of the session format before multi-role accounts.</param>
/// <param name="Roles">Every role; <c>null</c> in a session saved before multi-role accounts.</param>
public sealed record PortalSessionState(
    string Token,
    DateTimeOffset ExpiresAtUtc,
    string TenantName,
    string TenantCode,
    string DataSource,
    string Username,
    string FullName,
    string Role,
    bool CanApprove,
    string[]? Roles = null,
    bool RememberMe = false)
{
    /// <summary>The roles, falling back to the single role a session saved by an older portal holds.</summary>
    public IReadOnlyList<string> EffectiveRoles()
    {
        var roles = Roles is { Length: > 0 } ? Roles : string.IsNullOrWhiteSpace(Role) ? [] : [Role];
        return PortalRoles.All.Where(roles.Contains).ToArray();
    }
}

/// <summary>Keeps a session across page reloads without exposing the token to scripts.</summary>
public interface ISessionPersistence
{
    /// <summary>Saves for the tab, or for the browser when <see cref="PortalSessionState.RememberMe"/> is set.</summary>
    Task SaveAsync(PortalSessionState state);
    Task<PortalSessionState?> LoadAsync();
    Task ClearAsync();
}
