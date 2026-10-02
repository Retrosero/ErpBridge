using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ErpBridge.CentralApi.Storage;

namespace ErpBridge.CentralApi.Tests.Support;

/// <summary>
/// R2 stand-in for tests (GOAL_DEPOLAMA_R2): objects in a dictionary per bucket, presigned addresses that say what they
/// sign, and a switch that makes the next writes fail the way an unreachable R2 does.
/// </summary>
public sealed class InMemoryObjectStore : IObjectStore
{
    private readonly ConcurrentDictionary<(string Bucket, string Key), (byte[] Data, string ContentType, DateTimeOffset Modified)> _objects = new();
    private int _failPuts;

    public bool IsAvailable => true;

    /// <summary>How many of the next uploads fail with <see cref="StorageUnavailableException"/>.</summary>
    public void FailNextPuts(int count) => Interlocked.Exchange(ref _failPuts, count);

    /// <summary>Every delete fails while set.</summary>
    public bool FailDeletes { get; set; }

    public IReadOnlyCollection<(string Bucket, string Key)> Keys => _objects.Keys.ToList();

    public byte[]? Bytes(string bucket, string key) => _objects.TryGetValue((bucket, key), out var item) ? item.Data : null;

    public Task PutAsync(string bucket, string key, byte[] data, string contentType, CancellationToken ct)
    {
        if (Interlocked.Decrement(ref _failPuts) >= 0) throw new StorageUnavailableException("simulated R2 outage");
        _objects[(bucket, key)] = (data.ToArray(), contentType, DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }

    public Task<StoredObject?> GetAsync(string bucket, string key, CancellationToken ct) =>
        Task.FromResult(_objects.TryGetValue((bucket, key), out var item) ? new StoredObject(new MemoryStream(item.Data), item.Data.Length, item.ContentType) : null);

    public Task DeleteAsync(string bucket, string key, CancellationToken ct)
    {
        if (FailDeletes) throw new StorageUnavailableException("simulated R2 outage");
        _objects.TryRemove((bucket, key), out _);
        return Task.CompletedTask;
    }

    public Task<StoredObjectInfo?> HeadAsync(string bucket, string key, CancellationToken ct) =>
        Task.FromResult(_objects.TryGetValue((bucket, key), out var item) ? new StoredObjectInfo(key, item.Data.Length, item.ContentType, item.Modified) : null);

    /// <summary>Puts an object straight in (no failure switch), dated <paramref name="modified"/>: an orphan for the reconciliation.</summary>
    public void Seed(string bucket, string key, byte[] data, DateTimeOffset modified) => _objects[(bucket, key)] = (data, "image/jpeg", modified);

    public async IAsyncEnumerable<StoredObjectInfo> ListAsync(string bucket, string prefix, [EnumeratorCancellation] CancellationToken ct)
    {
        await Task.CompletedTask;
        foreach (var ((b, key), item) in _objects)
            if (b == bucket && key.StartsWith(prefix, StringComparison.Ordinal)) yield return new StoredObjectInfo(key, item.Data.Length, item.ContentType, item.Modified);
    }

    public Task<Uri> PresignGetAsync(string bucket, string key, TimeSpan validFor, CancellationToken ct) =>
        Task.FromResult(new Uri($"https://r2.test/{bucket}/{key}?X-Amz-Expires={(int)validFor.TotalSeconds}&X-Amz-Signature=test"));
}

/// <summary>
/// A relational host whose file store is <see cref="InMemoryObjectStore"/>, CDN at <c>https://img.test</c>, and whose
/// outside downloads (the XML picture sync) come from <see cref="FakeExternalFetcher"/>.
/// </summary>
public class StorageCentralApiFactory : SqliteCentralApiFactory
{
    public InMemoryObjectStore Store { get; } = new();

    public FakeExternalFetcher Fetcher { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Storage:PublicBaseUrl", "https://img.test");
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IObjectStore>(Store);
            services.AddSingleton<IExternalFetcher>(Fetcher);
        });
    }
}
