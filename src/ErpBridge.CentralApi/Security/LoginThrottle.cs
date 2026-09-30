using System.Collections.Concurrent;

namespace ErpBridge.CentralApi.Security;

/// <summary>
/// Slows down password guessing per sign-in name (GOAL_MUSTERI_KATALOGU T2). Five failed sign-ins of one name within
/// 15 minutes make it wait 60 seconds; every further failure doubles the wait, up to 15 minutes. While a name waits
/// the endpoint answers 429 <b>without checking the password</b>, so a right and a wrong password look the same and
/// the wait cannot be used to test guesses. A success clears the name; 15 quiet minutes after the last failure
/// and the last wait forgive it (so a name at the 15-minute ceiling gets one guess per 15 minutes, not a fresh five).
///
/// <para>Nothing is locked: the account is never disabled, so a rival cannot keep a customer out for long. Names are
/// counted whether or not the account exists (the reply to an unknown name is the same). Kept in memory, like
/// <c>TenantEventHub</c> and <c>PortalRecordMirror</c>: one CentralApi container.</para>
/// </summary>
public sealed class LoginThrottle
{
    /// <summary>Phone and portal users (<c>/api/v1/android/account/login</c>).</summary>
    public const string StaffArea = "staff";

    /// <summary>Customer catalog accounts.</summary>
    public const string CatalogArea = "catalog";

    /// <summary>Operators of the Admin console.</summary>
    public const string AdminArea = "admin";

    public const int MaxFailures = 5;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan FirstWait = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan MaxWait = TimeSpan.FromMinutes(15);

    /// <summary>A name longer than any valid one still counts, without holding an arbitrarily long key.</summary>
    private const int MaxKeyPartLength = 128;

    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<Key, Entry> _entries = new();
    private readonly TimeProvider _clock;
    private long _nextSweepMs;

    public LoginThrottle() : this(TimeProvider.System) { }

    /// <summary>Test seam — inject a deterministic <see cref="TimeProvider"/>.</summary>
    public LoginThrottle(TimeProvider clock) => _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>How long the name must wait before its next attempt; null when it may try now.</summary>
    public TimeSpan? RetryAfter(string area, string? tenant, string? username)
    {
        if (!_entries.TryGetValue(KeyOf(area, tenant, username), out var entry)) return null;
        var now = NowMs();
        lock (entry)
            return entry.BlockedUntilMs > now ? TimeSpan.FromMilliseconds(entry.BlockedUntilMs - now) : null;
    }

    /// <summary>A wrong password, an unknown name or an unknown company.</summary>
    public void RecordFailure(string area, string? tenant, string? username)
    {
        var now = NowMs();
        SweepIfDue(now);
        var entry = _entries.GetOrAdd(KeyOf(area, tenant, username), _ => new Entry());
        lock (entry)
        {
            if (entry.QuietSince(now) >= (long)Window.TotalMilliseconds) entry.Reset();
            entry.LastFailureMs = now;
            entry.Recent[entry.Next] = now;
            entry.Next = (entry.Next + 1) % MaxFailures;
            entry.Count = Math.Min(entry.Count + 1, MaxFailures);

            // Once the name had to wait, every further failure before a quiet window waits again, longer each time.
            var oldestOfLastFive = entry.Recent[entry.Next];
            var tooMany = entry.Count == MaxFailures && now - oldestOfLastFive < (long)Window.TotalMilliseconds;
            if (entry.Strikes == 0 && !tooMany) return;
            entry.Strikes++;
            var waitMs = Math.Min(FirstWait.TotalMilliseconds * Math.Pow(2, entry.Strikes - 1), MaxWait.TotalMilliseconds);
            entry.BlockedUntilMs = now + (long)waitMs;
        }
    }

    /// <summary>The right password: the name starts clean.</summary>
    public void RecordSuccess(string area, string? tenant, string? username) =>
        _entries.TryRemove(KeyOf(area, tenant, username), out _);

    /// <summary>Names currently remembered (tests).</summary>
    internal int Count => _entries.Count;

    private long NowMs() => _clock.GetUtcNow().ToUnixTimeMilliseconds();

    /// <summary>Forgets names that have been quiet for the whole window, at most once a minute.</summary>
    private void SweepIfDue(long now)
    {
        var due = Interlocked.Read(ref _nextSweepMs);
        if (now < due || Interlocked.CompareExchange(ref _nextSweepMs, now + (long)SweepInterval.TotalMilliseconds, due) != due) return;
        foreach (var (key, entry) in _entries)
        {
            bool stale;
            lock (entry) stale = entry.QuietSince(now) >= (long)Window.TotalMilliseconds;
            if (stale) _entries.TryRemove(key, out _);
        }
    }

    private static Key KeyOf(string area, string? tenant, string? username) =>
        new(area, Part(tenant).ToUpperInvariant(), Part(username).ToLowerInvariant());

    private static string Part(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return trimmed.Length <= MaxKeyPartLength ? trimmed : trimmed[..MaxKeyPartLength];
    }

    private readonly record struct Key(string Area, string Tenant, string Username);

    private sealed class Entry
    {
        /// <summary>Times of the last <see cref="MaxFailures"/> failures (a ring; <see cref="Next"/> is the oldest when full).</summary>
        public readonly long[] Recent = new long[MaxFailures];
        public int Next;
        public int Count;

        /// <summary>How many times the name was made to wait since its last quiet window.</summary>
        public int Strikes;

        public long LastFailureMs;
        public long BlockedUntilMs;

        /// <summary>Milliseconds since the name last failed or last finished waiting.</summary>
        public long QuietSince(long now) => now - Math.Max(LastFailureMs, BlockedUntilMs);

        public void Reset()
        {
            Array.Clear(Recent);
            Next = 0;
            Count = 0;
            Strikes = 0;
            BlockedUntilMs = 0;
        }
    }
}
