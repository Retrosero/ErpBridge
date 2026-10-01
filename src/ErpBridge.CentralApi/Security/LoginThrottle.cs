using System.Collections.Concurrent;

namespace ErpBridge.CentralApi.Security;

/// <summary>
/// Slows down password guessing per sign-in name (GOAL_MUSTERI_KATALOGU T2). Five failed sign-ins of one name within
/// 15 minutes make it wait 60 seconds; every further failure doubles the wait, up to 15 minutes. While a name waits
/// the endpoint answers 429 <b>without checking the password</b>, so a right and a wrong password look the same and
/// the wait cannot be used to test guesses. A success clears the name; 15 quiet minutes after the last failure
/// and the last wait forgive it (so a name at the 15-minute ceiling gets one guess per 15 minutes, not a fresh five).
///
/// <para>An attempt is taken with <see cref="TryBegin"/> before the password is checked and finished with
/// <see cref="Attempt.Succeeded"/> or <see cref="Attempt.Failed"/>. Attempts still running count against the name
/// (under the name's lock), so twenty parallel guesses cannot all pass the check before the first failure is written:
/// at most as many run at once as failures are left before the wait (one, once the name has waited).</para>
///
/// <para>Nothing is locked: the account is never disabled, so a rival cannot keep a customer out for long. Names are
/// counted whether or not the account exists (the reply to an unknown name is the same). The key may also carry the
/// caller's address partition (<see cref="ClientIpPartition"/>), so failures from one address do not slow the same
/// name down at another. Kept in memory, like <c>TenantEventHub</c> and <c>PortalRecordMirror</c>: one CentralApi
/// container; at most <see cref="DefaultMaxEntries"/> keys are remembered.</para>
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

    /// <summary>How long a caller waits when the name's remaining attempts are all running right now.</summary>
    public static readonly TimeSpan InFlightWait = TimeSpan.FromSeconds(1);

    /// <summary>Keys remembered at most; past it the quiet ones go first, then the least recently tried.</summary>
    public const int DefaultMaxEntries = 100_000;

    /// <summary>A name longer than any valid one still counts, without holding an arbitrarily long key.</summary>
    private const int MaxKeyPartLength = 128;

    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<Key, Entry> _entries = new();
    private readonly TimeProvider _clock;
    private readonly int _maxEntries;
    private readonly object _trimGate = new();
    private long _nextSweepMs;
    private int _size;

    public LoginThrottle() : this(TimeProvider.System) { }

    /// <summary>Test seam — inject a deterministic <see cref="TimeProvider"/> (and a smaller cap).</summary>
    public LoginThrottle(TimeProvider clock, int maxEntries = DefaultMaxEntries)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _maxEntries = maxEntries > 0 ? maxEntries : throw new ArgumentOutOfRangeException(nameof(maxEntries));
    }

    /// <summary>
    /// Starts one sign-in attempt of the name: refused (<see cref="Attempt.RetryAfter"/> set) while the name waits or
    /// while its remaining attempts are all running. An allowed attempt is finished with <see cref="Attempt.Succeeded"/>
    /// or <see cref="Attempt.Failed"/>; disposing it unfinished counts nothing.
    /// </summary>
    /// <param name="client">The caller's address partition when the name is counted per address; null counts it everywhere.</param>
    public Attempt TryBegin(string area, string? tenant, string? name, string? client = null)
    {
        var key = KeyOf(area, tenant, name, client);
        var now = NowMs();
        SweepIfDue(now);
        while (true)
        {
            var entry = Acquire(key, now);
            lock (entry)
            {
                // Swept between the lookup and the lock: take the key's new entry.
                if (entry.Removed) continue;
                if (entry.QuietSince(now) >= (long)Window.TotalMilliseconds) entry.Reset();
                entry.LastSeenMs = now;
                if (entry.BlockedUntilMs > now) return Attempt.Refused(TimeSpan.FromMilliseconds(entry.BlockedUntilMs - now));
                var room = entry.Strikes > 0 ? 1 : MaxFailures - entry.FailuresWithin(now);
                if (entry.Pending >= room) return Attempt.Refused(InFlightWait);
                entry.Pending++;
                return new Attempt(this, key, entry);
            }
        }
    }

    /// <summary>How long the name must wait before its next attempt; null when it may try now (a look, nothing counted).</summary>
    public TimeSpan? RetryAfter(string area, string? tenant, string? name, string? client = null)
    {
        if (!_entries.TryGetValue(KeyOf(area, tenant, name, client), out var entry)) return null;
        var now = NowMs();
        lock (entry)
            return entry.BlockedUntilMs > now ? TimeSpan.FromMilliseconds(entry.BlockedUntilMs - now) : null;
    }

    /// <summary>Keys currently remembered (tests).</summary>
    internal int Count => _entries.Count;

    private long NowMs() => _clock.GetUtcNow().ToUnixTimeMilliseconds();

    private Entry Acquire(Key key, long now)
    {
        while (true)
        {
            if (_entries.TryGetValue(key, out var found)) return found;
            if (Volatile.Read(ref _size) >= _maxEntries) Trim(now);
            var created = new Entry();
            if (_entries.TryAdd(key, created))
            {
                Interlocked.Increment(ref _size);
                return created;
            }
        }
    }

    /// <summary>Called under the entry's lock.</summary>
    private void Remove(Key key, Entry entry)
    {
        entry.Removed = true;
        if (_entries.TryRemove(new KeyValuePair<Key, Entry>(key, entry))) Interlocked.Decrement(ref _size);
    }

    private void Finish(Key key, Entry entry, bool? success)
    {
        var now = NowMs();
        lock (entry)
        {
            entry.Pending--;
            entry.LastSeenMs = now;
            if (success == true)
            {
                entry.Reset();
                if (entry.Pending == 0) Remove(key, entry);
            }
            else if (success == false)
            {
                RecordFailure(entry, now);
            }
        }
    }

    /// <summary>A wrong password, an unknown name or an unknown company. Called under the entry's lock.</summary>
    private static void RecordFailure(Entry entry, long now)
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

    /// <summary>Forgets keys that have been quiet for the whole window, at most once a minute.</summary>
    private void SweepIfDue(long now)
    {
        var due = Interlocked.Read(ref _nextSweepMs);
        if (now < due || Interlocked.CompareExchange(ref _nextSweepMs, now + (long)SweepInterval.TotalMilliseconds, due) != due) return;
        Sweep(now);
    }

    private void Sweep(long now)
    {
        foreach (var (key, entry) in _entries)
        {
            lock (entry)
            {
                if (!entry.Removed && entry.Pending == 0 && entry.QuietSince(now) >= (long)Window.TotalMilliseconds) Remove(key, entry);
            }
        }
    }

    /// <summary>
    /// Too many keys (a flood of made-up names): the quiet ones go, then — down to nine tenths of the cap — the least
    /// recently tried keys that are not waiting, waiting ones last. Keys with an attempt running stay. One trim at a time;
    /// callers arriving meanwhile go on.
    /// </summary>
    private void Trim(long now)
    {
        if (!Monitor.TryEnter(_trimGate)) return;
        try
        {
            Sweep(now);
            var excess = Volatile.Read(ref _size) - _maxEntries / 10 * 9;
            if (excess <= 0) return;
            var candidates = new List<(Key Key, Entry Entry, bool Waiting, long Seen)>();
            foreach (var (key, entry) in _entries)
            {
                lock (entry)
                {
                    if (!entry.Removed && entry.Pending == 0) candidates.Add((key, entry, entry.BlockedUntilMs > now, entry.LastSeenMs));
                }
            }
            foreach (var (key, entry, _, _) in candidates.OrderBy(c => c.Waiting).ThenBy(c => c.Seen).Take(excess))
            {
                lock (entry)
                {
                    if (!entry.Removed && entry.Pending == 0) Remove(key, entry);
                }
            }
        }
        finally
        {
            Monitor.Exit(_trimGate);
        }
    }

    private static Key KeyOf(string area, string? tenant, string? name, string? client) =>
        new(area, Part(tenant).ToUpperInvariant(), Part(name).ToLowerInvariant(), Part(client).ToLowerInvariant());

    private static string Part(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return trimmed.Length <= MaxKeyPartLength ? trimmed : trimmed[..MaxKeyPartLength];
    }

    internal readonly record struct Key(string Area, string Tenant, string Name, string Client);

    /// <summary>One sign-in attempt taken from <see cref="TryBegin"/>.</summary>
    public sealed class Attempt : IDisposable
    {
        private readonly LoginThrottle? _owner;
        private readonly Key _key;
        private readonly Entry? _entry;
        private int _ended;

        internal Attempt(LoginThrottle owner, Key key, Entry entry)
        {
            _owner = owner;
            _key = key;
            _entry = entry;
        }

        private Attempt(TimeSpan wait) => RetryAfter = wait;

        internal static Attempt Refused(TimeSpan wait) => new(wait);

        /// <summary>Set when the attempt may not go on: answer 429 with this wait, without checking the password.</summary>
        public TimeSpan? RetryAfter { get; }

        /// <summary>The right password: the name starts clean.</summary>
        public void Succeeded() => End(true);

        /// <summary>A wrong password, an unknown name or an unknown company.</summary>
        public void Failed() => End(false);

        /// <summary>An attempt left unfinished (an error before the password was judged) counts nothing.</summary>
        public void Dispose() => End(null);

        private void End(bool? success)
        {
            if (_owner is null || Interlocked.Exchange(ref _ended, 1) != 0) return;
            _owner.Finish(_key, _entry!, success);
        }
    }

    internal sealed class Entry
    {
        /// <summary>Times of the last <see cref="MaxFailures"/> failures (a ring; <see cref="Next"/> is the oldest when full).</summary>
        public readonly long[] Recent = new long[MaxFailures];
        public int Next;
        public int Count;

        /// <summary>How many times the name was made to wait since its last quiet window.</summary>
        public int Strikes;

        public long LastFailureMs;
        public long BlockedUntilMs;

        /// <summary>Attempts begun and not finished yet.</summary>
        public int Pending;

        /// <summary>When the key was last tried; the trim drops the oldest first.</summary>
        public long LastSeenMs;

        /// <summary>Taken out of the table; a caller holding it looks the key up again.</summary>
        public bool Removed;

        /// <summary>Milliseconds since the name last failed or last finished waiting.</summary>
        public long QuietSince(long now) => now - Math.Max(LastFailureMs, BlockedUntilMs);

        /// <summary>Failures among the last <see cref="MaxFailures"/> that are still inside the window.</summary>
        public int FailuresWithin(long now)
        {
            var within = 0;
            for (var i = 0; i < Count; i++)
                if (now - Recent[i] < (long)Window.TotalMilliseconds) within++;
            return within;
        }

        /// <summary>Forgets the failures; attempts still running stay counted.</summary>
        public void Reset()
        {
            Array.Clear(Recent);
            Next = 0;
            Count = 0;
            Strikes = 0;
            LastFailureMs = 0;
            BlockedUntilMs = 0;
        }
    }
}
