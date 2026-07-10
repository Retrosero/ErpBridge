using System.Diagnostics;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Adapters;

/// <summary>
/// Coordinates the three Mikro connection-test phases (quick probe, version detection,
/// full diagnostic) and owns the short-lived cache that keeps repeated probes cheap.
/// </summary>
/// <remarks>
/// The orchestrator is the single seam used by the WPF "Bağlantıyı test et" button,
/// the Windows Service pre-flight check, and the <see cref="MikroAdapter"/> test
/// methods. Centralising the logic here means:
/// <list type="bullet">
///   <item>The cache TTL is owned in one place (<c>MikroConnectionTestOrchestrator.CacheTtl</c>) and is
///         invalidatable from any caller via <see cref="InvalidateCache"/>.</item>
///   <item>Password masking is applied at every error boundary, not just inside
///         <see cref="MikroAdapter"/>.</item>
///   <item>The <see cref="ErpConnectionTestResult"/> is filled in once, with all
///         rich fields (DetectedMikroVersion, IdentityStrategyName, LatencyMs) in
///         a single shape.</item>
/// </list>
/// </remarks>
public interface IMikroConnectionTestOrchestrator
{
    /// <summary>
    /// Run the full diagnostic — quick probe + version detection + strategy cache warming —
    /// and return the consolidated <see cref="ErpConnectionTestResult"/>. Always returns;
    /// never throws: failures are surfaced via <see cref="ErpConnectionTestResult.Ok"/>.
    /// </summary>
    Task<ErpConnectionTestResult> RunFullTestAsync(CancellationToken ct = default);

    /// <summary>
    /// Open a short-lived <see cref="SqlConnection"/> and report only
    /// <see cref="ErpConnectionTestResult.Ok"/> + <see cref="ErpConnectionTestResult.ServerVersion"/>.
    /// </summary>
    Task<ErpConnectionTestResult> RunQuickTestAsync(CancellationToken ct = default);

    /// <summary>
    /// Probe Mikro SQL for V15 vs V16, returning the cached result when fresh. Side-effect:
    /// warms <see cref="MikroIdentityStrategySelector"/> so subsequent writers don't re-probe.
    /// </summary>
    Task<ErpVersionInfo> RunVersionDetectionAsync(CancellationToken ct = default);

    /// <summary>
    /// Drop every cached entry so the next test forces a re-probe. Called when the WPF
    /// operator saves new connection settings or explicitly requests a refresh.
    /// </summary>
    void InvalidateCache();
}

/// <summary>
/// Default implementation of <see cref="IMikroConnectionTestOrchestrator"/>.
/// </summary>
/// <remarks>
/// Lifetime: registered as a singleton — the cache is process-wide. The TTL is intentionally
/// generous (30 minutes) so a busy agent doesn't repeatedly probe Mikro between unrelated
/// write bursts but short enough that a server-side schema upgrade is picked up within a
/// reasonable window.
/// </remarks>
public sealed class MikroConnectionTestOrchestrator : IMikroConnectionTestOrchestrator
{
    /// <summary>
    /// How long a cached <see cref="ErpVersionInfo"/> stays valid. The probe is cheap
    /// but not free — caching prevents back-to-back re-probes from hammering Mikro.
    /// </summary>
    public static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    private readonly MikroConnectionFactory _connectionFactory;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MikroConnectionTestOrchestrator> _logger;

