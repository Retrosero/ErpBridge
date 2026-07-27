using System.Globalization;
using System.Text.Json;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Mikro.ChangeTracking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>Continuously detects Mikro changes and pushes only affected sections.</summary>
public sealed class LiveSyncWorker : BackgroundService
{
    private static readonly TimeSpan ChangeTrackingPoll = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan CompatibilityPoll = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan Debounce = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan CompatibilityReconcile = TimeSpan.FromHours(6);

    private readonly IMikroChangeMonitor _monitor;
    private readonly IBootstrapSyncService _sync;
    private readonly IAgentConfigStore _configStore;
    private readonly ICheckpointStore _checkpointStore;
    private readonly IRemoteApiClient _remoteApi;
    private readonly ILogger<LiveSyncWorker> _logger;
    private DateTimeOffset _lastCompatibilityReconcile = DateTimeOffset.MinValue;
    private string? _registeredLicenseKey;

    public LiveSyncWorker(
        IMikroChangeMonitor monitor,
        IBootstrapSyncService sync,
        IAgentConfigStore configStore,
        ICheckpointStore checkpointStore,
        IRemoteApiClient remoteApi,
        ILogger<LiveSyncWorker> logger)
    {
        _monitor = monitor;
        _sync = sync;
        _configStore = configStore;
        _checkpointStore = checkpointStore;
        _remoteApi = remoteApi;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay = ChangeTrackingPoll;
            try
            {
                var config = await _configStore.LoadAsync(stoppingToken).ConfigureAwait(false);
                if (config is null || string.IsNullOrWhiteSpace(config.MikroDatabaseName))
                {
                    await SaveStatusAsync("waiting", "unknown", null, null,
                        "Agent ayarları bekleniyor.", "unknown", stoppingToken).ConfigureAwait(false);
                    delay = CompatibilityPoll;
                }
                else
                {
                    if (!await EnsureRegisteredAsync(config, stoppingToken).ConfigureAwait(false))
                        throw new InvalidOperationException("Lisans sunucusuna agent kaydı yapılamadı.");
                    var tenantId = config.TenantId
                        ?? throw new InvalidOperationException("Agent registration returned no tenant id.");
                    var versionScope = LiveSyncScopes.ChangeVersion(config.MikroDatabaseName);
                    var checkpoint = await _checkpointStore.LoadAsync(
                        tenantId, versionScope, stoppingToken).ConfigureAwait(false);
                    var lastVersion = ParseVersion(checkpoint?.LastToken);
                    var batch = await _monitor.PollAsync(lastVersion, stoppingToken).ConfigureAwait(false);
                    delay = batch.Mode == MikroChangeMonitorMode.ChangeTracking
                        ? ChangeTrackingPoll : CompatibilityPoll;

                    var needsReconcile = batch.Mode == MikroChangeMonitorMode.Compatibility
                        && DateTimeOffset.UtcNow - _lastCompatibilityReconcile >= CompatibilityReconcile;
                    var detectedAt = batch.Sections.Count > 0 ? DateTimeOffset.UtcNow : (DateTimeOffset?)null;
                    if (batch.RequiresFullBootstrap || checkpoint is null || needsReconcile)
                    {
                        var result = await _sync.RunOnceAsync(stoppingToken).ConfigureAwait(false);
                        if (!result.Success)
                            throw new InvalidOperationException(result.ErrorMessage ?? result.ErrorCode ?? "Bootstrap failed.");
                        _lastCompatibilityReconcile = DateTimeOffset.UtcNow;
                        await SaveVersionAsync(tenantId, versionScope, batch.CurrentVersion, stoppingToken).ConfigureAwait(false);
                        await SaveStatusAsync("watching", ModeName(batch.Mode), detectedAt, DateTimeOffset.UtcNow,
                            batch.Warning, tenantId, stoppingToken).ConfigureAwait(false);
                    }
                    else if (batch.Sections.Count > 0)
                    {
                        var detectionTime = DateTime.UtcNow;
                        foreach (var section in batch.Sections)
                        {
                            await _checkpointStore.SaveAsync(new CheckpointRecord
                            {
                                TenantId = tenantId,
                                SyncScope = LiveSyncScopes.Detected(section),
                                LastSuccessAt = detectionTime,
                                UpdatedAt = detectionTime,
                            }, stoppingToken).ConfigureAwait(false);
                        }
                        await Task.Delay(Debounce, stoppingToken).ConfigureAwait(false);
                        var successful = true;
                        foreach (var section in batch.Sections.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
                        {
                            var result = await _sync.PushSectionAsync(section, stoppingToken).ConfigureAwait(false);
                            successful &= result.Success;
                            if (!result.Success)
                            {
                                _logger.LogWarning("Live section sync failed for {Section}: {Code} {Message}",
                                    section, result.ErrorCode, result.ErrorMessage);
                                break;
                            }
                        }
                        if (!successful)
                            throw new InvalidOperationException("One or more live section transfers failed.");

                        await SaveVersionAsync(tenantId, versionScope, batch.CurrentVersion, stoppingToken).ConfigureAwait(false);
                        await SaveStatusAsync("watching", ModeName(batch.Mode), detectedAt, DateTimeOffset.UtcNow,
                            batch.Warning, tenantId, stoppingToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await SaveStatusAsync("watching", ModeName(batch.Mode), null, null,
                            batch.Warning, tenantId, stoppingToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Live synchronization cycle failed.");
                var config = await _configStore.LoadAsync(CancellationToken.None).ConfigureAwait(false);
                await SaveStatusAsync("error", "unknown", null, null, ex.Message,
                    config?.TenantId ?? "unknown", CancellationToken.None).ConfigureAwait(false);
                delay = CompatibilityPoll;
            }

            try
            {
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task<bool> EnsureRegisteredAsync(AgentConfig config, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(config.LicenseKey) || string.IsNullOrWhiteSpace(config.ApiBaseUrl))
            return false;
        if (string.Equals(_registeredLicenseKey, config.LicenseKey, StringComparison.Ordinal))
            return true;

        _remoteApi.ConfigureSession(config.ApiBaseUrl, null);
        var registration = await _remoteApi.RegisterAgentAsync(
            config.LicenseKey, Environment.MachineName, ct).ConfigureAwait(false);
        if (!registration.Success || string.IsNullOrWhiteSpace(registration.Jwt))
            return false;

        _remoteApi.ConfigureSession(config.ApiBaseUrl, registration.Jwt);
        _registeredLicenseKey = config.LicenseKey;
        var tenant = registration.TenantId.ToString();
        if (!string.Equals(config.TenantId, tenant, StringComparison.OrdinalIgnoreCase))
        {
            config.TenantId = tenant;
            await _configStore.SaveAsync(config, ct).ConfigureAwait(false);
        }
        return true;
    }

    private async Task SaveVersionAsync(string tenantId, string scope, long version, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        await _checkpointStore.SaveAsync(new CheckpointRecord
        {
            TenantId = tenantId,
            SyncScope = scope,
            LastSuccessAt = now,
            LastToken = version.ToString(CultureInfo.InvariantCulture),
            UpdatedAt = now,
        }, ct).ConfigureAwait(false);
    }

    private async Task SaveStatusAsync(
        string status,
        string mode,
        DateTimeOffset? detected,
        DateTimeOffset? transferred,
        string? message,
        string tenantId,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var previous = await _checkpointStore.LoadAsync(tenantId, LiveSyncScopes.Status, ct).ConfigureAwait(false);
        LiveSyncState? old = null;
        if (!string.IsNullOrWhiteSpace(previous?.LastToken))
        {
            try { old = JsonSerializer.Deserialize<LiveSyncState>(previous.LastToken); }
            catch (JsonException) { }
        }
        var state = new LiveSyncState(
            status,
            mode,
            detected ?? old?.LastDetectedAtUtc,
            transferred ?? old?.LastTransferredAtUtc,
            message);
        await _checkpointStore.SaveAsync(new CheckpointRecord
        {
            TenantId = tenantId,
            SyncScope = LiveSyncScopes.Status,
            LastSuccessAt = status == "watching" ? now : previous?.LastSuccessAt,
            LastToken = JsonSerializer.Serialize(state),
            UpdatedAt = now,
        }, ct).ConfigureAwait(false);
    }

    private static long? ParseVersion(string? value) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version)
            ? version : null;

    private static string ModeName(MikroChangeMonitorMode mode) =>
        mode == MikroChangeMonitorMode.ChangeTracking ? "change-tracking" : "compatibility";
}
