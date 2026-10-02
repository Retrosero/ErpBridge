using System.Text;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Pages;
using ErpBridge.Portal.Shared.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_PANEL_GIRIS P6: the phone's tasks in the panel — the list by the user's place in each task, the detail's actions as
/// ops with their own ids, a new task, a recurring one, the pictures through the circuit, and the notification bell.
/// </summary>
public sealed class PortalTaskPagesTests : PortalPageTestContext
{
    private const string Tasks = "/api/v1/android/tasks";
    private static readonly Guid Me = Guid.Parse("aaaaaaaa-0000-0000-0000-0000000000ff");
    private static readonly Guid Ali = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Mine = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    private static readonly Guid Theirs = Guid.Parse("cccccccc-0000-0000-0000-000000000002");
    private static readonly Guid Picture = Guid.Parse("dddddddd-0000-0000-0000-000000000001");

    /// <summary>A token whose payload names the user, as the server's does (<c>sub</c>).</summary>
    private static string Token()
    {
        static string Part(string json) => Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{Part("{\"alg\":\"none\"}")}.{Part($"{{\"sub\":\"{Me:D}\"}}")}.imza";
    }

    private static object Task(Guid id, string title, Guid assignee, bool attachment = false) => new
    {
        id, title, description = "Raf düzeni", priority = "HIGH", status = "OPEN", createdByUserId = Ali, createdByName = "Ali Bey",
        createdAtMs = 1_790_000_000_000L, updatedAtMs = 1_790_000_000_000L, dueAtMs = 1_790_100_000_000L, requiresPhoto = false,
        customerCode = "C1", customerName = "Yılmaz Market", isDeleted = false, updatedSeq = 5,
        assignees = new[] { new { userId = assignee, name = assignee == Me ? "Firma Sahibi" : "Ali Bey" } },
        followers = Array.Empty<object>(),
        subtasks = new[] { new { id = Guid.NewGuid(), title = "Ön raf", isDone = false, sortOrder = 0 } },
        attachments = attachment ? new[] { new { id = Picture, contentType = "image/jpeg", sizeBytes = 10, uploadedByUserId = Ali, uploadedByName = "Ali Bey", createdAtMs = 1L } } : [],
        commentCount = 0, canEdit = true, canWork = true,
        comments = Array.Empty<object>(), events = Array.Empty<object>(),
    };

