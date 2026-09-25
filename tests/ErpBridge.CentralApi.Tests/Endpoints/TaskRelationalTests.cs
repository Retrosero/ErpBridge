using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tasks;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Görevler ve bildirimler (docs/GOAL_GOREVLER.md). Relational because every change takes its number from
/// the tenant counter inside its transaction and a rejected operation is rolled back to a savepoint.
/// </summary>
public sealed class TaskRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];

    private readonly SqliteCentralApiFactory _factory;

    public TaskRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_manager_assigns_a_task_the_assignee_sees_it_and_is_notified_and_others_do_not_see_it()
    {
        var c = await CompanyAsync();
        var taskId = Guid.NewGuid();
        var ops = await OpsAsync(c.Mehmet, new
        {
            opId = Guid.NewGuid(), type = "create_task", taskId, title = "Raf düzeni", priority = "HIGH",
            dueAtMs = TaskService.NowMs() + 86_400_000, customerCode = "C-001", customerName = "Bakkal Ali",
            assigneeIds = new[] { c.AliId }, followerIds = new[] { c.PatronId },
            subtasks = new[] { new { id = Guid.NewGuid(), title = "Ön yüzü düzelt" }, new { id = Guid.NewGuid(), title = "Fotoğraf çek" } },
        });
        ops.Results.Should().ContainSingle(r => r.Status == "applied");

        var aliTasks = await ListAsync(c.Ali);
        var seen = aliTasks.Tasks.Should().ContainSingle(t => t.Id == taskId).Subject;
        seen.Assignees.Select(a => a.Name).Should().Equal("Ali Saha");
        seen.Followers.Select(a => a.Name).Should().Equal("Patron");
        seen.Subtasks.Select(s => s.Title).Should().Equal("Ön yüzü düzelt", "Fotoğraf çek");
        seen.CanEdit.Should().BeFalse();
        seen.CanWork.Should().BeTrue();
        (await ListAsync(c.Veli)).Tasks.Should().BeEmpty("Veli is neither creator, assignee nor follower");
        (await ListAsync(c.Patron)).Tasks.Should().Contain(t => t.Id == taskId, "an administrator sees every task");

        var notices = await NotificationsAsync(c.Ali);
        notices.Notifications.Should().ContainSingle(n => n.Kind == "TASK_ASSIGNED" && n.TaskId == taskId && n.Title == "Yeni görev: Raf düzeni");
        notices.UnreadCount.Should().Be(1);
        (await NotificationsAsync(c.Mehmet)).Notifications.Should().BeEmpty("nobody is told about their own action");

        var summary = await GetJsonAsync<TaskSummaryDto>(c.Ali, "/api/v1/android/tasks/summary");
        summary.OpenAssignedCount.Should().Be(1);
        summary.UnreadCount.Should().Be(1);
        summary.CanManage.Should().BeFalse();
    }

    [Fact]
    public async Task A_field_user_assigns_only_themselves_and_a_rejected_op_does_not_stop_the_batch()
    {
        var c = await CompanyAsync();
        var mine = Guid.NewGuid();
        var foreign = Guid.NewGuid();
        var response = await OpsAsync(c.Ali,
            new { opId = Guid.NewGuid(), type = "create_task", taskId = foreign, title = "Veli'ye iş", assigneeIds = new[] { c.VeliId } },
            new { opId = Guid.NewGuid(), type = "create_task", taskId = mine, title = "Kendime not", assigneeIds = new[] { c.AliId } },
            new { opId = Guid.NewGuid(), type = "create_task", taskId = Guid.NewGuid(), title = "   " });

        response.Results.Select(r => r.Status).Should().Equal("rejected", "applied", "rejected");
        response.Results[0].ErrorCode.Should().Be("TASK_ASSIGN_FORBIDDEN");
        response.Results[2].ErrorCode.Should().Be("TASK_TITLE_REQUIRED");
        (await CountTasksAsync(c.Id, foreign)).Should().Be(0);
        response.Tasks.Should().ContainSingle(t => t.Id == mine);
    }

    [Fact]
    public async Task A_batch_sent_twice_is_applied_once()
    {
        var c = await CompanyAsync();
        var taskId = Guid.NewGuid();
        var op = new { opId = Guid.NewGuid(), type = "create_task", taskId, title = "Tekrar", assigneeIds = new[] { c.AliId } };
        (await OpsAsync(c.Patron, op)).Results.Single().Status.Should().Be("applied");
        (await OpsAsync(c.Patron, op)).Results.Single().Status.Should().Be("duplicate");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.WorkTaskEvents.CountAsync(e => e.TaskId == taskId && e.Action == WorkTaskActions.Created)).Should().Be(1);
        (await db.UserNotifications.CountAsync(n => n.TaskId == taskId)).Should().Be(1);
    }

    [Fact]
    public async Task A_photo_task_completes_only_with_a_picture_and_completion_tells_the_creator_and_followers()
    {
        var c = await CompanyAsync();
        var taskId = Guid.NewGuid();
        await OpsAsync(c.Patron, new
        {
            opId = Guid.NewGuid(), type = "create_task", taskId, title = "Vitrin", requiresPhoto = true,
            assigneeIds = new[] { c.AliId }, followerIds = new[] { c.MehmetId },
        });

        var early = await OpsAsync(c.Ali, new { opId = Guid.NewGuid(), type = "complete_task", taskId });
        early.Results.Single().ErrorCode.Should().Be("TASK_PHOTO_REQUIRED");

        var attachmentId = Guid.NewGuid();
        (await UploadAsync(c.Veli, taskId, attachmentId, Jpeg)).StatusCode.Should().Be(HttpStatusCode.NotFound, "Veli cannot see the task");
        (await UploadAsync(c.Ali, taskId, attachmentId, [0x3C, 0x68, 0x74, 0x6D, 0x6C])).StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        (await UploadAsync(c.Ali, taskId, attachmentId, Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await UploadAsync(c.Ali, taskId, attachmentId, Jpeg)).StatusCode.Should().Be(HttpStatusCode.OK, "a retried upload is the same picture");

        var download = await _factory.CreateClient().GetAsync($"/api/v1/android/tasks/{taskId}/attachments/{attachmentId}", c.Mehmet);
        download.StatusCode.Should().Be(HttpStatusCode.OK);
        (await download.Content.ReadAsByteArrayAsync()).Should().Equal(Jpeg);
        download.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");

        (await OpsAsync(c.Ali, new { opId = Guid.NewGuid(), type = "complete_task", taskId })).Results.Single().Status.Should().Be("applied");
        var task = (await ListAsync(c.Patron)).Tasks.Single(t => t.Id == taskId);
        task.Status.Should().Be("DONE");
        task.CompletedByName.Should().Be("Ali Saha");
        task.Attachments.Should().ContainSingle(a => a.Id == attachmentId);

        (await NotificationsAsync(c.Patron)).Notifications.Should().ContainSingle(n => n.Kind == "TASK_COMPLETED" && n.TaskId == taskId);
        (await NotificationsAsync(c.Mehmet)).Notifications.Should().ContainSingle(n => n.Kind == "TASK_COMPLETED");
        (await NotificationsAsync(c.Ali)).Notifications.Should().NotContain(n => n.Kind == "TASK_COMPLETED");
    }

    [Fact]
    public async Task A_scheduled_task_stays_hidden_from_the_assignee_until_it_starts()
    {
        var c = await CompanyAsync();
        var taskId = Guid.NewGuid();
        var start = TaskService.NowMs() + 3_600_000;
        await OpsAsync(c.Patron, new { opId = Guid.NewGuid(), type = "create_task", taskId, title = "Yarın ziyaret", startAtMs = start, assigneeIds = new[] { c.AliId } });

        (await ListAsync(c.Ali)).Tasks.Should().BeEmpty();
        (await NotificationsAsync(c.Ali)).Notifications.Should().BeEmpty();
        var before = (await ListAsync(c.Patron)).LatestSeq;

        await RunSchedulerAsync(start + 1_000);

        (await ListAsync(c.Ali)).Tasks.Should().ContainSingle(t => t.Id == taskId);
        (await NotificationsAsync(c.Ali)).Notifications.Should().ContainSingle(n => n.Kind == "TASK_ASSIGNED");
        (await ListAsync(c.Patron, since: before)).Tasks.Should().ContainSingle(t => t.Id == taskId, "the start is a change phones pull");
    }

    [Fact]
    public async Task The_scheduler_warns_before_the_due_time_and_once_after_it()
    {
        var c = await CompanyAsync();
        var taskId = Guid.NewGuid();
        var due = TaskService.NowMs() + 30 * 60_000;
        await OpsAsync(c.Patron, new { opId = Guid.NewGuid(), type = "create_task", taskId, title = "Tahsilat", dueAtMs = due, assigneeIds = new[] { c.AliId } });

        await RunSchedulerAsync(TaskService.NowMs());
        (await NotificationsAsync(c.Ali)).Notifications.Should().ContainSingle(n => n.Kind == "TASK_DUE_SOON");

        await RunSchedulerAsync(due + 60_000);
        await RunSchedulerAsync(due + 120_000);
        (await NotificationsAsync(c.Ali)).Notifications.Count(n => n.Kind == "TASK_OVERDUE").Should().Be(1);
        (await NotificationsAsync(c.Patron)).Notifications.Should().ContainSingle(n => n.Kind == "TASK_OVERDUE", "the creator hears it is late");
    }

    [Fact]
    public async Task A_repeating_task_creates_its_occurrence_and_moves_to_the_next_one()
    {
        var c = await CompanyAsync();
        var seriesId = Guid.NewGuid();
        var response = await OpsAsync(c.Mehmet, new
        {
            opId = Guid.NewGuid(), type = "create_series", seriesId, title = "Haftalık sayım", frequency = "DAILY", interval = 1,
            timeOfDayMinutes = 9 * 60, dueAfterMinutes = 120, assigneeIds = new[] { c.AliId }, subtaskTitles = new[] { "Depo", "Araç" },
        });
        response.Results.Single().Status.Should().Be("applied");
        var first = response.Series.Single().NextRunAtMs;

        await RunSchedulerAsync(first + 1_000);

        var occurrence = (await ListAsync(c.Ali)).Tasks.Should().ContainSingle(t => t.SeriesId == seriesId).Subject;
        occurrence.Subtasks.Select(s => s.Title).Should().Equal("Depo", "Araç");
        occurrence.DueAtMs.Should().Be(first + 120 * 60_000);
        (await NotificationsAsync(c.Ali)).Notifications.Should().ContainSingle(n => n.Kind == "TASK_ASSIGNED" && n.TaskId == occurrence.Id);
        var series = await GetJsonAsync<TaskSeriesDto[]>(c.Mehmet, "/api/v1/android/tasks/series");
        series.Single(s => s.Id == seriesId).NextRunAtMs.Should().Be(first + 86_400_000);
    }

    [Fact]
    public async Task A_change_inside_a_task_is_pulled_incrementally_and_comments_notify_the_others()
    {
        var c = await CompanyAsync();
        var taskId = Guid.NewGuid();
        var subtaskId = Guid.NewGuid();
        await OpsAsync(c.Patron, new
        {
            opId = Guid.NewGuid(), type = "create_task", taskId, title = "Kampanya", assigneeIds = new[] { c.AliId },
            subtasks = new[] { new { id = subtaskId, title = "Afiş as" } },
        });
        var cursor = (await ListAsync(c.Ali)).LatestSeq;
        (await ListAsync(c.Ali, since: cursor)).Tasks.Should().BeEmpty();

        await OpsAsync(c.Ali, new { opId = Guid.NewGuid(), type = "toggle_subtask", taskId, subtaskId, isDone = true });
        await OpsAsync(c.Ali, new { opId = Guid.NewGuid(), type = "add_comment", taskId, commentId = Guid.NewGuid(), text = "Afiş asıldı" });

        var changed = (await ListAsync(c.Ali, since: cursor)).Tasks.Should().ContainSingle().Subject;
        changed.Subtasks.Single().IsDone.Should().BeTrue();
        changed.Subtasks.Single().DoneByName.Should().Be("Ali Saha");
        changed.CommentCount.Should().Be(1);

        var detail = await GetJsonAsync<TaskDetailDto>(c.Patron, $"/api/v1/android/tasks/{taskId}");
        detail.Comments.Single().Text.Should().Be("Afiş asıldı");
        detail.Events.Select(e => e.Action).Should().Contain(["CREATED", "SUBTASK_DONE", "COMMENTED"]);

        var notices = await NotificationsAsync(c.Patron);
        notices.Notifications.Should().ContainSingle(n => n.Kind == "TASK_COMMENTED");
        var read = await _factory.CreateClient().PostJsonAsync("/api/v1/android/notifications/read", new { all = true }, c.Patron);
        read.StatusCode.Should().Be(HttpStatusCode.OK);
        (await NotificationsAsync(c.Patron)).UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task Only_the_creator_or_a_manager_edits_and_an_assignee_can_still_work_on_it()
    {
        var c = await CompanyAsync();
        var taskId = Guid.NewGuid();
        await OpsAsync(c.Patron, new { opId = Guid.NewGuid(), type = "create_task", taskId, title = "Fiyat etiketi", assigneeIds = new[] { c.AliId } });

        var edit = await OpsAsync(c.Ali, new { opId = Guid.NewGuid(), type = "update_task", taskId, title = "Değişti" });
        edit.Results.Single().ErrorCode.Should().Be("TASK_FORBIDDEN");
        var members = await OpsAsync(c.Mehmet, new { opId = Guid.NewGuid(), type = "set_members", taskId, assigneeIds = new[] { c.AliId, c.VeliId } });
        members.Results.Single().Status.Should().Be("applied");
        members.Tasks.Single().Assignees.Should().HaveCount(2);
        (await NotificationsAsync(c.Veli)).Notifications.Should().ContainSingle(n => n.Kind == "TASK_ASSIGNED");
        (await NotificationsAsync(c.Ali)).Notifications.Count(n => n.Kind == "TASK_ASSIGNED").Should().Be(1, "Ali was assigned already");

        var people = await GetJsonAsync<TaskPersonDto[]>(c.Ali, "/api/v1/android/tasks/people");
        people.Select(p => p.FullName).Should().Contain(["Ali Saha", "Mehmet Müdür", "Patron", "Veli Saha"]);
    }

    [Fact]
    public async Task A_visit_reminder_needs_a_customer_and_repeating_tasks_carry_it_from_their_start()
    {
        var c = await CompanyAsync();
        var withCustomer = Guid.NewGuid();
        var withoutCustomer = Guid.NewGuid();
        var from = TaskService.NowMs() + 86_400_000;
        var created = await OpsAsync(c.Mehmet,
            new { opId = Guid.NewGuid(), type = "create_task", taskId = withCustomer, title = "Rafı kontrol et", customerCode = "C-1", customerName = "Bakkal", visitReminder = true, visitReminderFromMs = from, assigneeIds = new[] { c.AliId } },
            new { opId = Guid.NewGuid(), type = "create_task", taskId = withoutCustomer, title = "Carisiz", visitReminder = true, assigneeIds = new[] { c.AliId } });
        var task = created.Tasks.Single(t => t.Id == withCustomer);
        task.VisitReminder.Should().BeTrue();
        task.VisitReminderFromMs.Should().Be(from);
        created.Tasks.Single(t => t.Id == withoutCustomer).VisitReminder.Should().BeFalse("there is no visit to remind at without a customer");

        // An app released before the field edits the title: the reminder stays as it was.
        var older = await OpsAsync(c.Mehmet, new { opId = Guid.NewGuid(), type = "update_task", taskId = withCustomer, title = "Rafı kontrol et!", customerCode = "C-1", customerName = "Bakkal" });
        older.Tasks.Single().VisitReminder.Should().BeTrue();
        older.Tasks.Single().VisitReminderFromMs.Should().Be(from);

        var edited = await OpsAsync(c.Mehmet, new { opId = Guid.NewGuid(), type = "update_task", taskId = withCustomer, title = "Rafı kontrol et", customerCode = "C-1", customerName = "Bakkal", visitReminder = false });
        edited.Tasks.Single().VisitReminder.Should().BeFalse();
        edited.Tasks.Single().VisitReminderFromMs.Should().BeNull();

        var seriesId = Guid.NewGuid();
        var series = await OpsAsync(c.Mehmet, new
        {
            opId = Guid.NewGuid(), type = "create_series", seriesId, title = "Aylık raf", frequency = "DAILY", interval = 1,
            timeOfDayMinutes = 9 * 60, customerCode = "C-1", customerName = "Bakkal", visitReminder = true, assigneeIds = new[] { c.AliId },
        });
        series.Series.Single().VisitReminder.Should().BeTrue();
        var olderSeries = await OpsAsync(c.Mehmet, new
        {
            opId = Guid.NewGuid(), type = "update_series", seriesId, title = "Aylık raf", frequency = "DAILY", interval = 1,
            timeOfDayMinutes = 9 * 60, customerCode = "C-1", customerName = "Bakkal", assigneeIds = new[] { c.AliId },
        });
        olderSeries.Series.Single().VisitReminder.Should().BeTrue("an older app's series edit keeps the reminder too");
        var run = olderSeries.Series.Single().NextRunAtMs;
        await RunSchedulerAsync(run + 1_000);
        var occurrence = (await ListAsync(c.Ali)).Tasks.Single(t => t.SeriesId == seriesId);
        occurrence.VisitReminder.Should().BeTrue();
        occurrence.VisitReminderFromMs.Should().Be(run, "an occurrence reminds from the moment it appears");
    }

    // ---- helpers -----------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Mehmet, string Ali, string Veli, Guid PatronId, Guid MehmetId, Guid AliId, Guid VeliId);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"TSK-{suffix}", $"Task tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await client.PutJsonAsync($"{basePath}/subscription", new { seats = 6, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        async Task Create(string username, string fullName, string role) =>
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        await Create("patron", "Patron", "ADMIN");
        await Create("mehmet", "Mehmet Müdür", "MANAGER");
        await Create("ali", "Ali Saha", "SALES");
        await Create("veli", "Veli Saha", "SALES");
        var overview = await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var code = overview.TenantCode!;
        var patron = await LoginAsync(code, "patron", $"DEV-P-{suffix}");
        var people = await GetJsonAsync<TaskPersonDto[]>(patron, "/api/v1/android/tasks/people");
        Guid Id(string name) => people.Single(p => p.FullName == name).Id;
        return new Company(tenant.Id, patron,
            await LoginAsync(code, "mehmet", $"DEV-M-{suffix}"),
            await LoginAsync(code, "ali", $"DEV-A-{suffix}"),
            await LoginAsync(code, "veli", $"DEV-V-{suffix}"),
            Id("Patron"), Id("Mehmet Müdür"), Id("Ali Saha"), Id("Veli Saha"));
    }

    private async Task<TaskOpsResponse> OpsAsync(string token, params object[] ops)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/tasks/ops", new { ops }, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<TaskOpsResponse>();
    }

    private Task<TaskListResponse> ListAsync(string token, long since = 0) =>
        GetJsonAsync<TaskListResponse>(token, $"/api/v1/android/tasks?changedSinceSeq={since}");

    private Task<UserNotificationListResponse> NotificationsAsync(string token) =>
        GetJsonAsync<UserNotificationListResponse>(token, "/api/v1/android/notifications");

    private async Task<HttpResponseMessage> UploadAsync(string token, Guid taskId, Guid attachmentId, byte[] data)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/android/tasks/{taskId}/attachments/{attachmentId}")
        {
            Content = new ByteArrayContent(data),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task RunSchedulerAsync(long nowMs)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        await scope.ServiceProvider.GetRequiredService<TaskService>().RunSchedulerAsync(db, nowMs, CancellationToken.None);
    }

    private async Task<int> CountTasksAsync(Guid tenantId, Guid taskId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return await db.WorkTasks.CountAsync(t => t.TenantId == tenantId && t.Id == taskId);
    }

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.260" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
