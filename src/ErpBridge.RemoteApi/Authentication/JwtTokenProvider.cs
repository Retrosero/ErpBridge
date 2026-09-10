using ErpBridge.Core.Authentication;
using ErpBridge.RemoteApi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.RemoteApi.Authentication;

/// <summary>
/// Resolves the bearer token to use for outbound calls.
/// </summary>
public interface IJwtTokenProvider
{
    string? GetToken();
}

/// <summary>
/// Prefers the token <see cref="IAgentTokenService"/> is keeping current, and
/// falls back to the statically configured <see cref="CentralApiOptions.Jwt"/>.
///
/// <para>The configured value used to be the only source, which meant the
/// process ran forever on whatever token it started with. The central API
/// issues 60-minute tokens, so an agent left running simply began failing every
/// call with 401 once that hour elapsed.</para>
/// </summary>
internal sealed class JwtTokenProvider : IJwtTokenProvider
{
    private readonly IAgentTokenSource _tokenSource;
    private readonly IOptionsMonitor<CentralApiOptions> _options;
    private readonly ILogger<JwtTokenProvider> _logger;

    public JwtTokenProvider(
        IAgentTokenSource tokenSource,
        IOptionsMonitor<CentralApiOptions> options,
        ILogger<JwtTokenProvider> logger)
    {
        _tokenSource = tokenSource;
        _options = options;
        _logger = logger;
    }

    public string? GetToken()
    {
        var current = _tokenSource.CurrentJwt;
        if (!string.IsNullOrWhiteSpace(current)) return current;

        var jwt = _options.CurrentValue.Jwt;
        if (string.IsNullOrWhiteSpace(jwt))
        {
            _logger.LogDebug("No JWT available; outbound calls will be unauthenticated.");
            return null;
        }
        return jwt;
    }
}
