namespace ErpBridge.Core.Authentication;

/// <summary>
/// Default <see cref="IAgentTokenSource"/>: keeps the token in memory for the
/// life of the process. The agent re-registers on startup anyway, so nothing is
/// lost by not persisting it — and a token that outlives the process it was
/// minted for is a liability, not an asset.
/// </summary>
public sealed class InMemoryAgentTokenSource : IAgentTokenSource
{
    private readonly Lock _gate = new();
    private string? _jwt;
    private DateTimeOffset? _expiresAtUtc;

    /// <inheritdoc />
    public string? CurrentJwt
    {
        get { lock (_gate) return _jwt; }
    }

    /// <inheritdoc />
    public DateTimeOffset? ExpiresAtUtc
    {
        get { lock (_gate) return _expiresAtUtc; }
    }

    /// <inheritdoc />
    public void Set(string jwt, DateTimeOffset? expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jwt);
        lock (_gate)
        {
            _jwt = jwt;
            _expiresAtUtc = expiresAtUtc;
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (_gate)
        {
            _jwt = null;
            _expiresAtUtc = null;
        }
    }
}
