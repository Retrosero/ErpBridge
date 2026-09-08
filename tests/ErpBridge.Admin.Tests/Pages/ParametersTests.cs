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
/// bUnit tests for <see cref="Parameters"/>. The page hits the central API at
/// <c>GET /api/v1/admin/parameters</c>. We assert on the rendered DOM and the
/// request URL so the filter + 'show all' toggles are covered.
/// </summary>
public sealed class ParametersTests : BunitContext
{
    [Fact]
    public void Renders_key_filter_and_load_button_initially()
    {
        var tenants = new[] { NewTenant("Acme") };
        var state = new ParametersState
        {
            TenantListJson = SerializeAsJson(tenants),
            ParametersJson = SerializeAsJson(new ParameterListResponse
            {
                TenantId = tenants[0].Id,
                Page = 1,
                Size = 200,
                Total = 0,
                Items = Array.Empty<ParameterRecordDto>(),
            }),
        };
        RegisterRaw(state);

        var cut = Render<Parameters>();

        cut.Find("#param-tenant").Should().NotBeNull();
        cut.Find("#param-key").Should().NotBeNull();
        cut.Find("#param-all").Should().NotBeNull();
        cut.Find("#param-load").TextContent.Should().Contain("Parametreleri getir");
        cut.Markup.Should().Contain("Bir müşteri seçin");
    }

