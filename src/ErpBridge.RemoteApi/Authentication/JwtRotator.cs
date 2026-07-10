using ErpBridge.RemoteApi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.RemoteApi.Authentication;

/// <summary>
/// Resolves the bearer token to use for outbound calls. MVP reads from
/// <see cref="CentralApiOptions.Jwt"/>; later phases may rotate/refresh from disk
/// or the registration endpoint.
/// </summary>
public interface IJwtTokenProvider
{
    string? GetToken();
}

internal sealed class JwtTokenProvider : IJwtTokenProvider
{
    private readonly IOptionsMonitor<CentralApiOptions> _options;
    private readonly ILogger<JwtTokenProvider> _logger;

    public JwtTokenProvider(IOptionsMonitor<CentralApiOptions> options, ILogger<JwtTokenProvider> logger)
    {
        _options = options;
        _logger = logger;
    }

    public string? GetToken()
    {
        var jwt = _options.CurrentValue.Jwt;
        if (string.IsNullOrWhiteSpace(jwt))
        {
            _logger.LogDebug("No JWT configured; outbound calls will be unauthenticated.");
            return null;
        }
        return jwt;
    }
}
