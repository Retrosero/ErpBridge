namespace ErpBridge.Core.Authentication;

/// <summary>
/// Keeps the agent's bearer token usable: obtains one when there is none and
/// replaces it before the central API starts rejecting it.
///
/// <para>The central API mints 60-minute tokens and offers agents no refresh
/// endpoint — the only way to get a new one is to register again. Nothing did
/// that on a schedule, and every "am I registered?" check in the agent asked
/// only whether a token <i>string</i> existed, never whether it still worked.
/// An agent therefore ran fine for an hour and was then permanently dead:
/// uploads, status probes and the notify long-poll all returned 401 forever,
/// and no button in the UI could recover it because they all short-circuited on
/// the stale token being non-empty.</para>
/// </summary>
public interface IAgentTokenService
{
    /// <summary>
    /// Make sure a token is present and not about to expire, registering again
    /// when needed. Cheap to call on every cycle — it only reaches the network
    /// when the token is missing or inside the renewal window.
    /// </summary>
    /// <returns><see langword="true"/> when a usable token is in place.</returns>
    Task<bool> EnsureValidAsync(CancellationToken ct = default);

    /// <summary>
    /// Discard the current token and register again regardless of its recorded
    /// expiry. For the case the expiry cannot predict: the server rejected the
    /// token anyway (restarted with a new signing key, agent revoked, clock
    /// skew).
    /// </summary>
    /// <returns><see langword="true"/> when a new token was obtained.</returns>
    Task<bool> RefreshAsync(CancellationToken ct = default);
}
