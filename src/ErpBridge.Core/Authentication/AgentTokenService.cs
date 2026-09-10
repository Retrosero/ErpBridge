using ErpBridge.Core.Stores;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Core.Authentication;

/// <summary>
/// Default <see cref="IAgentTokenService"/>. Registers against the licence key
/// held in <see cref="AgentConfig"/> and hands the resulting token to
/// <see cref="IAgentTokenSource"/>.
/// </summary>
public sealed class AgentTokenService : IAgentTokenService
{
    /// <summary>
    /// Renew this long before the recorded expiry. The sync loop ticks every
    /// 20 s, so a five-minute window gives many chances to renew before any
    /// request can be rejected, and absorbs modest clock skew between the agent
    /// and the server.
    /// </summary>
    public static readonly TimeSpan RenewalWindow = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Assumed lifetime when the central API does not report one. Deliberately
    /// short: guessing short costs one extra registration, guessing long costs an
    /// hour of silent 401s.
    /// </summary>
    private static readonly TimeSpan AssumedLifetime = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Shortest gap between two reactive refreshes.
    ///
    /// <para>Without it a 401 that renewal cannot cure — a request the server
    /// rejects for some reason other than the token's age — turns every failing
    /// call into another registration. The notify long-poll reconnects the
    /// instant it fails, so in production this reached ~100 registrations a
    /// minute and the central API rate-limited the agent with 429s. Renewing at
    /// most once every 30 s still recovers within a single sync tick.</para>
    /// </summary>
    public static readonly TimeSpan RefreshCooldown = TimeSpan.FromSeconds(30);

    private readonly IAgentConfigStore _configStore;
    private readonly IRemoteApiClient _remoteApi;
    private readonly IAgentTokenSource _tokenSource;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AgentTokenService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateTimeOffset? _lastRegisterAttemptUtc;

    /// <summary>DI constructor.</summary>
    public AgentTokenService(
        IAgentConfigStore configStore,
        IRemoteApiClient remoteApi,
        IAgentTokenSource tokenSource,
        ILogger<AgentTokenService> logger)
        : this(configStore, remoteApi, tokenSource, logger, TimeProvider.System)
    {
    }

    /// <summary>Test seam: inject a deterministic clock.</summary>
    public AgentTokenService(
        IAgentConfigStore configStore,
        IRemoteApiClient remoteApi,
        IAgentTokenSource tokenSource,
        ILogger<AgentTokenService> logger,
        TimeProvider timeProvider)
    {
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _tokenSource = tokenSource ?? throw new ArgumentNullException(nameof(tokenSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public async Task<bool> EnsureValidAsync(CancellationToken ct = default)
    {
        if (!NeedsRenewal()) return true;

        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // Re-check inside the gate: a concurrent caller may have renewed
            // while this one waited, and registering twice would leave the
            // loser using a token the winner already replaced.
            if (!NeedsRenewal()) return true;
            return await RegisterAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> RefreshAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_lastRegisterAttemptUtc is { } last
                && _timeProvider.GetUtcNow() - last < RefreshCooldown)
            {
                _logger.LogDebug(
                    "Skipping token refresh: the previous attempt was less than {Cooldown} ago.", RefreshCooldown);
                return false;
            }

            // The current token is deliberately left in place until a new one
            // arrives. Clearing first would make IJwtTokenProvider fall back to
            // the statically configured token — which is exactly the stale one
            // that is being rejected — for every request in flight meanwhile.
            return await RegisterAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool NeedsRenewal()
    {
        if (string.IsNullOrWhiteSpace(_tokenSource.CurrentJwt)) return true;
        // An unknown expiry is treated as "renew soon" by AssumedLifetime when
        // the token was adopted, so a null here means the token came from
        // somewhere that never recorded one — renew rather than trust it.
        if (_tokenSource.ExpiresAtUtc is not { } expiresAt) return true;
        return _timeProvider.GetUtcNow() >= expiresAt - RenewalWindow;
    }

    private async Task<bool> RegisterAsync(CancellationToken ct)
    {
        var config = await _configStore.LoadAsync(ct).ConfigureAwait(false);
        if (config is null || string.IsNullOrWhiteSpace(config.LicenseKey))
        {
            _logger.LogWarning("Cannot obtain an agent token: no licence key is configured.");
            return false;
        }

        var machineId = Environment.MachineName?.Trim();
        if (string.IsNullOrEmpty(machineId)) machineId = "unknown";

        // Stamped before the call, so a failing registration throttles the next
        // attempt just as a successful one does.
        _lastRegisterAttemptUtc = _timeProvider.GetUtcNow();

        AgentRegistrationResult result;
        try
        {
            result = await _remoteApi.RegisterAgentAsync(config.LicenseKey.Trim(), machineId, ct)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Agent registration failed; the previous token stays in place.");
            return false;
        }

        if (!result.Success || string.IsNullOrWhiteSpace(result.Jwt))
        {
            _logger.LogWarning(
                "Agent registration rejected: {Code} {Message}", result.ErrorCode, result.ErrorMessage);
            return false;
        }

        var expiresAt = result.ExpiresAtUtc ?? _timeProvider.GetUtcNow() + AssumedLifetime;
        _tokenSource.Set(result.Jwt, expiresAt);
        _logger.LogInformation(
            "Agent token renewed; valid until {ExpiresAtUtc:u} (machine {MachineId}).", expiresAt, machineId);

        // The tenant id travels with the registration and the change-set push
        // needs it. Persist it when the server reports one we do not have.
        var tenantId = result.TenantId == Guid.Empty ? null : result.TenantId.ToString();
        if (!string.IsNullOrWhiteSpace(tenantId)
            && !string.Equals(config.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
        {
            config.TenantId = tenantId;
            try
            {
                await _configStore.SaveAsync(config, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // The token itself is usable; losing the persisted tenant id
                // only costs a re-persist on the next registration.
                _logger.LogWarning(ex, "Agent token renewed but persisting the tenant id failed.");
            }
        }

        return true;
    }
}
