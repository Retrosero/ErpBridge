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
/// bUnit tests for <see cref="Parameters"/> — the catalogue-backed screen (P2a). What matters
/// here is the addressing: a value belongs to one ERP company and, inside it, to one scope, so
/// the page must not let someone load or change a set without saying which.
/// </summary>
public sealed class ParametersTests : BunitContext
{
    [Fact]
    public void The_program_and_set_pickers_come_from_the_catalogue()
    {
        var state = NewState(SetsJson: Sets(
            Set("akilli", "MobilKullanici", "MobileUser", "user", 1801, 1782),
            Set("foramikro", "MobilKullaniciFora", "DesktopUser", "user", 60, 60)));

        var cut = Render<Parameters>();

        // Written by hand, this list would drift from the catalogue, which is generated from
        // Fora's own sources.
        cut.WaitForState(() => cut.Find("#prm-program").ChildNodes.Length > 1);
        cut.Find("#prm-program").TextContent.Should().Contain("akilli").And.Contain("foramikro");
        cut.Markup.Should().Contain("Bir kapsam seçin");
        state.LastValuesUrl.Should().BeNull("nothing is loaded before a scope is chosen");
    }

    [Fact]
    public void The_company_picker_stays_visible_even_when_there_is_only_one()
    {
        var state = NewState();
        var cut = Render<Parameters>();

        cut.Find("#prm-tenant").Change(state.TenantId.ToString());
        cut.WaitForState(() => cut.Find("#prm-company").ChildNodes.Length > 1);

        // Pre-selected for convenience, but never hidden: the operator has to see which database
        // they are about to change (D3b).
        cut.Find("#prm-company").Should().NotBeNull();
        cut.Markup.Should().Contain("Tek firma olduğu için seçildi");
    }

    [Fact]
    public void A_mobile_user_set_asks_for_a_user_and_only_lists_active_ones()
    {
        var state = NewState();
        var cut = Render<Parameters>();

        cut.Find("#prm-tenant").Change(state.TenantId.ToString());
        cut.WaitForState(() => cut.FindAll("#prm-company option").Count > 1);
        cut.Find("#prm-program").Change("akilli");

        // The set has a single member, so it is chosen without a click and the scope picker shows.
        cut.WaitForState(() => cut.FindAll("#prm-scope").Count == 1);
        var scope = cut.Find("#prm-scope");
        scope.TextContent.Should().Contain("aktif").And.NotContain("ayrilmis");
        cut.Markup.Should().Contain("Mobil kullanıcı");
    }

    [Fact]
    public void The_load_button_stays_disabled_until_the_scope_is_complete()
    {
        var state = NewState();
        var cut = Render<Parameters>();

        cut.Find("#prm-tenant").Change(state.TenantId.ToString());
        cut.WaitForState(() => cut.FindAll("#prm-company option").Count > 1);
        cut.Find("#prm-program").Change("akilli");
        cut.WaitForState(() => cut.FindAll("#prm-load").Count == 1);

        // A mobile-user parameter written without naming the user would create a row no read ever
        // finds and the mirror could not place in Mikro.
        cut.Find("#prm-load").HasAttribute("disabled").Should().BeTrue();

        cut.Find("#prm-scope").Change(state.UserId.ToString());
        cut.WaitForState(() => !cut.Find("#prm-load").HasAttribute("disabled"));
    }

