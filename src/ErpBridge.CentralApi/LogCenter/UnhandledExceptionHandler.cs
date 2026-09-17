using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using Microsoft.AspNetCore.Diagnostics;

namespace ErpBridge.CentralApi.LogCenter;

/// <summary>
/// Last stop for an exception no endpoint handled (plan L0e). The caller gets a plain 500 <see cref="ApiError"/>
/// with the correlation id and nothing internal; the log centre gets the full exception with the route
/// template, company, user or agent. Recording can never turn the 500 into something worse.
/// </summary>
public static class UnhandledExceptionHandler
{
    /// <summary>
    /// Logger category of the console line written here. The database logger (plan L2b) must skip it —
    /// this handler already stores the event with more context.
    /// </summary>
    public const string LoggerCategory = "ErpBridge.CentralApi.LogCenter.UnhandledException";

    public const string Kind = "UNHANDLED_EXCEPTION";

    private static readonly TimeSpan RecordTimeout = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task HandleAsync(HttpContext context)
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;
        var correlationId = CorrelationId.Of(context);
        var route = (feature?.Endpoint as RouteEndpoint)?.RoutePattern.RawText ?? feature?.Path ?? context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        // The console line gets the scrubbed text, never the exception object: console providers print the raw
        // message and stack, and an exception can carry a connection string or token. The log centre row below is
        // scrubbed by the writer.
        context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(LoggerCategory)
            .LogError("Unhandled {ExceptionType} on {Method} {Route} (correlation {CorrelationId}): {Detail}",
                exception?.GetType().FullName, method, route, correlationId, LogScrubber.Scrub(exception?.ToString()));

        await RecordAsync(context, exception, method, route, correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers[CorrelationId.HeaderName] = correlationId;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new ApiError
        {
            ErrorCode = "INTERNAL_ERROR",
            Message = "An unexpected error occurred.",
            TraceId = correlationId,
        }, Json));
    }

    private static async Task RecordAsync(HttpContext context, Exception? exception, string method, string route, string correlationId)
    {
        try
        {
            var user = context.User;
            Guid? tenantId = user.TryGetTenantId(out var tenant) ? tenant : null;
            var scope = user.FindFirst(CentralApiClaims.Scope)?.Value;
            Guid? userId = scope == CentralApiClaims.MobileUserScope && Guid.TryParse(user.FindFirst("sub")?.Value, out var parsedUser) ? parsedUser : null;
            Guid? agentId = scope == "agent" && user.TryGetAgentId(out var agent) ? agent : null;

            // A fresh scope: the request's DbContext may hold the very entities whose save just failed.
            using var services = context.RequestServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var timeout = new CancellationTokenSource(RecordTimeout);
            await services.ServiceProvider.GetRequiredService<ILogEventWriter>().WriteAsync(
            [
                new LogEventInput
                {
                    Source = LogSources.CentralApi,
                    TenantId = tenantId,
                    Severity = LogSeverity.Error,
                    Kind = Kind,
                    Operation = $"{method} {route}",
                    ExceptionType = exception?.GetType().FullName,
                    Message = exception?.Message,
                    StackTrace = exception?.ToString(),
                    AppVersion = typeof(UnhandledExceptionHandler).Assembly.GetName().Version?.ToString(),
                    UserId = userId,
                    AgentId = agentId,
                    CorrelationId = correlationId,
                    HttpMethod = method,
                    HttpRoute = route,
                    HttpStatus = StatusCodes.Status500InternalServerError,
                    PropertiesJson = scope is null ? null : JsonSerializer.Serialize(new { scope }),
                },
            ], timeout.Token);
        }
        catch (Exception recordFailure)
        {
            context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(LoggerCategory)
                .LogWarning("Could not store the unhandled exception in the log centre: {Failure}", LogScrubber.Scrub(recordFailure.ToString()));
        }
    }
}
