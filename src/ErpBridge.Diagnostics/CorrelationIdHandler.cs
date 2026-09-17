using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Diagnostics;

/// <summary>
/// Log Merkezi L2e/L2f: every call a web host makes to the CentralApi carries an <c>X-Correlation-Id</c>, so the
/// API's own log line for the same request can be found from the host's. A 5xx answer or a failed call is logged
/// here as a warning with that id, the method, the path (never the query: it carries customer codes) and the
/// status — the host's log shipping then sends it to the log centre.
/// </summary>
public sealed class CorrelationIdHandler(ILogger<CorrelationIdHandler> logger) : DelegatingHandler
{
    public const string HeaderName = "X-Correlation-Id";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.TryGetValues(HeaderName, out var existing) || !existing.Any())
            request.Headers.TryAddWithoutValidation(HeaderName, Guid.NewGuid().ToString());
        var correlationId = request.Headers.GetValues(HeaderName).First();
        var method = request.Method.Method;
        var path = request.RequestUri?.IsAbsoluteUri == true ? request.RequestUri.AbsolutePath : request.RequestUri?.OriginalString.Split('?')[0];

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogWarning("CentralApi call failed: {HttpMethod} {HttpRoute} ({Failure}) [{Kind}] {CorrelationId}",
                method, path, ex.GetType().Name, "UPSTREAM_UNREACHABLE", correlationId);
            throw;
        }

        if ((int)response.StatusCode >= 500)
        {
            logger.LogWarning("CentralApi answered {HttpStatus}: {HttpMethod} {HttpRoute} [{Kind}] {CorrelationId}",
                (int)response.StatusCode, method, path, "UPSTREAM_5XX", correlationId);
        }
        return response;
    }
}

public static class RequestCorrelationExtensions
{
    /// <summary>
    /// Puts <see cref="HttpContext.TraceIdentifier"/> into the logging scope as <c>CorrelationId</c> for the whole request
    /// (register before <c>UseExceptionHandler</c>). The error page shows the same identifier as the support code, so
    /// the exception the handler logs can be found in the log centre by it. Components that set their own
    /// <c>CorrelationId</c> scope (the error boundaries) override it.
    /// </summary>
    public static IApplicationBuilder UseRequestCorrelationScope(this IApplicationBuilder app)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("ErpBridge.Diagnostics.Requests");
        return app.Use(async (context, next) =>
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = context.TraceIdentifier }))
            {
                await next(context);
            }
        });
    }
}
