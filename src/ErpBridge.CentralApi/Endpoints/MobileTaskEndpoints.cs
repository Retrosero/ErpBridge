using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/tasks</c> and <c>/api/v1/android/notifications</c> (docs/GOAL_GOREVLER.md §3).
/// Phone and portal users alike; every call re-checks the user, device and subscription. Rate limited
/// per user, not per company: a team long-polling its tasks must not use up the company's sync budget.
/// </summary>
public static class MobileTaskEndpoints
{
    private const int MinWaitSeconds = 1;
    private const int MaxWaitSeconds = 30;
    private const int DefaultWaitSeconds = 25;

    public static IEndpointRouteBuilder MapMobileTaskEndpoints(this IEndpointRouteBuilder routes)
    {
        var tasks = routes.MapGroup("/api/v1/android/tasks")
            .WithTags("Android/Tasks")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        tasks.MapGet("/", ListAsync).WithName("MobileTasksList");
        tasks.MapGet("/summary", SummaryAsync).WithName("MobileTasksSummary");
        tasks.MapGet("/people", PeopleAsync).WithName("MobileTasksPeople");
        tasks.MapGet("/series", SeriesAsync).WithName("MobileTasksSeries");
        tasks.MapGet("/events", EventsAsync).WithName("MobileTasksEvents");
        tasks.MapPost("/ops", OpsAsync).WithName("MobileTasksOps");
        tasks.MapGet("/{id:guid}", DetailAsync).WithName("MobileTasksDetail");
        tasks.MapPut("/{taskId:guid}/attachments/{attachmentId:guid}", UploadAsync).WithName("MobileTasksAttachmentUpload");
        tasks.MapGet("/{taskId:guid}/attachments/{attachmentId:guid}", DownloadAsync).WithName("MobileTasksAttachmentDownload");
        tasks.MapDelete("/{taskId:guid}/attachments/{attachmentId:guid}", DeleteAttachmentAsync).WithName("MobileTasksAttachmentDelete");

        var notifications = routes.MapGroup("/api/v1/android/notifications")
            .WithTags("Android/Notifications")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        notifications.MapGet("/", NotificationsAsync).WithName("MobileNotificationsList");
        notifications.MapPost("/read", MarkReadAsync).WithName("MobileNotificationsRead");
        return routes;
    }

    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TaskService tasks,
        long? changedSinceSeq, int? take, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await tasks.ListAsync(db, access.Tenant!.Id, access.User!, changedSinceSeq ?? 0, take, ct));
    }

    private static async Task<IResult> DetailAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TaskService tasks, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var result = await tasks.DetailAsync(db, access.Tenant!.Id, access.User!, id, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> SummaryAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TaskService tasks, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await tasks.SummaryAsync(db, access.Tenant!.Id, access.User!, ct));
    }

    private static async Task<IResult> PeopleAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await TaskService.PeopleAsync(db, access.Tenant!.Id, ct));
    }

    private static async Task<IResult> SeriesAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await TaskService.SeriesAsync(db, access.Tenant!.Id, access.User!, ct));
    }

    private static async Task<IResult> OpsAsync(HttpContext http, [FromBody] TaskOpsRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] TaskService tasks, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var ops = body?.Ops ?? [];
        if (ops.Length > TaskService.MaxOpsPerBatch)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "TASK_BATCH_TOO_LARGE",
                Message = $"Bir seferde en çok {TaskService.MaxOpsPerBatch} işlem gönderilebilir.",
            });
        return JsonResults.Ok(await tasks.ApplyAsync(db, access.Tenant!, access.User!, ops, ct));
    }

    /// <summary>Raw picture body (<c>image/jpeg|png|webp</c>); the attachment id makes a retried upload one picture.</summary>
    private static async Task<IResult> UploadAsync(Guid taskId, Guid attachmentId, HttpContext http,
        [FromServices] CentralApiDbContext db, [FromServices] TaskService tasks, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var max = tasks.Options.MaxAttachmentBytes;
        if (http.Request.ContentLength is { } declared && declared > max)
            return TooLarge(max);
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await http.Request.Body.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > max) return TooLarge(max);
            buffer.Write(chunk, 0, read);
        }
        var result = await tasks.AddAttachmentAsync(db, access.Tenant!, access.User!, taskId, attachmentId, http.Request.ContentType, buffer.ToArray(), ct);
        return result.Succeeded ? JsonResults.Ok(result.Value) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static IResult TooLarge(int max) =>
        JsonResults.Status(StatusCodes.Status413PayloadTooLarge, new ApiError
        {
            ErrorCode = "TASK_ATTACHMENT_TOO_LARGE",
            Message = $"Resim en çok {max / 1024 / 1024} MB olabilir.",
        });

    private static async Task<IResult> DownloadAsync(Guid taskId, Guid attachmentId, HttpContext http,
        [FromServices] CentralApiDbContext db, [FromServices] TaskService tasks, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var result = await tasks.ReadAttachmentAsync(db, access.Tenant!.Id, access.User!, taskId, attachmentId, ct);
        if (!result.Succeeded) return JsonResults.Status(result.StatusCode, result.Error);
        // A picture never changes under its id; the phone may keep it as long as it likes.
        http.Response.Headers.CacheControl = "private, max-age=31536000, immutable";
        return Results.Bytes(result.Value.Data, result.Value.ContentType);
    }

    private static async Task<IResult> DeleteAttachmentAsync(Guid taskId, Guid attachmentId, HttpContext http,
        [FromServices] CentralApiDbContext db, [FromServices] TaskService tasks, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var result = await tasks.DeleteAttachmentAsync(db, access.Tenant!, access.User!, taskId, attachmentId, ct);
        return result.Succeeded ? Results.NoContent() : JsonResults.Status(result.StatusCode, result.Error);
    }

    /// <summary>
    /// Long-poll: answers at once when the company's task version differs from <c>version</c>, else waits up
    /// to <c>wait</c> seconds for a change (200 with the new version) or times out (204). A phone reads its
    /// tasks and notifications after a 200; the version restarts with the server, which is simply a change.
    /// </summary>
    private static async Task<IResult> EventsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] ITenantEventHub hub,
        long? version, int? wait, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var seconds = wait ?? DefaultWaitSeconds;
        if (seconds < MinWaitSeconds || seconds > MaxWaitSeconds)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "INVALID_WAIT",
                Message = $"wait must be between {MinWaitSeconds} and {MaxWaitSeconds} seconds.",
            });
        var tenantId = access.Tenant!.Id;
        var current = hub.Version(tenantId, TenantEventTopics.Tasks);
        if (version != current) return JsonResults.Ok(new TaskEventsResponse { Version = current });

        var deadline = DateTimeOffset.UtcNow.AddSeconds(seconds);
        // Other topics wake the same waiter; keep waiting until our topic moves or the time is up.
        while (!ct.IsCancellationRequested)
        {
            var left = deadline - DateTimeOffset.UtcNow;
            if (left <= TimeSpan.Zero) break;
            await hub.WaitAsync(tenantId, left, ct);
            var now = hub.Version(tenantId, TenantEventTopics.Tasks);
            if (now != current) return JsonResults.Ok(new TaskEventsResponse { Version = now });
        }
        return Results.NoContent();
    }

    private static async Task<IResult> NotificationsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TaskService tasks,
        long? changedSinceSeq, int? take, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await tasks.NotificationsAsync(db, access.Tenant!.Id, access.User!.Id, changedSinceSeq ?? 0, take, ct));
    }

    private static async Task<IResult> MarkReadAsync(HttpContext http, [FromBody] MarkNotificationsReadRequest? body,
        [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var changed = await TaskService.MarkReadAsync(db, access.Tenant!.Id, access.User!.Id, body ?? new MarkNotificationsReadRequest(), ct);
        return JsonResults.Ok(new { marked = changed });
    }
}
