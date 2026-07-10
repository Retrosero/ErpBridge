using System.Collections.Concurrent;
using ErpBridge.Erp.Abstractions;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Versioning;

/// <summary>
/// Picks the right <see cref="IMikroIdentityStrategy"/> for a database, caching both
/// the strategy choice and the raw <see cref="ErpVersionInfo"/> per database name so
/// version-detection runs at most once per process per DB (within the configured TTL).
/// </summary>
/// <remarks>
/// Two independent caches live here:
/// <list type="bullet">
///   <item><c>_strategies</c> — maps database name to the resolved <see cref="IMikroIdentityStrategy"/>.</item>
///   <item><c>_versions</c> — maps database name to the last <see cref="ErpVersionInfo"/> we probed, with a probe
///         timestamp so callers can decide whether the result is still fresh.</item>
/// </list>
/// Both are <see cref="ConcurrentDictionary{TKey,TValue}"/> instances with case-insensitive keys — Mikro
/// database names on the same SQL Server can be referenced in mixed case from different tools, and the
/// selector should still treat them as the same key.
/// </remarks>
public sealed class MikroIdentityStrategySelector
{
    private readonly ILogger<MikroIdentityStrategySelector> _logger;
    private readonly ConcurrentDictionary<string, IMikroIdentityStrategy> _strategies = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, CachedVersion> _versions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// A probe result remembered with the moment it was recorded so callers can age it
    /// against a TTL. Stored as a private nested record so the rest of the codebase
    /// only sees <see cref="ErpVersionInfo"/> going out.
    /// </summary>
    private sealed record CachedVersion(ErpVersionInfo Info, DateTime ProbedAtUtc);

    /// <summary>The two concrete strategies held as singletons — they have no state.</summary>
    private static readonly IMikroIdentityStrategy Recno = new RecnoStrategy();
    private static readonly IMikroIdentityStrategy Guid = new GuidStrategy();

    /// <summary>Build the selector. The logger is used when an Unknown version is coerced to V15.</summary>
    public MikroIdentityStrategySelector(ILogger<MikroIdentityStrategySelector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolve the strategy for a specific database. The first call for a given
    /// database name evaluates the branches; subsequent calls are O(1).
    /// </summary>
    public IMikroIdentityStrategy GetFor(string databaseName, ErpVersionInfo info)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentNullException.ThrowIfNull(info);

        // Remember the raw probe so callers can short-circuit the detector later.
        _versions[databaseName] = new CachedVersion(info, DateTime.UtcNow);
        return _strategies.GetOrAdd(databaseName, _ => Pick(info));
    }

    /// <summary>
    /// Return the cached <see cref="ErpVersionInfo"/> for <paramref name="databaseName"/>, or
    /// <c>null</c> when no probe has been recorded. The TTL check is the caller's job — this
    /// method is intentionally cheap so the orchestrator can decide on every operation.
    /// </summary>
    public ErpVersionInfo? GetCached(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return null;
        }

        return _versions.TryGetValue(databaseName, out var cached) ? cached.Info : null;
    }

    /// <summary>
    /// Look up both the cached <see cref="ErpVersionInfo"/> AND the probe timestamp — needed
    /// by the orchestrator to age-test against its TTL without exposing internal record types.
    /// </summary>
    /// <returns>A tuple of <c>(Info, ProbedAtUtc)</c>; both <c>null</c> when nothing is cached.</returns>
    public (ErpVersionInfo? Info, DateTime? ProbedAtUtc)? GetCachedWithTimestamp(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return null;
        }

        return _versions.TryGetValue(databaseName, out var cached)
            ? (cached.Info, (DateTime?)cached.ProbedAtUtc)
            : null;
    }

    /// <summary>Explicit cache invalidation — exposed for tests and reconnect scenarios.</summary>
    public void Invalidate(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return;
        }

        _strategies.TryRemove(databaseName, out _);
        _versions.TryRemove(databaseName, out _);
    }

    /// <summary>Drop every cached entry — used by the orchestrator's <c>InvalidateCache()</c>.</summary>
    public void InvalidateAll()
    {
        _strategies.Clear();
        _versions.Clear();
    }

    /// <summary>Strategy decision — pure so the dispatcher behaviour is unit-testable.</summary>
    private IMikroIdentityStrategy Pick(ErpVersionInfo info)
    {
        switch (info.Version)
        {
            case MikroVersion.V16:
                _logger.LogInformation(
                    "Selected {Strategy} for database {Database}.", Guid.DisplayName, info.DatabaseName);
                return Guid;
            case MikroVersion.V15:
                _logger.LogInformation(
                    "Selected {Strategy} for database {Database}.", Recno.DisplayName, info.DatabaseName);
                return Recno;
            default:
                _logger.LogWarning(
                    "Mikro version for {Database} is Unknown — defaulting to {Strategy}. Confirm the server is supported.",
                    info.DatabaseName, Recno.DisplayName);
                return Recno;
        }
    }
}