    private FakeCentralApi Setup(string? page = null)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(token: Token()));
        api.Answer(Tasks + "/summary", new { openAssignedCount = 1, overdueCount = 0, unreadCount = 2, canManage = true });
        api.Answer(Tasks + "/people", new[] { new { id = Me, fullName = "Firma Sahibi", roles = new[] { "ADMIN" } }, new { id = Ali, fullName = "Ali Bey", roles = new[] { "SALES" } } });
        api.Answer(Tasks + "?changedSinceSeq=0&take=500", new { tasks = new[] { Task(Mine, "Rafları düzenle", Me, attachment: true), Task(Theirs, "Ali'nin görevi", Ali) }, latestSeq = 5, hasMore = false });
        api.Answer(Tasks + $"/{Mine:D}", Task(Mine, "Rafları düzenle", Me, attachment: true));
        api.Answer(Tasks + $"/{Mine:D}/attachments/{Picture:D}", new { image = true });
        api.Answer(Tasks + "/ops", new { results = Array.Empty<object>(), tasks = Array.Empty<object>(), series = Array.Empty<object>() });
        api.Answer(Tasks + "/series", Array.Empty<object>());
        if (page is not null) Services.GetRequiredService<NavigationManager>().NavigateTo(page);
        return api;
    }

    private static JsonElement LastOps(FakeCentralApi api) =>
        JsonDocument.Parse(api.Requests.Last(r => r.Method == HttpMethod.Post && r.PathAndQuery == Tasks + "/ops").Body!).RootElement.GetProperty("ops").Clone();

    [Fact]
    public void The_list_starts_with_the_users_own_tasks_and_a_manager_may_see_all()
    {
        Setup();
        var cut = Render<Gorevler>();

        cut.WaitForAssertion(() => cut.FindAll("#tasks-table tbody tr").Should().ContainSingle().Which.GetAttribute("data-task").Should().Be(Mine.ToString()));
        cut.Find("#tasks-unread").TextContent.Should().Contain("2");

        cut.Find("#tasks-scope").Change("all");
        cut.FindAll("#tasks-table tbody tr").Should().HaveCount(2);
    }

    [Fact]
    public void Completing_a_task_sends_one_op_and_its_picture_comes_through_the_circuit()
    {
        var api = Setup();
        var cut = Render<Gorevler>();
        cut.WaitForAssertion(() => cut.Find($"[data-task='{Mine}']"));

        cut.Find($"[data-task='{Mine}']").Click();
        cut.WaitForAssertion(() => cut.Find("#task-detail"));
        cut.WaitForAssertion(() => cut.Find($"[data-attachment='{Picture}'] img").GetAttribute("src").Should().StartWith("data:"));

        cut.Find("#task-complete").Click();
        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.PathAndQuery == Tasks + "/ops"));
        var op = LastOps(api)[0];
        op.GetProperty("type").GetString().Should().Be("complete_task");
        op.GetProperty("taskId").GetGuid().Should().Be(Mine);
        Guid.TryParse(op.GetProperty("opId").GetString(), out _).Should().BeTrue();
    }

    [Fact]
    public void A_refused_op_shows_its_reason()
    {
        var api = Setup();
        api.Answer(Tasks + "/ops", new { results = new[] { new { opId = Guid.NewGuid(), status = "rejected", errorCode = "TASK_PHOTO_REQUIRED", message = "Bu görev fotoğraf eklenmeden tamamlanamaz." } }, tasks = Array.Empty<object>() });
        var cut = Render<Gorevler>();
        cut.WaitForAssertion(() => cut.Find($"[data-task='{Mine}']"));
        cut.Find($"[data-task='{Mine}']").Click();
        cut.WaitForAssertion(() => cut.Find("#task-complete"));

        cut.Find("#task-complete").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("fotoğraf eklenmeden"));
    }

    [Fact]
    public void A_new_task_is_one_create_op_assigned_to_the_chosen_people()
    {
        var api = Setup();
        var cut = Render<Gorevler>();
        cut.WaitForAssertion(() => cut.Find("#task-new"));

        cut.Find("#task-new").Click();
        cut.WaitForAssertion(() => cut.Find("#task-title"));
        cut.Find("#task-title").Change("Stok say");
        cut.Find("#task-due").Change("2026-09-30T17:30");
        cut.Find($"#task-assignees [data-person='{Ali}'] input").Change(true);
        cut.Find("#task-subtask-lines").Change("Depo 1\nDepo 2");
        cut.Find("#task-save").Click();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.PathAndQuery == Tasks + "/ops"));
        var op = LastOps(api)[0];
        op.GetProperty("type").GetString().Should().Be("create_task");
        op.GetProperty("title").GetString().Should().Be("Stok say");
        op.GetProperty("assigneeIds").EnumerateArray().Select(a => a.GetGuid()).Should().BeEquivalentTo([Me, Ali]);
        op.GetProperty("subtasks").GetArrayLength().Should().Be(2);
        // 30 Sep 2026 17:30 in Istanbul (UTC+3) is 14:30 UTC.
        op.GetProperty("dueAtMs").GetInt64().Should().Be(new DateTimeOffset(2026, 9, 30, 14, 30, 0, TimeSpan.Zero).ToUnixTimeMilliseconds());
    }

    [Fact]
    public void A_recurring_task_sends_its_rule()
    {
        var api = Setup();
        var cut = Render<GorevSerileri>();
        cut.WaitForAssertion(() => cut.Find("#series-new"));

        cut.Find("#series-new").Click();
        cut.WaitForAssertion(() => cut.Find("#task-title"));
        cut.Find("#task-title").Change("Haftalık kasa sayımı");
        cut.Find("#series-weekdays [data-day='16'] input").Change(true);
        cut.Find("#series-time").Change("08:30");
        cut.Find("#series-save").Click();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.PathAndQuery == Tasks + "/ops"));
        var op = LastOps(api)[0];
        op.GetProperty("type").GetString().Should().Be("create_series");
        op.GetProperty("frequency").GetString().Should().Be("WEEKLY");
        op.GetProperty("weekdays").GetInt32().Should().Be(1 | 16, "Monday is on by default, Friday was added");
        op.GetProperty("timeOfDayMinutes").GetInt32().Should().Be(8 * 60 + 30);
    }

    [Fact]
    public void The_bell_shows_the_unread_count()
    {
        var api = Setup();
        api.Answer("/api/v1/android/notifications?changedSinceSeq=0&take=200", new
        {
            notifications = new[] { new { id = Guid.NewGuid(), kind = "TASK_ASSIGNED", title = "Yeni görev", body = "Rafları düzenle", taskId = Mine, createdAtMs = 1_790_000_000_000L, seq = 3 } },
            latestSeq = 3, hasMore = false, unreadCount = 1,
        });

        var cut = Render<NotificationBell>();

        cut.WaitForAssertion(() => cut.Find("#notification-bell").TextContent.Should().Contain("1"));
    }
}
