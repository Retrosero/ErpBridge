using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;

namespace ErpBridge.CentralApi.Storage;

/// <summary>Answers that send a stored file's bytes from the server (the phone's own picture endpoints, S4/S5).</summary>
public static class StoredFileResults
{
    /// <summary>A file id never gets other bytes: the phone may keep the answer as long as it likes.</summary>
    public const string ImmutableCache = "private, max-age=31536000, immutable";

    /// <summary>
    /// Streams the object from R2 with <see cref="ImmutableCache"/>; the caller has checked the user may see it. A missing
    /// object answers the caller's 404, R2 out of reach <c>503 STORAGE_UNAVAILABLE</c>.
    /// </summary>
    public static async Task<IResult> StreamAsync(HttpContext http, IObjectStore store, StoredFile file, string notFoundCode, string notFoundMessage, CancellationToken ct)
    {
        StoredObject? stored;
        try
        {
            stored = await store.GetAsync(file.Bucket, file.ObjectKey, ct);
        }
        catch (StorageUnavailableException)
        {
            return StorageErrors.Unavailable().ToResult(http);
        }
        if (stored is null) return JsonResults.Status(StatusCodes.Status404NotFound, new ApiError { ErrorCode = notFoundCode, Message = notFoundMessage });
        http.Response.Headers.CacheControl = ImmutableCache;
        return Results.Stream(stored.Content, file.ContentType);
    }
}
