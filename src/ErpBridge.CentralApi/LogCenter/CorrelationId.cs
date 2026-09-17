using System.Text.RegularExpressions;

namespace ErpBridge.CentralApi.LogCenter;

/// <summary>
/// <c>X-Correlation-Id</c> (plan D4): one id for everything a single user action causes — the phone's request,
/// this API's log lines, the job the agent later writes into the ERP. A caller's id is kept when it is safe;
/// otherwise the API makes one. Either way it is echoed on the response, set as the request's
/// <see cref="HttpContext.TraceIdentifier"/> (so <c>ApiError.traceId</c> carries it) and put in the logging scope.
/// </summary>
public static partial class CorrelationId
{
    public const string HeaderName = "X-Correlation-Id";
    public const int MaxLength = 128;
    private const string ItemKey = "LogCenter.CorrelationId";

    /// <summary>The caller's id if it is 1–128 characters of <c>[A-Za-z0-9._:-]</c>; otherwise null.</summary>
    public static string? Sanitize(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxLength) return null;
        return SafePattern().IsMatch(trimmed) ? trimmed : null;
    }

    /// <summary>The id assigned to this request (falls back to the trace identifier outside the middleware).</summary>
    public static string Of(HttpContext context) =>
        context.Items.TryGetValue(ItemKey, out var value) && value is string id ? id : context.TraceIdentifier;

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        var id = Sanitize(context.Request.Headers[HeaderName].FirstOrDefault()) ?? Guid.NewGuid().ToString();
        context.Items[ItemKey] = id;
        context.TraceIdentifier = id;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = id;
            return Task.CompletedTask;
        });

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ErpBridge.CentralApi.Requests");
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id }))
        {
            await next(context);
        }
    });

    [GeneratedRegex("^[A-Za-z0-9._:-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex SafePattern();
}
