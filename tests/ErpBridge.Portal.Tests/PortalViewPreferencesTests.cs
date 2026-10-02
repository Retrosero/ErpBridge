using System.Text.Json.Nodes;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// Görünüm ayarları panelde (Siparis_Cepte KB kural 59, ErpBridge KB kural 35): bir kişinin ayarları ve kilitleri rollerinin
/// varsayılanları üzerinde düzenlenir, bilinmeyen alanlar korunur, başka kişilere kopyalanır; rol şablonları ayrı sayfada.
/// </summary>
public sealed class PortalViewPreferencesTests : PortalPageTestContext
{
    private static readonly Guid AliId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid VeliId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static string PrefsPath => $"/api/v1/android/account/users/{AliId}/preferences";

    private static object UserList() => new
    {
        seats = new { max = 5, used = 3, status = "active" },
        users = new object[]
        {
            new { id = Guid.NewGuid(), username = "patron", fullName = "Firma Sahibi", role = "ADMIN", roles = new[] { "ADMIN" }, isActive = true },
            new { id = AliId, username = "ali", fullName = "Ali Yılmaz", role = "SALES", roles = new[] { "SALES" }, isActive = true },
            new { id = VeliId, username = "veli", fullName = "Veli Demir", role = "SALES", roles = new[] { "SALES" }, isActive = true },
        },
    };

    private static object AliPrefs(object? data, object? locks = null, object? roleData = null, object? roleLocks = null) => new
    {
        userId = AliId,
        username = "ali",
        fullName = "Ali Yılmaz",
        roles = new[] { "SALES" },
        version = 3,
        updatedAtUtc = PortalTestSetup.Now,
        updatedByName = "Ali Yılmaz",
        updatedByClient = "android",
        data,
        locks = locks ?? new { },
        roleBase = new { data = roleData ?? new { }, locks = roleLocks ?? new { }, stamp = "SALES:1|u:0" },
    };

