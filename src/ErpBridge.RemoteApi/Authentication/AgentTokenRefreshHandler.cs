using System.Net;
using ErpBridge.Core.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ErpBridge.RemoteApi.Authentication;

/// <summary>
/// Notices that the central API rejected the agent's token and gets a new one,
/// so the next call succeeds.
///
/// <para><see cref="IAgentTokenService"/> renews on a schedule, which covers the
/// expiry the agent can predict. This handler covers the expiry it cannot: the
/// server restarted with a different signing key, the agent was revoked, or the
/// two clocks disagree. Without it those cases were unrecoverable — every
/// subsequent request returned 401 until somebody restarted the process.</para>
///
/// <para>The rejected request is deliberately <b>not</b> replayed. Bootstrap
/// uploads stream tens of megabytes and an <see cref="HttpRequestMessage"/>
/// cannot be sent twice, so retrying in place would mean buffering or cloning
/// the whole payload for a case that should be vanishingly rare. The caller
/// sees the 401 and its own cycle — 20 seconds away — runs with the new
/// token.</para>
/// </summary>
public sealed class AgentTokenRefreshHandler : DelegatingHandler
{
    private readonly IServiceProvider _services;
    private readonly IAgentTokenSource _tokenSource;
    private readonly ILogger<AgentTokenRefreshHandler> _logger;

    /// <summary>DI constructor.</summary>
    /// <param name="services">
    /// Resolved lazily: <see cref="IAgentTokenService"/> reaches the network
    /// through the very client this handler sits in, so taking it as a
    /// constructor dependency would close a loop through the handler pipeline.
    /// </param>
    /// <param name="tokenSource">Holds the token in flight, used to collapse a burst of 401s into one renewal.</param>
    /// <param name="logger">Diagnostics.</param>
    public AgentTokenRefreshHandler(
        IServiceProvider services,
        IAgentTokenSource tokenSource,
        ILogger<AgentTokenRefreshHandler> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _tokenSource = tokenSource ?? throw new ArgumentNullException(nameof(tokenSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Remember which token this request carried so a burst of parallel 401s
        // triggers one renewal rather than one per request: whoever renews first
        // changes CurrentJwt, and everyone still holding the old value stands
        // down.
        var tokenInFlight = _tokenSource.CurrentJwt;

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.Unauthorized) return response;

        // An unauthenticated agent (no token at all yet) is the scheduled
        // renewal's job, not this one's.
        if (string.IsNullOrWhiteSpace(tokenInFlight)) return response;
        if (!string.Equals(_tokenSource.CurrentJwt, tokenInFlight, StringComparison.Ordinal)) return response;

        _logger.LogWarning(
            "Central API rejected the agent token for {Path}; renewing it for the next call.",
            request.RequestUri?.AbsolutePath);

        try
        {
            var tokens = _services.GetRequiredService<IAgentTokenService>();
            await tokens.RefreshAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Renewal is best-effort here; the scheduled path retries shortly.
            _logger.LogWarning(ex, "Token renewal after a 401 failed.");
        }

        return response;
    }
}