    /// <summary>Build the orchestrator — every dependency is required.</summary>
    public MikroConnectionTestOrchestrator(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        IConfiguration configuration,
        ILogger<MikroConnectionTestOrchestrator> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ErpConnectionTestResult> RunFullTestAsync(CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var testedAt = DateTimeOffset.UtcNow;

        // Step 1 — quick probe. If it fails, surface the failure but still emit a result;
        // a soft failure (e.g. wrong password) shouldn't cascade into a NullRef later.
        var quick = await RunQuickTestAsync(ct).ConfigureAwait(false);
        if (!quick.Ok)
        {
            stopwatch.Stop();
            return quick with
            {
                TestedAtUtc = testedAt,
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }

        // Step 2 — version detection. Either path is allowed to throw InvalidOperationException
        // (missing config) — the catch below keeps the orchestrator's "never throw" contract.
        try
        {
            var info = await RunVersionDetectionAsync(ct).ConfigureAwait(false);
            var strategy = _strategySelector.GetFor(info.DatabaseName, info);

            stopwatch.Stop();
            return quick with
            {
                DetectedMikroVersion = info.Version,
                IdentityStrategyName = strategy.DisplayName,
                DatabaseName = info.DatabaseName,
                TestedAtUtc = testedAt,
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            // The version probe failed in a way that doesn't surface as a SqlException
            // (typically: Mikro configuration missing). Mask the message anyway — it
            // shouldn't contain a password but the policy is consistent.
            _logger.LogWarning(ex, "Mikro version detection failed during full test.");
            return quick with
            {
                Ok = false,
                Message = ConnectionStringMasker.MaskForLog(ex.Message),
                TestedAtUtc = testedAt,
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <inheritdoc />
    public async Task<ErpConnectionTestResult> RunQuickTestAsync(CancellationToken ct = default)
    {
        var settings = MikroConnectionSettings.FromConfiguration(_configuration);
        if (settings is null)
        {
            const string message =
                "Mikro konfigürasyonu eksik. 'Mikro:Server', 'Mikro:UserId' ve 'Mikro:DatabaseName' " +
                "alanlarını doldurup Kaydet'e basın.";
            _logger.LogWarning("Mikro connection test skipped — {Reason}", message);
            return new ErpConnectionTestResult(Ok: false, Message: message, ServerVersion: null);
        }

        var connectionString = _connectionFactory.BuildConnectionString(settings);
        var maskedConnectionString = ConnectionStringMasker.MaskPassword(connectionString);

        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var raw = conn.ServerVersion;
            await conn.CloseAsync().ConfigureAwait(false);

            _logger.LogInformation(
                "Mikro quick connection test succeeded for {Database} on {Server}. ServerVersion={ServerVersion}.",
                settings.DatabaseName, settings.Server, raw);

            return new ErpConnectionTestResult(
                Ok: true,
                Message: "Connection established.",
                ServerVersion: raw,
                DatabaseName: settings.DatabaseName);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or ArgumentException)
        {
            _logger.LogWarning(ex,
                "Mikro quick connection test failed for {Database} on {Server}. MaskedConn={MaskedConn}",
                settings.DatabaseName, settings.Server, maskedConnectionString);

            // Never echo the raw SqlException message verbatim — SqlException sometimes
            // embeds fragments of the connection string into the error text. Mask first.
            return new ErpConnectionTestResult(
                Ok: false,
                Message: ConnectionStringMasker.MaskForLog(ex.Message),
                ServerVersion: null,
                DatabaseName: settings.DatabaseName);
        }
    }

    /// <inheritdoc />
    public async Task<ErpVersionInfo> RunVersionDetectionAsync(CancellationToken ct = default)
    {
        var settings = MikroConnectionSettings.FromConfiguration(_configuration)
            ?? throw new InvalidOperationException(
                "Mikro konfigürasyonu eksik — version detection skipped. " +
                "'Mikro:Server', 'Mikro:UserId' ve 'Mikro:DatabaseName' ayarlanmalı.");

        var databaseName = settings.DatabaseName;

        // Cache short-circuit — honour the TTL. The selector stores ProbedAtUtc alongside
        // the version info; the orchestrator owns the TTL because it's a deployment knob,
        // not an adapter internal.
        var cached = _strategySelector.GetCachedWithTimestamp(databaseName);
        if (cached is { Info: not null, ProbedAtUtc: not null } hit)
        {
            var age = DateTime.UtcNow - hit.ProbedAtUtc.Value;
            if (age < CacheTtl)
            {
                _logger.LogInformation(
                    "Returning cached Mikro version for {Database}: {Version} (age {Age} < TTL {Ttl}).",
                    databaseName, hit.Info.Version, age, CacheTtl);
                return hit.Info;
            }

            _logger.LogInformation(
                "Cached Mikro version for {Database} is stale (age {Age} >= TTL {Ttl}); re-probing.",
                databaseName, age, CacheTtl);
        }

        var connectionString = _connectionFactory.BuildConnectionString(settings);
        var info = await _versionDetector.DetectAsync(connectionString, ct).ConfigureAwait(false);

        // Warm both caches in one call — strategy choice is derived from the version.
        _strategySelector.GetFor(databaseName, info);

        _logger.LogInformation(
            "Mikro version detected for {Database}: {Version} (raw={Raw}, v16Column={Col}).",
            info.DatabaseName, info.Version, info.RawVersion, info.IsV16);

        return info;
    }

    /// <inheritdoc />
    public void InvalidateCache()
    {
        _strategySelector.InvalidateAll();
        _logger.LogInformation("MikroConnectionTestOrchestrator cache invalidated.");
    }
}