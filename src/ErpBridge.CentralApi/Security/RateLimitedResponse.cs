using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.LogCenter;

namespace ErpBridge.CentralApi.Security;

/// <summary>
/// The one 429 the API answers with (GOAL_MUSTERI_KATALOGU §5): <see cref="ApiError"/> <c>RATE_LIMITED</c> and, when
/// the wait is known, <c>Retry-After</c> in whole seconds. The rate limiter and <see cref="LoginThrottle"/> both use
/// it, so a throttled sign-in looks exactly like any other rejected request.
/// </summary>
public static class RateLimitedResponse
{
    public const string ErrorCode = "RATE_LIMITED";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext http, TimeSpan? retryAfter, CancellationToken ct = default)
    {
        http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (retryAfter is { } wait)
            http.Response.Headers.RetryAfter = Seconds(wait).ToString(CultureInfo.InvariantCulture);
        http.Response.ContentType = "application/json; charset=utf-8";
        return http.Response.WriteAsync(JsonSerializer.Serialize(new ApiError
        {
            ErrorCode = ErrorCode,
            Message = "Too many requests. Try again later.",
            TraceId = CorrelationId.Of(http),
        }, Json), ct);
    }

    /// <summary>The same response as an endpoint result.</summary>
    public static IResult Result(TimeSpan? retryAfter) => new Rejected(retryAfter);

    /// <summary>Rounded up and at least one second: "0" would invite an immediate retry.</summary>
    public static long Seconds(TimeSpan wait) => Math.Max(1, (long)Math.Ceiling(wait.TotalSeconds));

    private sealed class Rejected(TimeSpan? retryAfter) : IResult
    {
        public Task ExecuteAsync(HttpContext httpContext) => WriteAsync(httpContext, retryAfter, httpContext.RequestAborted);
    }
}
