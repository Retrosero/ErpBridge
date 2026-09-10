namespace ErpBridge.Core.Authentication;

/// <summary>
/// Holds the bearer token the agent currently uses for outbound calls, together
/// with the moment it stops being valid.
///
/// <para>Before this existed the token lived only in configuration
/// (<c>CentralApi:Jwt</c>) and its expiry was thrown away, so nothing in the
/// process could tell a live token from a dead one. The central API issues
/// 60-minute tokens; an agent left running past that hour had every request
/// rejected with 401 and no way to notice — it kept preparing and discarding
/// full snapshots indefinitely.</para>
/// </summary>
public interface IAgentTokenSource
{
    /// <summary>The token to send, or <see langword="null"/> when none has been obtained.</summary>
    string? CurrentJwt { get; }

    /// <summary>When <see cref="CurrentJwt"/> stops being accepted, when known.</summary>
    DateTimeOffset? ExpiresAtUtc { get; }

    /// <summary>Adopt a freshly issued token.</summary>
    void Set(string jwt, DateTimeOffset? expiresAtUtc);

    /// <summary>Forget the current token, forcing the next caller to obtain a new one.</summary>
    void Clear();
}
