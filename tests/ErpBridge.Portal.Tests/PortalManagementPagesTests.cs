using System.Net;
using Bunit;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>The pages that change something: approvals and users.</summary>
public sealed class PortalManagementPagesTests : PortalPageTestContext
{
    private static readonly Guid SaleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ReturnId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static object Request(Guid id, string kind, string counterparty, decimal amount) => new
    {
        id, kind, counterpartyName = counterparty, amount, status = "Pending",
        requestedByName = "Ali Yılmaz", requestedAtUtc = PortalTestSetup.Now,
        summary = new { },
    };

    private const string Pending = "/api/v1/android/approvals?status=pending&take=50";

    private static object PendingBoth() => new[] { Request(SaleId, "sale", "Bakkal Veli", 12450.5m), Request(ReturnId, "return", "Market Can", 300m) };

    [Fact]
    public void Approving_sends_the_note_and_takes_the_request_off_the_list()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Pending, PendingBoth());
        api.Answer($"/api/v1/android/approvals/{SaleId}/approve", Request(SaleId, "sale", "Bakkal Veli", 12450.5m));

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(2));

        cut.Find($"[data-request='{SaleId}'] .portal-note input").Change(" Fiyat uygun ");
        cut.Find($"[data-request='{SaleId}'] .approve-btn").Click();

        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().ContainSingle());
        cut.Find("#page-notice").TextContent.Trim().Should().Be("Bakkal Veli — satış talebi onaylandı.");
        var decision = api.Requests.Single(r => r.PathAndQuery.EndsWith("/approve"));
        decision.Method.Should().Be(HttpMethod.Post);
        decision.Body.Should().Contain("\"note\":\"Fiyat uygun\"");
    }

    [Fact]
    public void A_request_someone_else_just_decided_refreshes_the_list_and_says_so()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Pending, PendingBoth());
        api.Fail($"/api/v1/android/approvals/{ReturnId}/reject", HttpStatusCode.Conflict, "APPROVAL_ALREADY_DECIDED");

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(2));
        api.Answer(Pending, new[] { Request(SaleId, "sale", "Bakkal Veli", 12450.5m) });

        cut.Find($"[data-request='{ReturnId}'] .reject-btn").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("başka bir yetkili"));
        cut.FindAll("[data-request]").Should().ContainSingle();
    }

    [Fact]
    public void Refusing_ones_own_request_keeps_it_and_explains()
    {
        var (api, session, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Pending, PendingBoth());
        api.Fail($"/api/v1/android/approvals/{SaleId}/approve", HttpStatusCode.Forbidden, "SELF_APPROVAL_NOT_ALLOWED");

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(2));
        cut.Find($"[data-request='{SaleId}'] .approve-btn").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("başka bir onay yetkilisi"));
        cut.FindAll("[data-request]").Should().HaveCount(2);
        session.IsSignedIn.Should().BeTrue();
    }

    [Fact]
    public void A_manager_without_the_right_to_approve_sees_requests_but_no_buttons()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER") with { CanApprove = false });
        api.Answer(Pending, PendingBoth());

        var cut = Render<Onaylar>();

        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(2));
        cut.Find("#approvals-readonly");
        cut.FindAll("[data-request] .approve-btn, [data-request] .reject-btn").Should().BeEmpty();
    }

    private static object Decided(Guid id, string status, string decidedBy, string? note, long seq = 7) => new
    {
        id, externalId = $"APR-{seq}", kind = "sale", counterpartyName = "Bakkal Veli", amount = 900m, status,
        requestedByName = "Ali Yılmaz", requestedAtUtc = PortalTestSetup.Now.AddHours(-3), requestedSeq = seq,
        decidedByName = decidedBy, decidedAtUtc = PortalTestSetup.Now.AddHours(-1), decisionNote = note,
        summary = new { },
    };

    private static object SaleDetail(Guid id, string status = "Pending") => new
    {
        request = new
        {
            id, kind = "sale", counterpartyName = "Bakkal Veli", amount = 750m, status,
            requestedByName = "Ali Yılmaz", requestedAtUtc = PortalTestSetup.Now, requestedSeq = 12, summary = new { },
        },
        documents = new object[]
        {
            new
            {
                documentType = "sales_order", externalId = "MOB-SO-1",
                payload = new
                {
                    mobileDocumentId = "MOB-SO-1", customerCode = "C-001", counterparty = "Bakkal Veli", paymentType = "Cari Borç", amount = 750,
                    campaign = "Eylül",
                    lines = new object[] { new { productCode = "CAY-1", productTitle = "Çay 1 kg", quantity = 5, unitPrice = 150, lineTotal = 750 } },
                },
            },
        },
        events = new object[] { new { action = "Submitted", byName = "Ali Yılmaz", atUtc = PortalTestSetup.Now, note = (string?)null } },
        warnings = new object[] { new { stockCode = "CAY-1", title = "Çay 1 kg", requested = 5m, onHand = 2m } },
    };

    [Fact]
    public void The_approved_tab_lists_decisions_with_who_decided_and_no_buttons()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Pending, PendingBoth());
        api.Answer("/api/v1/android/approvals?status=approved&take=50", new[] { Decided(SaleId, "Approved", "Patron", "Fiyat uygun") });
        api.Answer("/api/v1/android/approvals/summary", new { pendingCount = 2 });

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(2));
        cut.Find("#approvals-pending-count").TextContent.Should().Be("2");

        cut.Find("[data-tab='onaylanan']").Click();

        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().ContainSingle());
        var card = cut.Find($"[data-request='{SaleId}']");
        card.QuerySelector(".approval-status")!.TextContent.Should().Be("Onaylandı");
        card.QuerySelector(".approval-decision")!.TextContent.Should().Contain("Patron").And.Contain("Fiyat uygun");
        card.QuerySelectorAll(".approve-btn, .reject-btn").Should().BeEmpty();
        cut.Find("[data-tab='onaylanan']").ClassList.Should().Contain("is-active");
        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain("durum=onaylanan");
    }

    [Fact]
    public void The_rejected_tab_and_the_kind_filter_ask_the_server_and_more_pages_follow_the_last_request()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Pending, PendingBoth());
        api.Answer("/api/v1/android/approvals/summary", new { pendingCount = 2 });
        var firstPage = Enumerable.Range(0, 50).Select(i => Decided(Guid.NewGuid(), "Rejected", "Patron", null, seq: 100 - i)).ToArray();
        api.Answer("/api/v1/android/approvals?status=rejected%2Cresubmitted&take=50", firstPage);
        api.Answer("/api/v1/android/approvals?status=rejected%2Cresubmitted&kind=collection&take=50", firstPage);
        api.Answer("/api/v1/android/approvals?status=rejected%2Cresubmitted&kind=collection&beforeSeq=51&beforeExternalId=APR-51&take=50", new[] { Decided(SaleId, "Rejected", "Patron", "Eksik") });

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(2));
        cut.Find("[data-tab='reddedilen']").Click();
        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(50));

        cut.Find("#approvals-kind").Change("collection");
        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.PathAndQuery.Contains("kind=collection&take=50")));
        cut.WaitForAssertion(() => cut.Find("#approvals-more"));

        cut.Find("#approvals-more").Click();

        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(51));
        cut.FindAll("#approvals-more").Should().BeEmpty();
    }

    [Fact]
    public void Clicking_a_request_opens_its_lines_warnings_and_history_and_it_can_be_approved_there()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Pending, PendingBoth());
        api.Answer($"/api/v1/android/approvals/{SaleId}", SaleDetail(SaleId));
        api.Answer($"/api/v1/android/approvals/{SaleId}/approve", Decided(SaleId, "Approved", "Firma Sahibi", null));

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(2));

        cut.Find($"[data-request='{SaleId}'] .approval-open").Click();

        cut.WaitForAssertion(() => cut.Find("#approval-detail [data-line='CAY-1']"));
        var sheet = cut.Find("#approval-detail");
        sheet.QuerySelector(".sheet-title")!.TextContent.Should().Be("Bakkal Veli");
        sheet.QuerySelector("#detail-status")!.TextContent.Should().Be("Bekliyor");
        sheet.QuerySelector("[data-line='CAY-1']")!.TextContent.Should().Contain("Çay 1 kg").And.Contain("750,00 TL");
        sheet.QuerySelector("#detail-warnings")!.TextContent.Should().Contain("stokta 2");
        sheet.QuerySelector("#detail-events")!.TextContent.Should().Contain("Gönderildi");
        sheet.TextContent.Should().Contain("Cari Borç").And.Contain("campaign");
        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain($"talep={SaleId}");

        cut.Find("#detail-actions .approve-btn").Click();

        cut.WaitForAssertion(() => cut.FindAll("#approval-detail").Should().BeEmpty());
        cut.FindAll("[data-request]").Should().ContainSingle();
        cut.Find("#page-notice").TextContent.Should().Contain("onaylandı");
        Services.GetRequiredService<NavigationManager>().Uri.Should().NotContain("talep=");
    }

    [Fact]
    public void A_slow_detail_of_a_request_the_user_left_does_not_replace_the_one_on_screen()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Pending, PendingBoth());
        api.Answer($"/api/v1/android/approvals/{SaleId}", SaleDetail(SaleId));
        var slowSale = api.Hold($"/api/v1/android/approvals/{SaleId}");
        var returnDetail = new
        {
            request = new { id = ReturnId, externalId = "APR-R", kind = "return", counterpartyName = "Market Can", amount = 300m, status = "Pending", requestedAtUtc = PortalTestSetup.Now, summary = new { } },
            documents = Array.Empty<object>(), events = Array.Empty<object>(), warnings = Array.Empty<object>(),
        };
        api.Answer($"/api/v1/android/approvals/{ReturnId}", returnDetail);

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("[data-request]").Should().HaveCount(2));
        cut.Find($"[data-request='{SaleId}'] .approval-open").Click();
        cut.Find("#approval-detail .sheet-close").Click();
        cut.Find($"[data-request='{ReturnId}'] .approval-open").Click();
        cut.WaitForAssertion(() => cut.Find("#approval-detail #detail-amount").TextContent.Should().Be("300,00 TL"));

        slowSale.SetResult();

        cut.WaitForAssertion(() => api.Requests.Count(r => r.PathAndQuery == $"/api/v1/android/approvals/{SaleId}").Should().Be(1));
        Thread.Sleep(200); // let the released answer finish; nothing on screen may change
        cut.Find("#approval-detail .sheet-title").TextContent.Should().Be("Market Can");
        cut.Find("#approval-detail #detail-amount").TextContent.Should().Be("300,00 TL");
        cut.FindAll("#approval-detail [data-line]").Should().BeEmpty();
    }

    [Fact]
    public void A_link_with_a_request_opens_its_detail_and_a_decided_request_has_no_buttons()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/approvals?status=all&take=50", new[] { Decided(SaleId, "Approved", "Patron", null) });
        api.Answer($"/api/v1/android/approvals/{SaleId}", SaleDetail(SaleId, status: "Approved"));
        api.Answer("/api/v1/android/approvals/summary", new { pendingCount = 0 });
        Services.GetRequiredService<NavigationManager>().NavigateTo($"onaylar?durum=tumu&talep={SaleId}");

        var cut = Render<Onaylar>();

        cut.WaitForAssertion(() => cut.Find("#approval-detail #detail-status").TextContent.Should().Be("Onaylandı"));
        cut.FindAll("#detail-actions").Should().BeEmpty();
        cut.Find("[data-tab='tumu']").ClassList.Should().Contain("is-active");

        cut.Find("#approval-detail .sheet-close").Click();
        cut.WaitForAssertion(() => cut.FindAll("#approval-detail").Should().BeEmpty());
    }

    // ---- users ------------------------------------------------------------------------

    private static object UserList(bool aliActive = true) => new
    {
        seats = new { max = 5, used = 3, status = "active" },
        users = new object[]
        {
            new { id = Guid.NewGuid(), username = "patron", fullName = "Firma Sahibi", role = "ADMIN", canApprove = true, isActive = true },
            new { id = Guid.Parse("33333333-3333-3333-3333-333333333333"), username = "ali", fullName = "Ali Yılmaz", role = "SALES", canApprove = false, isActive = aliActive },
        },
    };

    [Fact]
    public void A_manager_cannot_open_the_users_page()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER"));
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("kullanicilar");

        var cut = Render<Kullanicilar>();

        cut.WaitForAssertion(() => nav.Uri.Should().Be(nav.BaseUri));
        cut.FindAll("#users-table").Should().BeEmpty();
    }

    [Fact]
    public void An_administrator_sees_seats_and_cannot_disable_themselves()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/account/users", UserList());

        var cut = Render<Kullanicilar>();

        cut.WaitForAssertion(() => cut.Find("#seats").TextContent.Should().Be("3 / 5 kullanıcı hakkı"));
        cut.Find("tr[data-user=patron]").QuerySelector("button").Should().BeNull("an administrator changes neither their own state nor their own roles");
        cut.Find("tr[data-user=ali] .active-btn").TextContent.Trim().Should().Be("Devre dışı bırak");
    }

    [Fact]
    public void Disabling_a_user_patches_them_and_shows_the_new_state()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var aliId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        api.Answer("/api/v1/android/account/users", UserList());
        api.Answer($"/api/v1/android/account/users/{aliId}", new { id = aliId, username = "ali", fullName = "Ali Yılmaz", role = "SALES", isActive = false });

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=ali] .active-btn"));
        api.Answer("/api/v1/android/account/users", UserList(aliActive: false));

        cut.Find("tr[data-user=ali] .active-btn").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Trim().Should().Be("Ali Yılmaz devre dışı bırakıldı."));
        cut.Find("tr[data-user=ali] .badge--off").TextContent.Should().Be("Pasif");
        api.Requests.Single(r => r.Method == HttpMethod.Patch).Body.Should().Contain("\"isActive\":false");
    }

    [Fact]
    public void Creating_a_manager_sends_the_approval_right_and_a_lowercase_username()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/account/users", UserList());

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("#user-create"));
        api.Answer("/api/v1/android/account/users", new { id = Guid.NewGuid(), username = "sef", fullName = "Satış Şefi", role = "MANAGER", roles = new[] { "MANAGER", "WAREHOUSE" }, canApprove = true, isActive = true });

        cut.Find("#new-fullname").Change(" Satış Şefi ");
        cut.Find("#new-username").Change(" Sef ");
        cut.Find("#new-password").Change("parola123");
        cut.Find("#new-roles .mud-chip[data-role=SALES]").Click();
        cut.Find("#new-roles .mud-chip[data-role=WAREHOUSE]").Click();
        cut.Find("#new-roles .mud-chip[data-role=MANAGER]").Click();
        cut.Find("#new-can-approve input").Change(true);
        cut.Find("#user-create form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Trim().Should().Be("Satış Şefi eklendi. Telefonda ve panelde firma kodu, kullanıcı adı ve parolasıyla giriş yapabilir."));
        var body = api.Requests.Single(r => r.Method == HttpMethod.Post).Body!;
        body.Should().Contain("\"username\":\"sef\"").And.Contain("\"roles\":[\"MANAGER\",\"WAREHOUSE\"]").And.Contain("\"canApprove\":true");
        body.Should().NotContain("\"role\":", "the single-role field would replace only the field role on the server");
        cut.Find("#new-username").GetAttribute("value").Should().BeNullOrEmpty();
        cut.FindAll("#new-roles .mud-chip[data-role=SALES].mud-chip-selected").Should().ContainSingle("the form starts over with a field user");
    }

    [Fact]
    public void A_full_seat_count_is_explained_and_the_form_is_kept()
    {
        var (api, session, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/account/users", UserList());

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("#user-create"));
        // The same path answers both the list (GET) and the create (POST); make the create fail.
        api.Fail("/api/v1/android/account/users", HttpStatusCode.Conflict, "SEAT_LIMIT_REACHED");

        cut.Find("#new-fullname").Change("Yeni Kişi");
        cut.Find("#new-username").Change("yeni");
        cut.Find("#new-password").Change("parola123");
        cut.Find("#user-create form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("kullanıcı hakları dolu"));
        cut.Find("#new-username").GetAttribute("value").Should().Be("yeni");
        session.IsSignedIn.Should().BeTrue();
    }

    [Fact]
    public void A_user_without_any_role_is_not_sent_to_the_server()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/account/users", UserList());

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("#user-create"));
        cut.Find("#new-fullname").Change("Rolsüz");
        cut.Find("#new-username").Change("rolsuz");
        cut.Find("#new-password").Change("parola123");
        cut.Find("#new-roles .mud-chip[data-role=SALES]").Click();

        cut.Find("#new-roles .role-picker-warning");
        cut.Find("#user-create form").Submit();

        cut.Find("#page-error").TextContent.Should().Contain("En az bir rol");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post);
    }

    [Fact]
    public void Giving_a_field_user_the_warehouse_role_keeps_their_field_role_and_says_what_they_now_have()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var aliId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        api.Answer("/api/v1/android/account/users", UserList());
        api.Answer($"/api/v1/android/account/users/{aliId}", new { id = aliId, username = "ali", fullName = "Ali Yılmaz", role = "SALES", roles = new[] { "WAREHOUSE", "SALES" }, isActive = true });

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=ali] .roles-btn"));
        cut.FindAll("#user-roles-edit").Should().BeEmpty();

        cut.Find("tr[data-user=ali] .roles-btn").Click();
        cut.Find("#user-roles-edit").TextContent.Should().Contain("Ali Yılmaz");
        cut.FindAll("#edit-roles .mud-chip-selected").Select(c => c.GetAttribute("data-role")).Should().Equal("SALES");
        cut.Find("#edit-roles .mud-chip[data-role=WAREHOUSE]").Click();
        cut.Find("#user-roles-edit form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Trim().Should().Be("Ali Yılmaz için roller kaydedildi: Depo · Saha."));
        var patch = api.Requests.Single(r => r.Method == HttpMethod.Patch);
        patch.PathAndQuery.Should().EndWith(aliId.ToString());
        patch.Body.Should().Contain("\"roles\":[\"WAREHOUSE\",\"SALES\"]").And.Contain("\"canApprove\":null");
        cut.FindAll("#user-roles-edit").Should().BeEmpty();
    }

    [Fact]
    public void Saving_the_roles_of_an_accounting_manager_never_grants_the_managers_approval_right()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var elifId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        api.Answer("/api/v1/android/account/users", new
        {
            seats = new { max = 5, used = 2, status = "active" },
            // canApprove is the effective right: accounting approves money documents without the manager flag.
            users = new object[] { new { id = elifId, username = "elif", fullName = "Elif Şef", role = "MANAGER", roles = new[] { "MANAGER", "ACCOUNTING" }, canApprove = true, isActive = true } },
        });
        api.Answer($"/api/v1/android/account/users/{elifId}", new { id = elifId, username = "elif", fullName = "Elif Şef", role = "MANAGER", roles = new[] { "MANAGER", "ACCOUNTING" }, canApprove = true, isActive = true });

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=elif] .roles-btn"));
        cut.Find("tr[data-user=elif]").TextContent.Should().NotContain("onaylayabilir", "the list cannot tell whether the manager flag is set");

        cut.Find("tr[data-user=elif] .roles-btn").Click();
        ((AngleSharp.Html.Dom.IHtmlInputElement)cut.Find("#edit-can-approve input")).IsChecked.Should().BeFalse();
        cut.Find("#edit-can-approve-kept");
        cut.Find("#user-roles-edit form").Submit();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Patch));
        api.Requests.Single(r => r.Method == HttpMethod.Patch).Body.Should().Contain("\"canApprove\":null");
    }

    [Fact]
    public void Turning_the_approval_switch_on_sends_it()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var elifId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        api.Answer("/api/v1/android/account/users", new
        {
            seats = new { max = 5, used = 2, status = "active" },
            users = new object[] { new { id = elifId, username = "elif", fullName = "Elif Şef", role = "MANAGER", roles = new[] { "MANAGER", "ACCOUNTING" }, canApprove = true, isActive = true } },
        });
        api.Answer($"/api/v1/android/account/users/{elifId}", new { id = elifId, username = "elif", fullName = "Elif Şef", role = "MANAGER", roles = new[] { "MANAGER", "ACCOUNTING" }, canApprove = true, isActive = true });

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=elif] .roles-btn"));
        cut.Find("tr[data-user=elif] .roles-btn").Click();
        cut.Find("#edit-can-approve input").Change(true);
        cut.FindAll("#edit-can-approve-kept").Should().BeEmpty();
        cut.Find("#user-roles-edit form").Submit();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Patch));
        api.Requests.Single(r => r.Method == HttpMethod.Patch).Body.Should().Contain("\"canApprove\":true");
    }

    [Fact]
    public void A_manager_only_user_starts_from_their_own_approval_flag()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var sefId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        api.Answer("/api/v1/android/account/users", new
        {
            seats = new { max = 5, used = 2, status = "active" },
            users = new object[] { new { id = sefId, username = "sef", fullName = "Satış Şefi", role = "MANAGER", roles = new[] { "MANAGER" }, canApprove = true, isActive = true } },
        });

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=sef] .roles-btn"));
        cut.Find("tr[data-user=sef]").TextContent.Should().Contain("onaylayabilir");
        cut.Find("tr[data-user=sef] .roles-btn").Click();

        ((AngleSharp.Html.Dom.IHtmlInputElement)cut.Find("#edit-can-approve input")).IsChecked.Should().BeTrue();
        cut.FindAll("#edit-can-approve-kept").Should().BeEmpty();
    }

    // ---- roles changed while signed in ------------------------------------------------------

    [Fact]
    public void A_restored_session_reads_the_roles_again_and_follows_an_administrators_change()
    {
        var saved = PortalTestSetup.State(role: "SALES", roles: ["ACCOUNTING"]) with { RememberMe = true };
        var (api, session, storage) = PortalTestSetup.Register(this, inTab: saved);
        api.Answer("/api/v1/android/account/me", new
        {
            user = new { username = "patron", fullName = "Firma Sahibi", role = "SALES", roles = new[] { "WAREHOUSE" }, canApprove = false },
            tenantName = "Ege Dağıtım",
        });
        var nav = Services.GetRequiredService<NavigationManager>();

        var cut = Render<Depo>();

        cut.WaitForAssertion(() => cut.Find("#warehouse-coming"));
        nav.Uri.Should().NotEndWith("/onaylar", "the old accounting role no longer decides where the user goes");
        session.Roles.Should().Equal("WAREHOUSE");
        storage.Stored!.Roles.Should().Equal("WAREHOUSE");
        storage.Stored.RememberMe.Should().BeTrue();
    }

    [Fact]
    public void A_user_left_without_a_portal_role_is_signed_out_with_the_reason()
    {
        var (api, session, storage) = PortalTestSetup.Register(this, inTab: PortalTestSetup.State(role: "SALES", roles: ["WAREHOUSE"]));
        api.Fail("/api/v1/android/account/me", HttpStatusCode.Forbidden, "PORTAL_REQUIRES_MANAGER");
        var nav = Services.GetRequiredService<NavigationManager>();

        Render<Depo>();

        nav.Uri.Should().EndWith("/login?reason=PORTAL_SALES_ONLY");
        session.IsSignedIn.Should().BeFalse();
        storage.Stored.Should().BeNull();
    }

    [Fact]
    public void Roles_read_a_moment_ago_are_not_read_again()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "SALES", roles: ["WAREHOUSE"]));

        var cut = Render<Depo>();

        cut.WaitForAssertion(() => cut.Find("#warehouse-coming"));
        api.Requests.Should().BeEmpty();
    }

    [Fact]
    public void Removing_the_last_admin_role_is_refused_with_the_servers_reason()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { Username = "ikinci" });
        var patronId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        api.Answer("/api/v1/android/account/users", new
        {
            seats = new { max = 5, used = 1, status = "active" },
            users = new object[] { new { id = patronId, username = "patron", fullName = "Firma Sahibi", role = "ADMIN", roles = new[] { "ADMIN" }, canApprove = true, isActive = true } },
        });
        api.Fail($"/api/v1/android/account/users/{patronId}", HttpStatusCode.Conflict, "LAST_ADMIN");

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=patron] .roles-btn"));
        cut.Find("tr[data-user=patron] .roles-btn").Click();
        cut.Find("#edit-roles .mud-chip[data-role=MANAGER]").Click();
        cut.Find("#edit-roles .mud-chip[data-role=ADMIN]").Click();
        cut.Find("#user-roles-edit form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("en az bir aktif admin"));
        cut.Find("#user-roles-edit");
    }

    // ---- role gates ------------------------------------------------------------------------

    [Fact]
    public void Accounting_opening_the_day_summary_is_sent_to_the_approval_desk()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "SALES", roles: ["ACCOUNTING"]));
        var nav = Services.GetRequiredService<NavigationManager>();

        var cut = Render<ErpBridge.Portal.Pages.Index>();

        cut.WaitForAssertion(() => nav.Uri.Should().EndWith("/muhasebe"));
        api.Requests.Should().BeEmpty("the summary is never asked for on behalf of a role the server refuses");
    }

    [Fact]
    public void Warehouse_staff_opening_the_approvals_are_sent_to_the_warehouse_page()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "SALES", roles: ["WAREHOUSE"]));
        var nav = Services.GetRequiredService<NavigationManager>();

        Render<Onaylar>();

        nav.Uri.Should().EndWith("/depo");
        api.Requests.Should().BeEmpty();
    }

    [Fact]
    public void Warehouse_staff_see_their_page()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "SALES", roles: ["WAREHOUSE"]));

        var cut = Render<Depo>();

        cut.WaitForAssertion(() => cut.Find("#warehouse-coming"));
    }
}
