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

        cut.WaitForState(() => cut.FindAll(".prm-field").Count == 2);

        cut.Markup.Should().Contain("DefaultKaynakDepoNo").And.Contain("Kaynak depo no");
        cut.Markup.Should().Contain("sapmış");

        // A setting this release of the app ignores is labelled rather than silently inert (D16).
        cut.Markup.Should().Contain("etkisiz")
            .And.Contain("Sipariş Cepte bu ayarı dikkate almıyor");
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

    [Fact]
    public void The_tab_tree_is_drawn_from_the_loaded_parameters()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 0,
            Tabbed(1, "EvrakKayitKrediKontroluYap", "Evrak girişi / Cari risk takibi"),
            Tabbed(2, "MiktarGirisi", "Evrak girişi / Miktar girişi"),
            Tabbed(3, "AnaSayfa", "Görünüm ve seçenekler / Ana sayfa"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-tab__link").Count >= 4);

        // 63 tabs would be 63 Razor files if the tree were written out; it comes from the
        // catalogue's own tab paths instead (D11).
        cut.Markup.Should().Contain("Evrak girişi").And.Contain("Cari risk takibi")
            .And.Contain("Görünüm ve seçenekler");
    }

    [Fact]
    public void Choosing_a_tab_shows_that_tabs_fields()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 0,
            Tabbed(1, "KrediKontrol", "Evrak girişi / Cari risk takibi"),
            Tabbed(2, "AnaSayfa", "Görünüm ve seçenekler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-tab__link").Count >= 3);
        cut.Markup.Should().Contain("Bir sekme seçin");

        var risk = cut.FindAll(".prm-tab__link").First(l => l.TextContent.Contains("Cari risk takibi"));
        risk.Click();

        cut.WaitForState(() => cut.FindAll(".prm-field").Count == 1);
        cut.Markup.Should().Contain("KrediKontrol").And.NotContain("AnaSayfa");
    }

    [Fact]
    public void A_family_that_repeats_a_hundred_times_opens_collapsed()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 0,
            Enumerable.Range(1, 100)
                .Select(i => Tabbed(i, $"GosterZyrt_Temsilci_Ozel_{i}_Derece", "Parametreler / Ziyaret anket", "Göster"))
                .ToArray());

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-tab__link").Count >= 2);
        cut.FindAll(".prm-tab__link").First(l => l.TextContent.Contains("Ziyaret anket")).Click();

        // 830 of the 1,782 akilli fields sit on this tab; drawn as one flat list it is unusable.
        cut.WaitForState(() => cut.FindAll("details.prm-group").Count == 1);
        cut.Find("details.prm-group").HasAttribute("open").Should().BeFalse();
        cut.Find("details.prm-group > summary").TextContent.Should().Contain("100 adet");
    }

    [Fact]
    public void Editing_a_field_enables_the_save_and_sends_only_what_changed()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 0,
            Tabbed(58, "DefaultKaynakDepoNo", "Parametreler"),
            Tabbed(89, "Goster_AnaMenu_Tahsilat", "Parametreler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-field").Count == 2);
        cut.Find("#prm-save").HasAttribute("disabled").Should().BeTrue();

        cut.FindAll(".prm-field input").First().Change("3");
        cut.WaitForState(() => !cut.Find("#prm-save").HasAttribute("disabled"));
        cut.Find("#prm-save").Click();

        cut.WaitForState(() => state.LastWriteBody is not null);

        // Only the field that moved is sent: the parameter is named by its catalogue entry, and
        // the untouched one has nothing to say.
        state.LastWriteBody.Should().Contain("\"value\":\"3\"");
        state.LastWriteBody.Should().Contain($"\"erpCompanyId\":\"{state.CompanyId}\"");
        state.LastWriteBody.Should().Contain($"\"mobileUserId\":\"{state.UserId}\"");
    }

    [Fact]
    public void Typing_a_value_back_to_what_it_was_is_not_a_change()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(58, "DefaultKaynakDepoNo", "Parametreler", value: "1"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-field").Count == 1);

        var input = cut.Find(".prm-field input");
        input.Change("9");
        cut.WaitForState(() => !cut.Find("#prm-save").HasAttribute("disabled"));

        input.Change("1");

        // The server would answer "Unchanged" anyway; not sending it keeps the audit trail honest.
        cut.WaitForState(() => cut.Find("#prm-save").HasAttribute("disabled"));
        cut.Markup.Should().Contain("Bekleyen değişiklik yok");
    }

    [Fact]
    public void A_save_that_removes_a_row_says_so()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(58, "DefaultKaynakDepoNo", "Parametreler", value: "3"));
        state.WriteResponseJson = Json(new ParameterWriteResponseDto
        {
            Revision = 4,
            Results = [new ParameterWriteResultDto { CatalogEntryId = Guid.NewGuid(), Outcome = "Deleted" }],
        });

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-field").Count == 1);
        cut.Find(".prm-field input").Change("");
        cut.WaitForState(() => !cut.Find("#prm-save").HasAttribute("disabled"));
        cut.Find("#prm-save").Click();

        // Back to the default means the row is deleted, which is Fora's own behaviour and worth
        // saying out loud: "saved" and "removed" are not the same thing to an operator.
        cut.WaitForState(() => cut.FindAll(".admin-state--success").Count > 0);
        cut.Find(".admin-state--success").TextContent.Should().Contain("satır silindi");
    }

    [Fact]
    public void Searching_narrows_the_tree_to_the_tabs_that_have_hits()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 0,
            Tabbed(1, "DefaultKaynakDepoNo", "Parametreler / Tanımlamalar"),
            Tabbed(2, "Goster_AnaMenu_Tahsilat", "Evrak girişi"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-tab__link").Count >= 3);
        cut.Find("#prm-search").Input("KaynakDepo");

        // Finding the parameter but still having to guess which of 63 tabs it is on would be no
        // help, so the tree is re-derived from the matches.
        cut.WaitForState(() => cut.Markup.Contains("1 eşleşme"));
        cut.Markup.Should().Contain("Tanımlamalar").And.NotContain("Evrak girişi");
    }

    [Fact]
    public void A_search_that_matches_nothing_says_so()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(1, "DefaultKaynakDepoNo", "Parametreler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-tab__link").Count >= 1);
        cut.Find("#prm-search").Input("yokboyle");

        cut.WaitForState(() => cut.Markup.Contains("Eşleşen parametre yok"));
    }

    [Fact]
    public void Clearing_the_search_brings_the_whole_tree_back()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 0,
            Tabbed(1, "DefaultKaynakDepoNo", "Parametreler"),
            Tabbed(2, "Goster_AnaMenu_Tahsilat", "Evrak girişi"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-tab__link").Count >= 2);
        cut.Find("#prm-search").Input("KaynakDepo");
        cut.WaitForState(() => cut.FindAll("#prm-search-clear").Count == 1);

        cut.Find("#prm-search-clear").Click();

        cut.WaitForState(() => cut.FindAll(".prm-tab__link").Count == 2);
        cut.Markup.Should().Contain("Evrak girişi");
    }

    [Fact]
    public void Copying_names_the_source_the_target_and_the_set()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(58, "DefaultKaynakDepoNo", "Parametreler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll("#prm-copy-from").Count == 1);

        // The open scope's own user is excluded: copying a scope onto itself means nothing.
        cut.FindAll("#prm-copy-from option").Select(o => o.GetAttribute("value"))
            .Should().NotContain(state.UserId.ToString());

        cut.Find("#prm-copy-from").Change(state.OtherUserId.ToString());
        cut.Find("#prm-copy").Click();

        cut.WaitForState(() => state.LastCopyBody is not null);
        state.LastCopyBody.Should().Contain("catalogMethod").And.Contain($"\"toMobileUserId\":\"{state.UserId}\"");
        cut.Markup.Should().Contain("kendi ayarı kaldırıldı", "a replace is not the same as an add");
    }

    [Fact]
    public void Only_a_changed_field_can_be_picked_for_a_batch()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 0,
            Overridden(58, "DefaultKaynakDepoNo", "Parametreler"),
            Tabbed(89, "Goster_AnaMenu_Tahsilat", "Parametreler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll(".prm-field").Count == 2);

        // Selecting a parameter already at its default would put a no-op in the batch.
        cut.FindAll(".prm-field__pick").Should().ContainSingle();

        cut.Find(".prm-field__pick").Change(true);
        cut.WaitForState(() => !cut.Find("#prm-reset-picked").HasAttribute("disabled"));
        cut.Find("#prm-reset-picked").Click();

        cut.WaitForState(() => state.LastResetBody is not null);
        state.LastResetBody.Should().Contain("catalogEntryIds");
    }

    [Fact]
    public void Export_carries_only_the_deviations()
    {
        var state = NewState();
        state.ValuesJson = Values(
            revision: 0,
            Overridden(58, "DefaultKaynakDepoNo", "Parametreler", value: "3"),
            Tabbed(89, "Goster_AnaMenu_Tahsilat", "Parametreler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll("#prm-export").Count == 1);
        var href = Uri.UnescapeDataString(cut.Find("#prm-export").GetAttribute("href") ?? "");

        // A parameter at its default has no value to carry, which is why the database stores none.
        href.Should().Contain("DefaultKaynakDepoNo").And.NotContain("Goster_AnaMenu_Tahsilat");
    }

    [Fact]
    public void Import_matches_by_Mikro_id_and_reports_what_it_could_not_place()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(58, "DefaultKaynakDepoNo", "Parametreler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll("#prm-import").Count == 1);

        // 9999 is not in this set. Matching by the catalogue entry's own id would match nothing at
        // all, because that id belongs to the installation the file came from.
        cut.Find("#prm-import").Change(
            "{\"values\":[{\"parametreId\":58,\"value\":\"3\"},{\"parametreId\":9999,\"value\":\"x\"}]}");
        cut.Find("#prm-import-apply").Click();

        cut.WaitForState(() => state.LastWriteBody is not null);
        state.LastWriteBody.Should().Contain("\"value\":\"3\"");
        cut.Markup.Should().Contain("1 satır bu kümede tanınmadığı için atlandı");
    }

    [Fact]
    public void Import_of_something_that_is_not_json_says_so_rather_than_failing_silently()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(58, "DefaultKaynakDepoNo", "Parametreler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll("#prm-import").Count == 1);
        cut.Find("#prm-import").Change("bu json degil");
        cut.Find("#prm-import-apply").Click();

        cut.WaitForState(() => cut.FindAll(".admin-state--error").Count > 0);
        cut.Find(".admin-state--error").TextContent.Should().Contain("geçerli JSON değil");
    }

    [Fact]
    public void History_is_fetched_only_when_it_is_opened()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(58, "DefaultKaynakDepoNo", "Parametreler"));
        state.AuditJson = Json(new[]
        {
            new ParameterAuditDto
            {
                Name = "DefaultKaynakDepoNo", Outcome = "Updated", OldValue = "1", NewValue = "3",
                Source = "panel", Actor = "gurbuz", AtUtc = DateTimeOffset.Parse("2026-09-18T07:30:00Z"),
            },
        });

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll("#prm-history-open").Count == 1);

        // A second query on a screen that already pulls 1,801 parameters, and most visits never
        // open it.
        state.LastAuditUrl.Should().BeNull();

        cut.Find("#prm-history-open").ParentElement!.TriggerEvent("ontoggle", EventArgs.Empty);

        cut.WaitForState(() => state.LastAuditUrl is not null);
        cut.WaitForState(() => cut.Markup.Contains("gurbuz"));
        cut.Markup.Should().Contain("güncellendi").And.Contain("18 Eyl 2026");
    }

    [Fact]
    public void History_asks_for_the_open_scope_only()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(58, "DefaultKaynakDepoNo", "Parametreler"));

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll("#prm-history-open").Count == 1);
        cut.Find("#prm-history-open").ParentElement!.TriggerEvent("ontoggle", EventArgs.Empty);

        cut.WaitForState(() => state.LastAuditUrl is not null);
        state.LastAuditUrl.Should().Contain("scoped=true")
            .And.Contain($"mobileUserId={state.UserId}")
            .And.Contain("catalogMethod=MobilKullanici");
    }

    [Fact]
    public void A_value_that_was_removed_reads_as_back_to_default()
    {
        var state = NewState();
        state.ValuesJson = Values(revision: 0, Tabbed(58, "DefaultKaynakDepoNo", "Parametreler"));
        state.AuditJson = Json(new[]
        {
            new ParameterAuditDto
            {
                Name = "DefaultKaynakDepoNo", Outcome = "Deleted", OldValue = "3", NewValue = null,
                Source = "reset", Actor = "gurbuz", AtUtc = DateTimeOffset.UtcNow,
            },
        });

        var cut = Render<Parameters>();
        LoadAkilliAsync(cut, state);

        cut.WaitForState(() => cut.FindAll("#prm-history-open").Count == 1);
        cut.Find("#prm-history-open").ParentElement!.TriggerEvent("ontoggle", EventArgs.Empty);

        // A null new value is not "empty": the row is gone and the catalogue default applies.
        cut.WaitForState(() => cut.Markup.Contains("(varsayılan)"));
        cut.Markup.Should().Contain("varsayılana döndü");
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

    private static ParameterValueDto Tabbed(
        int id, string name, string tabPath, string? label = null, string value = "") => new()
    {
        CatalogEntryId = Guid.NewGuid(), ParametreId = id, Name = name, Label = label ?? name,
        Editor = "text", Value = value, DefaultValue = "", TabPath = tabPath, IsImplemented = true,
    };

    private static ParameterValueDto Overridden(
        int id, string name, string tabPath, string value = "3") => new()
    {
        CatalogEntryId = Guid.NewGuid(), ParametreId = id, Name = name, Label = name,
        Editor = "text", Value = value, DefaultValue = "1", TabPath = tabPath,
        IsOverridden = true, IsImplemented = true,
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
        public Guid OtherUserId { get; } = Guid.NewGuid();

        public string SetsJson { get; set; } = "[]";
        public string? ValuesJson { get; set; }
        public HttpResponseMessage? ValuesError { get; set; }
        public string? LastValuesUrl { get; set; }
        public string? LastWriteBody { get; set; }
        public string? WriteResponseJson { get; set; }
        public string? LastCopyBody { get; set; }
        public string? LastResetBody { get; set; }
        public string? LastAuditUrl { get; set; }
        public string? AuditJson { get; set; }
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
                        new MobileUserDto { Id = _state.OtherUserId, Username = "ikinci", FullName = "İkinci Plasiyer", IsActive = true },
                        new MobileUserDto { Id = Guid.NewGuid(), Username = "ayrilmis", FullName = "Ayrılmış", IsActive = false },
                    ],
                }));
            }

            if (path.EndsWith("/admin/parameters/sets", StringComparison.Ordinal))
            {
                return Ok(_state.SetsJson);
            }

            if (path.EndsWith("/admin/parameters/audit", StringComparison.Ordinal))
            {
                _state.LastAuditUrl = request.RequestUri?.ToString();
                return Ok(_state.AuditJson ?? "[]");
            }

            if (path.EndsWith("/admin/parameters/values/copy", StringComparison.Ordinal))
            {
                _state.LastCopyBody = request.Content?.ReadAsStringAsync(cancellationToken).Result;
                return Ok(Json(new ParameterCopyResponseDto { Copied = 2, Written = 2, Cleared = 1, Revision = 5 }));
            }

            if (path.EndsWith("/admin/parameters/values/reset", StringComparison.Ordinal))
            {
                _state.LastResetBody = request.Content?.ReadAsStringAsync(cancellationToken).Result;
                return Ok(Json(new ParameterWriteResponseDto { Revision = 6 }));
            }

            if (path.EndsWith("/admin/parameters/values", StringComparison.Ordinal))
            {
                if (request.Method == HttpMethod.Put)
                {
                    _state.LastWriteBody = request.Content?.ReadAsStringAsync(cancellationToken).Result;
                    return Ok(_state.WriteResponseJson ?? Json(new ParameterWriteResponseDto { Revision = 1 }));
                }

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
