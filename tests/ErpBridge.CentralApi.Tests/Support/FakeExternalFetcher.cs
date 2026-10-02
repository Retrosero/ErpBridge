using System.Collections.Concurrent;
using ErpBridge.CentralApi.Storage;

namespace ErpBridge.CentralApi.Tests.Support;

/// <summary>
/// Outside downloads for tests (GOAL_DEPOLAMA_R2 S7): answers by address from a dictionary, counts every call per address,
/// answers 304 when the request's ETag is the current one and keeps to the request's byte limit. An unknown address is 404.
/// </summary>
public sealed class FakeExternalFetcher : IExternalFetcher
{
    private sealed record Route(FetchStatus Status, byte[] Body, string? ETag, string? LastModified);

    private readonly ConcurrentDictionary<string, Route> _routes = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, int> _calls = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<(string Url, FetchRequest Request)> _requests = new();

    /// <summary>A 200 with <paramref name="body"/>; 304 to a request that sends <paramref name="etag"/> back.</summary>
    public void Serve(string url, byte[] body, string? etag = null, string? lastModified = null) =>
        _routes[url] = new Route(FetchStatus.Ok, body, etag, lastModified);

    /// <summary>The address answers with <paramref name="status"/> (not found, failed, too large).</summary>
    public void Fail(string url, FetchStatus status) => _routes[url] = new Route(status, [], null, null);

    public void Forget(string url) => _routes.TryRemove(url, out _);

    public int Calls(string url) => _calls.GetValueOrDefault(url);

    /// <summary>The requests made to <paramref name="url"/>, oldest first.</summary>
    public IReadOnlyList<FetchRequest> Requests(string url) => [.. _requests.Where(r => r.Url == url).Select(r => r.Request)];

    public async Task<FetchResult> GetAsync(Uri url, FetchRequest request, Stream destination, CancellationToken ct)
    {
        var key = url.OriginalString;
        _calls.AddOrUpdate(key, 1, (_, n) => n + 1);
        _requests.Enqueue((key, request));
        if (!_routes.TryGetValue(key, out var route)) return new FetchResult(FetchStatus.NotFound, HttpStatus: 404, Reason: "not found");
        if (route.Status != FetchStatus.Ok) return new FetchResult(route.Status, Reason: "fake " + route.Status);
        if (route.ETag is not null && request.ETag == route.ETag)
            return new FetchResult(FetchStatus.NotModified, 0, route.ETag, route.LastModified, HttpStatus: 304);
        if (route.Body.Length > request.MaxBytes) return new FetchResult(FetchStatus.TooLarge, Reason: "too large");
        await destination.WriteAsync(route.Body, ct);
        return new FetchResult(FetchStatus.Ok, route.Body.Length, route.ETag, route.LastModified, HttpStatus: 200);
    }
}