    [Fact]
    public void Loading_names_the_company_and_the_scope_in_the_request()
    {
        var state = NewState();
        var cut = Render<Parameters>();

        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => state.LastValuesUrl is not null);
        state.LastValuesUrl.Should().Contain($"erpCompanyId={state.CompanyId}");
        state.LastValuesUrl.Should().Contain($"mobileUserId={state.UserId}");
        state.LastValuesUrl.Should().Contain("catalogMethod=MobilKullanici");
    }

    [Fact]
    public void A_loaded_set_shows_the_value_its_default_and_why_it_may_do_nothing()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 4,
            Value(58, "DefaultKaynakDepoNo", "Kaynak depo no :", value: "3", def: "1", overridden: true, implemented: false),
            Value(89, "Goster_AnaMenu_Tahsilat", "Tahsilat girebilir", value: "1", def: "1", overridden: false, implemented: true));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 2);

        cut.Markup.Should().Contain("DefaultKaynakDepoNo").And.Contain("Kaynak depo no");
        cut.Markup.Should().Contain("sapmış");

        // A setting this release of the app ignores is labelled rather than silently inert (D16).
        cut.Markup.Should().Contain("bu sürümde etkisiz");
        cut.Markup.Should().Contain("4", "the scope revision tells a client whether its copy is current");
    }

    [Fact]
    public void Only_changed_is_passed_through_to_the_server()
    {
        var state = NewState();
        var cut = Render<Parameters>();

        cut.Find("#prm-tenant").Change(state.TenantId.ToString());
        cut.WaitForState(() => cut.FindAll("#prm-company option").Count > 1);
        cut.Find("#prm-program").Change("akilli");
        cut.WaitForState(() => cut.FindAll("#prm-scope").Count == 1);
        cut.Find("#prm-scope").Change(state.UserId.ToString());
        cut.Find("#prm-only-changed").Change(true);
        cut.Find("#prm-load").Click();

        cut.WaitForState(() => state.LastValuesUrl is not null);
        state.LastValuesUrl.Should().Contain("onlyOverridden=true");
    }

    [Fact]
    public void An_error_is_shown_rather_than_an_empty_table()
    {
        var state = NewState();
        state.ValuesError = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                @"{""errorCode"":""PARAMETER_SCOPE_MISMATCH"",""message"":""Kapsam eksik""}",
                Encoding.UTF8, "application/json"),
        };

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".admin-state--error").Count > 0);
        cut.Find(".admin-state--error").TextContent.Should().Contain("Kapsam eksik");
    }

    // ---- Test helpers ----

    private static void LoadAkilliAsync(IRenderedComponent<Parameters> cut, PageState state)
    {
        cut.Find("#prm-tenant").Change(state.TenantId.ToString());
        cut.WaitForState(() => cut.FindAll("#prm-company option").Count > 1);
        cut.Find("#prm-program").Change("akilli");
        cut.WaitForState(() => cut.FindAll("#prm-scope").Count == 1);
        cut.Find("#prm-scope").Change(state.UserId.ToString());
        cut.Find("#prm-load").Click();
    }

    private PageState NewState(string? SetsJson = null)
    {
        var state = new PageState();
        state.SetsJson = SetsJson ?? Sets(Set("akilli", "MobilKullanici", "MobileUser", "user", 1801, 1782));

        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(
            new HttpClient(new PageHandler(state)) { BaseAddress = new Uri("https://centralapi.test/") },
            tokenStore));

        return state;
    }

    private static string Json<T>(T value) =>
        System.Text.Json.JsonSerializer.Serialize(
            value, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

    private static ParameterSetDto Set(
        string program, string method, string scopeKind, string scopeFields, int count, int withEditor) => new()
    {
        Program = program, CatalogMethod = method, ScopeKind = scopeKind,
        ScopeFields = scopeFields, ParameterCount = count, WithEditor = withEditor,
    };

    private static string Sets(params ParameterSetDto[] sets) => Json(sets);

    private static ParameterValueDto Value(
        int id, string name, string label, string value, string def, bool overridden, bool implemented) => new()
    {
        CatalogEntryId = Guid.NewGuid(), ParametreId = id, Name = name, Label = label,
        Editor = "text", Value = value, DefaultValue = def,
        IsOverridden = overridden, IsImplemented = implemented,
    };

    private static string Values(long revision, params ParameterValueDto[] items) => Json(new ParameterValuesResponseDto
    {
        CatalogMethod = "MobilKullanici",
        Revision = revision,
        Count = items.Length,
        Items = items,
    });

    private sealed class PageState
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public Guid CompanyId { get; } = Guid.NewGuid();
        public Guid UserId { get; } = Guid.NewGuid();

        public string SetsJson { get; set; } = "[]";
        public string? ValuesJson { get; set; }
        public HttpResponseMessage? ValuesError { get; set; }
        public string? LastValuesUrl { get; set; }
    }

    private sealed class PageHandler : HttpMessageHandler
    {
        private readonly PageState _state;

        public PageHandler(PageState state) => _state = state;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;

            if (path.EndsWith("/admin/tenants", StringComparison.Ordinal))
            {
                return Ok(Json(new[]
                {
                    new TenantDto { Id = _state.TenantId, Name = "Acme", IsActive = true },
                }));
            }

            if (path.EndsWith("/admin/erp-companies/", StringComparison.Ordinal))
            {
                return Ok(Json(new[]
                {
                    new ErpCompanyDto
                    {
                        Id = _state.CompanyId, TenantId = _state.TenantId, Code = "MERKEZ",
                        Name = "Merkez", SourceDatabase = "MikroDB_V16_03", IsActive = true,
                    },
                }));
            }

            if (path.EndsWith("/mobile", StringComparison.Ordinal))
            {
                return Ok(Json(new TenantMobileOverviewDto
                {
                    TenantId = _state.TenantId,
                    Users =
                    [
                        new MobileUserDto { Id = _state.UserId, Username = "aktif", FullName = "Aktif Plasiyer", IsActive = true },
                        new MobileUserDto { Id = Guid.NewGuid(), Username = "ayrilmis", FullName = "Ayrılmış", IsActive = false },
                    ],
                }));
            }

            if (path.EndsWith("/admin/parameters/sets", StringComparison.Ordinal))
            {
                return Ok(_state.SetsJson);
            }

            if (path.EndsWith("/admin/parameters/values", StringComparison.Ordinal))
            {
                _state.LastValuesUrl = request.RequestUri?.ToString();

                if (_state.ValuesError is not null)
                {
                    return Task.FromResult(_state.ValuesError);
                }

                return Ok(_state.ValuesJson ?? Values(0));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static Task<HttpResponseMessage> Ok(string json) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
    }
}
