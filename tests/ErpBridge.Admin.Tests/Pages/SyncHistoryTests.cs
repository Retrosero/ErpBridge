using System.Net;
using System.Text;
using Bunit;
using ErpBridge.Admin.Api;
using ErpBridge.Admin.Auth;
using ErpBridge.Admin.Pages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Admin.Tests.Pages;

/// <summary>
/// bUnit tests for <see cref="SyncHistory"/>. The page drives two HTTP calls:
/// the tenant list (once on init) and <c>ListChangeSetAuditAsync</c> on click.
/// We assert on rendered DOM nodes and the most recent request URL so the
/// pagination/filter contract stays tight.
/// </summary>
public sealed class SyncHistoryTests : BunitContext
{
    [Fact]
    public void Renders_tenant_filter_and_load_button_initially()
    {
        var tenants = new[] { NewTenant("Acme"), NewTenant("Beta") };
        var state = new SyncHistoryState();
        Register(state, tenants, pagedResult: NewPagedResult(tenants[0].Id, items: Array.Empty<ChangeSetAuditEntryDto>()));

        var cut = Render<SyncHistory>();

        cut.Find("#sync-tenant").Should().NotBeNull();
        cut.Find("button.btn-primary").TextContent.Should().Contain("Olayları getir");
        // Empty state hint while no result has been loaded
        cut.Markup.Should().Contain("Bir müşteri seçin");
        state.LastChangeSetRequestUrl.Should().BeNull("no list call should be issued on init");
    }

