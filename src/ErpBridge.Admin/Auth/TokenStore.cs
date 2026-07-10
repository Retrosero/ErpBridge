namespace ErpBridge.Admin.Auth;

/// <summary>
/// Process-wide in-memory holder for the admin JWT. The Admin panel does not
/// persist the token across restarts on purpose — admin credentials live in
/// the central API, not in the panel. Singleton-scoped.
/// </summary>
public sealed class TokenStore
{
    private string? _token;
    private string? _email;
    private string? _displayName;
    private Guid _adminId;
    private DateTimeOffset _expiresAtUtc;

    private readonly object _gate = new();

    /// <summary>Fired whenever the stored token changes (login or logout).</summary>
    public event EventHandler? Changed;

    public string? GetToken()
    {
        lock (_gate) return _token;
    }

    /// <summary>Alias of <see cref="GetToken"/> for clarity at call sites.</summary>
    public string? Get() => GetToken();

    public string? GetEmail()
    {
        lock (_gate) return _email;
    }

    public string? GetDisplayName()
    {
        lock (_gate) return _displayName;
    }

    public Guid GetAdminId()
    {
        lock (_gate) return _adminId;
    }

    public DateTimeOffset GetExpiresAtUtc()
    {
        lock (_gate) return _expiresAtUtc;
    }

    public void Set(string token, Guid adminId, string email, string displayName, DateTimeOffset expiresAtUtc)
    {
        lock (_gate)
        {
            _token = token;
            _adminId = adminId;
            _email = email;
            _displayName = displayName;
            _expiresAtUtc = expiresAtUtc;
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        lock (_gate)
        {
            _token = null;
            _email = null;
            _displayName = null;
            _adminId = Guid.Empty;
            _expiresAtUtc = DateTimeOffset.MinValue;
        }
        Changed?.Invoke(this, EventArgs.Empty);
    }
}