    [Fact]
    public void Shows_loading_state_while_fetching()
    {
        var tenants = new[] { NewTenant("Acme") };
        var state = new ParametersState
        {
            TenantListJson = SerializeAsJson(tenants),
            Gate = new TaskCompletionSource<ParameterListResponse>(TaskCreationOptions.RunContinuationsAsynchronously),
        };
        RegisterRaw(state);

        var cut = Render<Parameters>();
        var select = cut.Find("#param-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("#param-load").Click();

        cut.Find(".admin-loading").TextContent.Should().Contain("Parametreler");

        state.Gate.SetResult(new ParameterListResponse
        {
            TenantId = tenants[0].Id,
            Page = 1,
            Size = 200,
            Total = 0,
            Items = Array.Empty<ParameterRecordDto>(),
        });
        cut.WaitForState(() => cut.FindAll(".admin-state, .admin-data-table").Count > 0);
    }

    [Fact]
    public void Renders_table_with_parameters_after_successful_load()
    {
        var tenants = new[] { NewTenant("Acme") };
        var items = new[]
        {
            NewParameter(id: "KASA_HESAP", adi: "Varsayılan kasa", degeri: "ANA_KASA", updatedAtUtc: DateTimeOffset.Parse("2026-09-04T10:00:00Z")),
            NewParameter(id: "DEPO_KOD", adi: "Ana depo", degeri: "D01", updatedAtUtc: DateTimeOffset.Parse("2026-09-04T11:00:00Z")),
        };
        var state = new ParametersState
        {
            TenantListJson = SerializeAsJson(tenants),
            ParametersJson = SerializeAsJson(new ParameterListResponse
            {
                TenantId = tenants[0].Id,
                Page = 1,
                Size = 200,
                Total = items.Length,
                Items = items,
            }),
        };
        RegisterRaw(state);

        var cut = Render<Parameters>();
        var select = cut.Find("#param-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("#param-load").Click();

        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 2);
        cut.Markup.Should().Contain("KASA_HESAP");
        cut.Markup.Should().Contain("ANA_KASA");
        cut.Markup.Should().Contain("D01");
        cut.Markup.Should().Contain("MikroDB_V15_02");
        state.LastParametersRequestUrl.Should().NotBeNull();
        state.LastParametersRequestUrl.Should().Contain("size=200");
        state.LastParametersRequestUrl.Should().NotContain("size=1000");
    }

    [Fact]
    public void Renders_empty_state_when_no_parameters()
    {
        var tenants = new[] { NewTenant("Acme") };
        var state = new ParametersState
        {
            TenantListJson = SerializeAsJson(tenants),
            ParametersJson = SerializeAsJson(new ParameterListResponse
            {
                TenantId = tenants[0].Id,
                Page = 1,
                Size = 200,
                Total = 0,
                Items = Array.Empty<ParameterRecordDto>(),
            }),
        };
        RegisterRaw(state);

        var cut = Render<Parameters>();
        var select = cut.Find("#param-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("#param-load").Click();

        cut.WaitForState(() => cut.FindAll(".admin-state").Count > 0);
        cut.Markup.Should().Contain("Filtreye uyan parametre yok");
    }

    [Fact]
    public void Renders_error_state_when_api_throws()
    {
        var tenants = new[] { NewTenant("Acme") };
        var state = new ParametersState
        {
            TenantListJson = SerializeAsJson(tenants),
            ErrorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(@"{""errorCode"":""BAD"",""message"":""Geçersiz müşteri""}", Encoding.UTF8, "application/json")
            },
        };
        RegisterRaw(state);

        var cut = Render<Parameters>();
        var select = cut.Find("#param-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("#param-load").Click();

        cut.WaitForState(() => cut.FindAll(".admin-state--error").Count > 0);
        cut.Find(".admin-state--error").TextContent.Should().Contain("Geçersiz müşteri");
    }

    [Fact]
    public void Show_all_toggle_requests_larger_page_size()
    {
        var tenants = new[] { NewTenant("Acme") };
        var state = new ParametersState
        {
            TenantListJson = SerializeAsJson(tenants),
            ParametersJson = SerializeAsJson(new ParameterListResponse
            {
                TenantId = tenants[0].Id,
                Page = 1,
                Size = 1000,
                Total = 0,
                Items = Array.Empty<ParameterRecordDto>(),
            }),
        };
        RegisterRaw(state);

        var cut = Render<Parameters>();
        var select = cut.Find("#param-tenant");
        select.Change(tenants[0].Id.ToString());
        cut.Find("#param-all").Change(true);
        cut.Find("#param-load").Click();

        cut.WaitForState(() => state.LastParametersRequestUrl is not null);
        state.LastParametersRequestUrl.Should().Contain("size=1000");
    }

    // ---- Test helpers ----

    private static TenantDto NewTenant(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        IsActive = true,
        CreatedAtUtc = DateTimeOffset.UtcNow,
    };

    private static ParameterRecordDto NewParameter(string id, string adi, string degeri, DateTimeOffset? updatedAtUtc = null) => new()
    {
        ParametreID = id,
        ParametreAdi = adi,
        ParametreDegeri = degeri,
        SourceDatabase = "MikroDB_V15_02",
        UpdatedAtUtc = updatedAtUtc ?? DateTimeOffset.UtcNow,
    };

    private static string SerializeAsJson<T>(T value) =>
        System.Text.Json.JsonSerializer.Serialize(value, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

    private void RegisterRaw(ParametersState state)
    {
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(new ParametersHandler(state)) { BaseAddress = new Uri("https://centralapi.test/") }, tokenStore));
    }

    private sealed class ParametersState
    {
        public string? TenantListJson { get; set; }
        public string? ParametersJson { get; set; }
        public HttpResponseMessage? ErrorResponse { get; set; }
        public TaskCompletionSource<ParameterListResponse>? Gate { get; set; }
        public string? LastParametersRequestUrl { get; set; }
    }

    private sealed class ParametersHandler : HttpMessageHandler
    {
        private readonly ParametersState _state;
        public ParametersHandler(ParametersState state) { _state = state; }

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
            if (path.EndsWith("/admin/parameters", StringComparison.Ordinal))
            {
                _state.LastParametersRequestUrl = request.RequestUri?.ToString();
                if (_state.ErrorResponse is not null) return Task.FromResult(_state.ErrorResponse);
                if (_state.Gate is not null) return AwaitGate(_state.Gate);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_state.ParametersJson ?? "{}", Encoding.UTF8, "application/json")
                });
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static async Task<HttpResponseMessage> AwaitGate(TaskCompletionSource<ParameterListResponse> gate)
        {
            var value = await gate.Task.ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(value, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json")
            };
        }
    }
}