    [Fact]
    public void Shows_loading_state_while_fetching()
    {
        var tenants = new[] { NewTenant("Acme") };
        var state = new SyncHistoryState();
        state.ChangeSetGate = new TaskCompletionSource<ChangeSetAuditPagedResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        state.TenantListJson = SerializeAsJson(tenants);
        RegisterRaw(state);

        var cut = Render<SyncHistory>();
        // Pick the first tenant so the load button becomes enabled.
        var select = cut.Find("#sync-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("button.btn-primary").Click();

        // The page renders AdminLoading while the gated task is in flight.
        cut.Find(".admin-loading").TextContent.Should().Contain("Sync olayları");

        state.ChangeSetGate.SetResult(NewPagedResult(tenants[0].Id, items: Array.Empty<ChangeSetAuditEntryDto>()));
        cut.WaitForState(() => cut.FindAll(".admin-data-table").Count > 0 || cut.FindAll(".admin-state").Count > 0);
    }

    [Fact]
    public void Renders_error_state_when_api_throws()
    {
        var tenants = new[] { NewTenant("Acme") };
        var state = new SyncHistoryState();
        state.TenantListJson = SerializeAsJson(tenants);
        state.ChangeSetResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(@"{""errorCode"":""BOOM"",""message"":""Sunucu hatası""}", Encoding.UTF8, "application/json")
        };
        RegisterRaw(state);

        var cut = Render<SyncHistory>();
        var select = cut.Find("#sync-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("button.btn-primary").Click();

        cut.WaitForState(() => cut.FindAll(".admin-state--error").Count > 0);
        cut.Find(".admin-state--error").TextContent.Should().Contain("Sunucu hatası");
    }

    [Fact]
    public void Renders_table_with_audit_entries_after_successful_load()
    {
        var tenants = new[] { NewTenant("Acme") };
        var items = new[]
        {
            NewEntry(tenants[0].Id, table: "STOKLAR", direction: "new", lastTriggerRecNo: 100, rowCount: 5, idempotencyKey: "abc-1", receivedAtUtc: DateTimeOffset.Parse("2026-09-04T10:00:00Z")),
            NewEntry(tenants[0].Id, table: "CARI_HESAPLAR", direction: "changed", lastTriggerRecNo: 200, rowCount: 3, idempotencyKey: "abc-2", receivedAtUtc: DateTimeOffset.Parse("2026-09-04T10:05:00Z")),
        };
        var state = new SyncHistoryState
        {
            ChangeSetJson = SerializeAsJson(NewPagedResult(tenants[0].Id, items, page: 1, size: 50, total: items.Length)),
            TenantListJson = SerializeAsJson(tenants),
        };
        Register(state, tenants, NewPagedResult(tenants[0].Id, items, page: 1, size: 50, total: items.Length));

        var cut = Render<SyncHistory>();
        var select = cut.Find("#sync-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("button.btn-primary").Click();

        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 2);
        cut.Markup.Should().Contain("STOKLAR");
        cut.Markup.Should().Contain("CARI_HESAPLAR");
        cut.Markup.Should().Contain("abc-1");
        cut.Markup.Should().Contain("abc-2");
        cut.Markup.Should().Contain("Toplam olay");
        cut.Markup.Should().Contain("Yeni");
        cut.Markup.Should().Contain("Değişen");
    }

    [Fact]
    public void Renders_empty_state_when_no_entries()
    {
        var tenants = new[] { NewTenant("Acme") };
        var state = new SyncHistoryState
        {
            ChangeSetJson = SerializeAsJson(NewPagedResult(tenants[0].Id, items: Array.Empty<ChangeSetAuditEntryDto>(), page: 1, size: 50, total: 0)),
            TenantListJson = SerializeAsJson(tenants),
        };
        Register(state, tenants, NewPagedResult(tenants[0].Id, items: Array.Empty<ChangeSetAuditEntryDto>(), page: 1, size: 50, total: 0));

        var cut = Render<SyncHistory>();
        var select = cut.Find("#sync-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("button.btn-primary").Click();

        cut.WaitForState(() => cut.FindAll(".admin-state").Count > 0);
        cut.Markup.Should().Contain("Bu filtre için olay yok");
    }

    [Fact]
    public void Pagination_buttons_call_api_with_correct_page()
    {
        var tenants = new[] { NewTenant("Acme") };
        var firstPage = new[] { NewEntry(tenants[0].Id, idempotencyKey: "p1-1"), NewEntry(tenants[0].Id, idempotencyKey: "p1-2") };
        var secondPage = new[] { NewEntry(tenants[0].Id, idempotencyKey: "p2-1") };
        var state = new SyncHistoryState();
        // size=2, total=3 → TotalPages=2 so the "next" button is enabled.
        state.PageResponses["1"] = SerializeAsJson(NewPagedResult(tenants[0].Id, firstPage, page: 1, size: 2, total: 3));
        state.PageResponses["2"] = SerializeAsJson(NewPagedResult(tenants[0].Id, secondPage, page: 2, size: 2, total: 3));
        state.TenantListJson = SerializeAsJson(tenants);
        RegisterRaw(state);

        var cut = Render<SyncHistory>();
        var select = cut.Find("#sync-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("button.btn-primary").Click();

        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 2);
        state.LastChangeSetRequestUrl.Should().Contain("page=1");

        cut.Find("#sync-next").Click();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);
        state.LastChangeSetRequestUrl.Should().Contain("page=2");
        cut.Markup.Should().Contain("p2-1");
    }

    [Fact]
    public void Csv_export_button_calls_export_endpoint()
    {
        var tenants = new[] { NewTenant("Acme") };
        var items = new[]
        {
            NewEntry(tenants[0].Id, table: "STOKLAR", direction: "new", idempotencyKey: "exp-1"),
        };
        var state = new SyncHistoryState();
        state.CsvResponse = "receivedAtUtc,table,direction\n2026-09-04T10:00:00Z,STOKLAR,new\n";
        state.TenantListJson = SerializeAsJson(tenants);
        state.ChangeSetJson = SerializeAsJson(NewPagedResult(tenants[0].Id, items, page: 1, size: 50, total: items.Length));
        RegisterRaw(state);

        var cut = Render<SyncHistory>();
        var select = cut.Find("#sync-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("button.btn-primary").Click();
        cut.WaitForState(() => cut.FindAll("#sync-export").Count > 0);

        cut.Find("#sync-export").Click();

        cut.WaitForState(() => cut.FindAll("[data-testid=csv-preview]").Count > 0);
        state.LastChangeSetRequestUrl.Should().Contain("/api/v1/admin/audit/changeset/export.csv");
        cut.Find("[data-testid=csv-preview]").TextContent.Should().Contain("STOKLAR");
    }

    // ---- Test helpers ----

    private static TenantDto NewTenant(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        IsActive = true,
        CreatedAtUtc = DateTimeOffset.UtcNow,
    };

    private static ChangeSetAuditEntryDto NewEntry(Guid tenantId, string table = "STOKLAR", string direction = "new", long lastTriggerRecNo = 1, int rowCount = 1, string idempotencyKey = "key-1", DateTimeOffset? receivedAtUtc = null) => new()
    {
        Id = Guid.NewGuid(),
        SourceDatabase = "MikroDB_V15_02",
        Table = table,
        TabloId = 12,
        Direction = direction,
        LastTriggerRecNo = lastTriggerRecNo,
        RowCount = rowCount,
        PayloadSha256 = "deadbeef",
        ReceivedAtUtc = receivedAtUtc ?? DateTimeOffset.UtcNow,
        AgentId = "agent-1",
        IdempotencyKey = idempotencyKey,
    };

    private static ChangeSetAuditPagedResult NewPagedResult(Guid tenantId, IReadOnlyList<ChangeSetAuditEntryDto> items, int page = 1, int size = 50, int total = 0) => new()
    {
        TenantId = tenantId,
        Page = page,
        Size = size,
        Total = total == 0 ? items.Count : total,
        Items = items,
    };

    private static string SerializeAsJson<T>(T value) =>
        System.Text.Json.JsonSerializer.Serialize(value, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

    private void Register(SyncHistoryState state, TenantDto[] tenants, ChangeSetAuditPagedResult pagedResult)
    {
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(new SyncHistoryHandler(state, tenants, pagedResult)) { BaseAddress = new Uri("https://centralapi.test/") }, tokenStore));
    }

    private void RegisterRaw(SyncHistoryState state)
    {
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(new RawSyncHistoryHandler(state)) { BaseAddress = new Uri("https://centralapi.test/") }, tokenStore));
    }

    /// <summary>
    /// Stateful harness shared across the test class. The same handler instance
    /// is reused so it can record the last request URL and choose per-call
    /// responses (e.g. pagination).
    /// </summary>
    private sealed class SyncHistoryState
    {
        public string? TenantListJson { get; set; }
        public string? ChangeSetJson { get; set; }
        public Dictionary<string, string> PageResponses { get; } = new();
        public string? CsvResponse { get; set; }
        public HttpResponseMessage? ChangeSetResponse { get; set; }
        public TaskCompletionSource<ChangeSetAuditPagedResult>? ChangeSetGate { get; set; }
        public string? LastChangeSetRequestUrl { get; set; }
    }

    private sealed class SyncHistoryHandler : HttpMessageHandler
    {
        private readonly SyncHistoryState _state;
        private readonly TenantDto[] _tenants;
        private readonly ChangeSetAuditPagedResult _paged;

        public SyncHistoryHandler(SyncHistoryState state, TenantDto[] tenants, ChangeSetAuditPagedResult paged)
        {
            _state = state;
            _tenants = tenants;
            _paged = paged;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (path.EndsWith("/admin/tenants", StringComparison.Ordinal))
            {
                return Task.FromResult(JsonOk(_tenants));
            }
            if (path.EndsWith("/admin/audit/changeset/export.csv", StringComparison.Ordinal))
            {
                _state.LastChangeSetRequestUrl = request.RequestUri?.ToString();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_state.CsvResponse ?? string.Empty, Encoding.UTF8, "text/csv")
                });
            }
            if (path.EndsWith("/admin/audit/changeset", StringComparison.Ordinal))
            {
                _state.LastChangeSetRequestUrl = request.RequestUri?.ToString();
                return Task.FromResult(JsonOk(_paged));
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }

    private sealed class RawSyncHistoryHandler : HttpMessageHandler
    {
        private readonly SyncHistoryState _state;
        public RawSyncHistoryHandler(SyncHistoryState state) { _state = state; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (path.EndsWith("/admin/tenants", StringComparison.Ordinal))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_state.TenantListJson ?? "[]", Encoding.UTF8, "application/json")
                });
            }
            if (path.EndsWith("/admin/audit/changeset/export.csv", StringComparison.Ordinal))
            {
                _state.LastChangeSetRequestUrl = request.RequestUri?.ToString();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_state.CsvResponse ?? string.Empty, Encoding.UTF8, "text/csv")
                });
            }
            if (path.EndsWith("/admin/audit/changeset", StringComparison.Ordinal))
            {
                _state.LastChangeSetRequestUrl = request.RequestUri?.ToString();
                if (_state.ChangeSetResponse is not null) return Task.FromResult(_state.ChangeSetResponse);
                if (_state.ChangeSetGate is not null) return AwaitGate(_state.ChangeSetGate);
                var qs = request.RequestUri?.Query ?? string.Empty;
                var pageMatch = System.Text.RegularExpressions.Regex.Match(qs, @"page=(\d+)");
                var key = pageMatch.Success ? pageMatch.Groups[1].Value : "1";
                if (_state.PageResponses.TryGetValue(key, out var json))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    });
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_state.ChangeSetJson ?? "{}", Encoding.UTF8, "application/json")
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static async Task<HttpResponseMessage> AwaitGate(TaskCompletionSource<ChangeSetAuditPagedResult> gate)
        {
            var value = await gate.Task.ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(value, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json")
            };
        }
    }

    private static HttpResponseMessage JsonOk<T>(T value) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(value, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json")
    };
}
