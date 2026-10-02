using System.Net;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.PortalEntryTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_GIRIS P6 (Codex #249): the panel's task pages are gated on the server too — a panel session without the
/// tasks module gets 403 from every task endpoint — while the phone's session and the notifications stay as they were.
/// </summary>
public sealed class PortalTasksAccessRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public PortalTasksAccessRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_panel_session_needs_the_tasks_module_the_phone_and_notifications_do_not_change()
    {
        var c = await EntryCompanyAsync(_factory);
        (await SendAsync(_factory, HttpMethod.Get, "/api/v1/android/tasks", c.Mudur)).StatusCode.Should().Be(HttpStatusCode.OK);

        await SetPermissionAsync(_factory, c.MudurId, PermissionKeys.ModuleTasks, PermissionValues.False);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, "/api/v1/android/tasks", c.Mudur), HttpStatusCode.Forbidden, "TASKS_MODULE_DENIED");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, "/api/v1/android/tasks/summary", c.Mudur), HttpStatusCode.Forbidden, "TASKS_MODULE_DENIED");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, "/api/v1/android/tasks/ops", c.Mudur, new { ops = Array.Empty<object>() }),
            HttpStatusCode.Forbidden, "TASKS_MODULE_DENIED");
        (await SendAsync(_factory, HttpMethod.Get, "/api/v1/android/tasks", c.MudurPhone)).StatusCode.Should().Be(HttpStatusCode.OK, "the phone hides its own screen");
        (await SendAsync(_factory, HttpMethod.Get, "/api/v1/android/notifications", c.Mudur)).StatusCode.Should().Be(HttpStatusCode.OK, "catalog requests notify there too");
    }
    [Fact]
    public async Task A_subtask_added_to_an_existing_task_is_saved()
    {
        var c = await EntryCompanyAsync(_factory);
        var taskId = Guid.NewGuid();
        var subtaskId = Guid.NewGuid();
        await OkAsync<Contracts.TaskOpsResponse>(await SendAsync(_factory, HttpMethod.Post, "/api/v1/android/tasks/ops", c.Mudur, new
        {
            ops = new object[] { new { opId = Guid.NewGuid(), type = "create_task", taskId, title = "Raf düzeni", priority = "NORMAL", assigneeIds = new[] { c.MudurId } } },
        }));

        var added = await OkAsync<Contracts.TaskOpsResponse>(await SendAsync(_factory, HttpMethod.Post, "/api/v1/android/tasks/ops", c.Mudur, new
        {
            ops = new object[]
            {
                new { opId = Guid.NewGuid(), type = "add_subtask", taskId, subtaskId, title = "Ön raflar", sortOrder = 0 },
                new { opId = Guid.NewGuid(), type = "toggle_subtask", taskId, subtaskId, isDone = true },
            },
        }));

        added.Results.Select(r => r.Status).Should().Equal("applied", "applied");
        added.Tasks.Single().Subtasks.Should().ContainSingle().Which.Should().Match<Contracts.TaskSubtaskDto>(s => s.Id == subtaskId && s.Title == "Ön raflar" && s.IsDone);
    }
}
