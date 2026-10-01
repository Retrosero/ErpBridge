using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// The object storage behind the central file store (GOAL_DEPOLAMA_R2 T1, T6). Buckets are named by their role —
/// <see cref="StorageBuckets.Public"/> or <see cref="StorageBuckets.Private"/> — and the implementation maps them to the
/// configured bucket names, so the ledger (<c>stored_files.Bucket</c>) never carries an account's bucket name.
/// Production uses <see cref="R2ObjectStore"/>; without its settings <see cref="UnavailableObjectStore"/> stands in and
/// every call throws <see cref="StorageUnavailableException"/>.
/// </summary>
public interface IObjectStore
{
    /// <summary>False when the store is not configured; nothing is attempted then.</summary>
    bool IsAvailable { get; }

    /// <summary>Writes (or replaces) the object.</summary>
    Task PutAsync(string bucket, string key, byte[] data, string contentType, CancellationToken ct);

    /// <summary>The object's bytes as a stream the caller disposes, or null when there is no such object.</summary>
    Task<StoredObject?> GetAsync(string bucket, string key, CancellationToken ct);

    /// <summary>Deletes the object; a missing object is not an error.</summary>
    Task DeleteAsync(string bucket, string key, CancellationToken ct);

    /// <summary>The object's size and type, or null when there is no such object.</summary>
    Task<StoredObjectInfo?> HeadAsync(string bucket, string key, CancellationToken ct);

    /// <summary>Every object whose key starts with <paramref name="prefix"/> (S9 reconciliation).</summary>
    IAsyncEnumerable<StoredObjectInfo> ListAsync(string bucket, string prefix, CancellationToken ct);

    /// <summary>A GET address that works without credentials for <paramref name="validFor"/>.</summary>
    Task<Uri> PresignGetAsync(string bucket, string key, TimeSpan validFor, CancellationToken ct);
}

/// <summary>An object's metadata.</summary>
public sealed record StoredObjectInfo(string Key, long SizeBytes, string? ContentType, DateTimeOffset? LastModified);

/// <summary>An open object: the caller disposes it.</summary>
public sealed class StoredObject : IAsyncDisposable, IDisposable
{
    public StoredObject(Stream content, long sizeBytes, string? contentType)
    {
        Content = content;
        SizeBytes = sizeBytes;
        ContentType = contentType;
    }

    public Stream Content { get; }

    public long SizeBytes { get; }

    public string? ContentType { get; }

    public void Dispose() => Content.Dispose();

    public ValueTask DisposeAsync() => Content.DisposeAsync();
}

/// <summary>The store is not configured or could not be reached; endpoints answer <c>503 STORAGE_UNAVAILABLE</c>.</summary>
public sealed class StorageUnavailableException : Exception
{
    public StorageUnavailableException(string message) : base(message) { }

    public StorageUnavailableException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Stands in while the <c>Storage</c> settings are incomplete: the API runs, storage answers 503.</summary>
public sealed class UnavailableObjectStore : IObjectStore
{
    private const string Message = "Central file storage is not configured (Storage:* settings).";

    public bool IsAvailable => false;

    public Task PutAsync(string bucket, string key, byte[] data, string contentType, CancellationToken ct) => throw new StorageUnavailableException(Message);

    public Task<StoredObject?> GetAsync(string bucket, string key, CancellationToken ct) => throw new StorageUnavailableException(Message);

    public Task DeleteAsync(string bucket, string key, CancellationToken ct) => throw new StorageUnavailableException(Message);

    public Task<StoredObjectInfo?> HeadAsync(string bucket, string key, CancellationToken ct) => throw new StorageUnavailableException(Message);

    public IAsyncEnumerable<StoredObjectInfo> ListAsync(string bucket, string prefix, CancellationToken ct) => throw new StorageUnavailableException(Message);

    public Task<Uri> PresignGetAsync(string bucket, string key, TimeSpan validFor, CancellationToken ct) => throw new StorageUnavailableException(Message);
}
