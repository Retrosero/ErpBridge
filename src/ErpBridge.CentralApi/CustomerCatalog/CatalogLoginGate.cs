using System.Threading.RateLimiting;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// What the <c>catalog-login</c> rate-limit policy (per client IP) cannot do on its own (GOAL_MUSTERI_KATALOGU §6):
/// a budget per company, so many addresses together cannot try one company's names faster than
/// <see cref="CompanyPermitsPerMinute"/>, and a cap on BCrypt work running at once, so a spread-out attack cannot take
/// every core. Unknown companies are counted too (by the code they asked for). In memory: one CentralApi container.
/// </summary>
public sealed class CatalogLoginGate : IDisposable
{
    public const int CompanyPermitsPerMinute = 300;
    public const int MaxConcurrentHashes = 8;

    private static readonly TimeSpan DefaultWait = TimeSpan.FromMinutes(1);

    private readonly PartitionedRateLimiter<string> _companies;
    private readonly SemaphoreSlim _hashes = new(MaxConcurrentHashes, MaxConcurrentHashes);

    public CatalogLoginGate() : this(CompanyPermitsPerMinute) { }

    /// <summary>Test seam: a smaller company budget.</summary>
    internal CatalogLoginGate(int companyPermitsPerMinute) =>
        _companies = PartitionedRateLimiter.Create<string, string>(code => RateLimitPartition.GetFixedWindowLimiter(code, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = companyPermitsPerMinute,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true,
        }));

    /// <summary>One sign-in attempt for the company: null when it may go on, else how long to wait.</summary>
    public TimeSpan? Enter(string companyCode)
    {
        using var lease = _companies.AttemptAcquire(companyCode.ToUpperInvariant());
        if (lease.IsAcquired) return null;
        return lease.TryGetMetadata(MetadataName.RetryAfter, out var wait) ? wait : DefaultWait;
    }

    /// <summary><c>BCrypt.Verify</c>, at most <see cref="MaxConcurrentHashes"/> at a time.</summary>
    public async Task<bool> VerifyAsync(string password, string hash, CancellationToken ct)
    {
        await _hashes.WaitAsync(ct);
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        finally
        {
            _hashes.Release();
        }
    }

    public void Dispose()
    {
        _companies.Dispose();
        _hashes.Dispose();
    }
}