    private IRenderedComponent<Kullanicilar> OpenAli(FakeCentralApi api, object prefs)
    {
        api.Answer("/api/v1/android/account/users", UserList());
        api.Answer(PrefsPath, prefs);
        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=ali] .view-prefs-btn"));
        cut.Find("tr[data-user=ali] .view-prefs-btn").Click();
        cut.WaitForAssertion(() => cut.Find("#user-view-prefs-editor"));
        return cut;
    }

    private static JsonNode PutBody(FakeCentralApi api, string path) =>
        JsonNode.Parse(api.Requests.Single(r => r.Method == HttpMethod.Put && r.PathAndQuery == path).Body!)!;

    [Fact]
    public void The_embedded_catalog_is_the_phones()
    {
        var catalog = ViewSettingsCatalogFile.Load();
        catalog.Pages.Select(p => p.Key).Should().Contain(["home", "sales", "catalog", "collection", "targets"]);
        catalog.Settings.Should().Contain(s => s.Path == "sales.viewMode" && s.Type == "choice");
        catalog.Pages.Where(p => p.Route is not null).Should().OnlyContain(p => p.Sections[0].Settings[0].Type == "member");
    }

    [Fact]
    public void A_persons_sheet_saves_the_change_and_keeps_what_the_panel_does_not_know()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var cut = OpenAli(api, AliPrefs(new { sales = new { viewMode = "List" }, gelecek = new { x = 1 } }));

        cut.Find("#view-prefs-meta").TextContent.Should().Contain("Ali Yılmaz").And.Contain("(telefon)");
        cut.Find("#user-view-prefs-editor .vp-page[data-page=sales]").Click();
        cut.Find("#user-view-prefs-editor [data-key='sales.viewMode'] [data-source]").TextContent.Should().Be("Kişisel");
        cut.Find("#user-view-prefs-editor [data-key='sales.fields.brand'] [data-choice=off]").Click();
        cut.Find("#view-prefs-save").TextContent.Should().Contain("(1)");
        cut.Find("#view-prefs-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Ali Yılmaz için görünüm ayarları kaydedildi"));
        var body = PutBody(api, PrefsPath);
        body["data"]!["sales"]!["fields"]!["brand"]!.GetValue<bool>().Should().BeFalse();
        body["data"]!["sales"]!["viewMode"]!.GetValue<string>().Should().Be("List");
        body["data"]!["gelecek"]!["x"]!.GetValue<int>().Should().Be(1);
        body["expectedVersion"]!.GetValue<long>().Should().Be(3);
    }

    [Fact]
    public void Turning_a_module_off_keeps_the_rest_of_the_inherited_list()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var cut = OpenAli(api, AliPrefs(null, roleData: new Dictionary<string, object> { ["home.visibleModules#tasks"] = false }));

        cut.Find("#user-view-prefs-editor .vp-page[data-page=reports]").Click();
        cut.Find("#user-view-prefs-editor [data-key='home.visibleModules#reports'] [data-choice=off]").Click();
        cut.Find("#view-prefs-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice"));
        var modules = PutBody(api, PrefsPath)["data"]!["home"]!["visibleModules"]!.AsArray().Select(x => x!.GetValue<string>()).ToList();
        modules.Should().Contain("sales").And.NotContain("reports").And.NotContain("tasks", "the role turned it off");
    }

    [Fact]
    public void A_lock_keeps_the_shown_value_and_a_role_lock_is_read_only_here()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var cut = OpenAli(api, AliPrefs(null,
            roleData: new Dictionary<string, object> { ["catalog.viewMode"] = "List" },
            roleLocks: new Dictionary<string, object> { ["home.showRouteCard"] = false }));

        cut.Find("#user-view-prefs-editor [data-key='home.showRouteCard'] [data-source]").TextContent.Should().Be("Rol kilidi");
        cut.Find("#user-view-prefs-editor [data-key='home.showRouteCard'] [data-choice=on]").HasAttribute("disabled").Should().BeTrue();

        cut.Find("#user-view-prefs-editor .vp-page[data-page=catalog]").Click();
        cut.Find("#user-view-prefs-editor [data-key='catalog.viewMode'] [data-source]").TextContent.Should().Be("Rolden");
        cut.Find("#user-view-prefs-editor [data-key='catalog.viewMode'] .vp-lock").Click();
        cut.Find("#user-view-prefs-editor [data-key='catalog.viewMode'] [data-source]").TextContent.Should().Be("Kişiye kilitli");
        cut.Find("#view-prefs-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice"));
        var body = PutBody(api, PrefsPath);
        body["locks"]!["catalog.viewMode"]!.GetValue<string>().Should().Be("List");
        body["data"]!.AsObject().Count.Should().Be(0, "locking does not write the person's own value");
    }

    [Fact]
    public void Copying_sends_the_chosen_people_and_whether_locks_go_too()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var cut = OpenAli(api, AliPrefs(new { home = new { showTargetCard = false } }));
        api.Answer($"{PrefsPath}/copy", new { copied = 1 });

        cut.Find("#view-prefs-copy-open").Click();
        cut.Find("#view-prefs-copy [data-user=veli] input").Change(true);
        cut.FindAll("#view-prefs-copy [data-user=ali]").Should().BeEmpty("nobody copies onto themselves");
        cut.Find("#view-prefs-copy-locks").Change(true);
        cut.Find("#view-prefs-copy-run").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("1 kişiye kopyalandı"));
        var post = JsonNode.Parse(api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == $"{PrefsPath}/copy").Body!)!;
        post["targetUserIds"]!.AsArray().Select(x => x!.GetValue<Guid>()).Should().Equal(VeliId);
        post["includeLocks"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void The_role_templates_page_saves_flat_values_and_locks_for_the_chosen_role()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        object Role(string role, long version) => new { role, data = new { }, locks = new { }, version };
        api.Answer("/api/v1/android/account/roles/view-preferences", new[] { Role("ADMIN", 0), Role("MANAGER", 0), Role("ACCOUNTING", 0), Role("WAREHOUSE", 0), Role("SALES", 2) });
        api.Answer("/api/v1/android/account/roles/SALES/view-preferences", new { role = "SALES", data = new { }, locks = new { }, version = 3 });

        var cut = Render<GorunumSablonlari>();
        cut.WaitForAssertion(() => cut.Find("#role-view-prefs-editor"));
        cut.Find("[data-role=SALES]").ClassList.Should().Contain("is-active");
        cut.Find("#role-view-prefs-editor [data-key='home.showTargetCard'] [data-choice=off]").Click();
        cut.Find("#role-view-prefs-editor [data-key='home.showRouteCard'] .vp-lock").Click();
        cut.Find("#view-roles-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Saha rolünün görünüm varsayılanları kaydedildi"));
        var body = PutBody(api, "/api/v1/android/account/roles/SALES/view-preferences");
        body["data"]!["home.showTargetCard"]!.GetValue<bool>().Should().BeFalse();
        body["locks"]!["home.showRouteCard"]!.GetValue<bool>().Should().BeTrue();
        body["expectedVersion"]!.GetValue<long>().Should().Be(2);
    }

    [Fact]
    public void A_manager_cannot_open_the_role_templates()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER"));
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("gorunum-sablonlari");

        var cut = Render<GorunumSablonlari>();

        cut.WaitForAssertion(() => nav.Uri.Should().Be(nav.BaseUri));
        cut.FindAll("#role-view-prefs-editor").Should().BeEmpty();
    }

    [Fact]
    public void Resetting_a_page_removes_the_persons_values_and_sets_members_back()
    {
        var catalog = ViewSettingsCatalogFile.Load();
        var draft = new ViewPrefsDraft(catalog, ViewPrefsMode.User,
            JsonNode.Parse("""{"sales":{"viewMode":"Grid","fields":{"brand":false}},"home":{"visibleModules":["catalog"]},"gelecek":1}""")!.AsObject(),
            new JsonObject());

        draft.ResetPage(catalog.Pages.Single(p => p.Key == "sales"));

        draft.Values["sales"]!.AsObject().ContainsKey("viewMode").Should().BeFalse();
        draft.Values["sales"]!["fields"]!.AsObject().ContainsKey("brand").Should().BeFalse();
        draft.Values["home"]!["visibleModules"]!.AsArray().Select(x => x!.GetValue<string>()).Should().Contain(["catalog", "sales"]);
        draft.Values["gelecek"]!.GetValue<int>().Should().Be(1);
        draft.Changes.Should().BeGreaterThan(0);
    }
}
