using System.Net;
using Bunit;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>The pages that change something: approvals and users.</summary>
public sealed class PortalManagementPagesTests : BunitContext
{
    private static readonly Guid SaleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ReturnId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static object Request(Guid id, string kind, string counterparty, decimal amount) => new
    {
        id, kind, counterpartyName = counterparty, amount, status = "PENDING",
        requestedByName = "Ali Yılmaz", requestedAtUtc = PortalTestSetup.Now,
        summary = new { },
    };

    private static object PendingBoth() => new[] { Request(SaleId, "sale", "Bakkal Veli", 12450.5m), Request(ReturnId, "return", "Market Can", 300m) };

    [Fact]
    public void Approving_sends_the_note_and_takes_the_request_off_the_list()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/approvals?status=pending", PendingBoth());
        api.Answer($"/api/v1/android/approvals/{SaleId}/approve", Request(SaleId, "sale", "Bakkal Veli", 12450.5m));

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("article[data-request]").Should().HaveCount(2));

        var sale = cut.Find($"article[data-request='{SaleId}']");
        sale.QuerySelector("input.portal-note")!.Change(" Fiyat uygun ");
        cut.Find($"article[data-request='{SaleId}'] .portal-button:not(.portal-button--danger)").Click();

        cut.WaitForAssertion(() => cut.FindAll("article[data-request]").Should().ContainSingle());
        cut.Find("#page-notice").TextContent.Should().Be("Bakkal Veli — satış talebi onaylandı.");
        var decision = api.Requests.Single(r => r.PathAndQuery.EndsWith("/approve"));
        decision.Method.Should().Be(HttpMethod.Post);
        decision.Body.Should().Contain("\"note\":\"Fiyat uygun\"");
    }

    [Fact]
    public void A_request_someone_else_just_decided_refreshes_the_list_and_says_so()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/approvals?status=pending", PendingBoth());
        api.Fail($"/api/v1/android/approvals/{ReturnId}/reject", HttpStatusCode.Conflict, "APPROVAL_ALREADY_DECIDED");

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("article[data-request]").Should().HaveCount(2));
        api.Answer("/api/v1/android/approvals?status=pending", new[] { Request(SaleId, "sale", "Bakkal Veli", 12450.5m) });

        cut.Find($"article[data-request='{ReturnId}'] .portal-button--danger").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("başka bir yetkili"));
        cut.FindAll("article[data-request]").Should().ContainSingle();
    }

    [Fact]
    public void Refusing_ones_own_request_keeps_it_and_explains()
    {
        var (api, session, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/approvals?status=pending", PendingBoth());
        api.Fail($"/api/v1/android/approvals/{SaleId}/approve", HttpStatusCode.Forbidden, "SELF_APPROVAL_NOT_ALLOWED");

        var cut = Render<Onaylar>();
        cut.WaitForAssertion(() => cut.FindAll("article[data-request]").Should().HaveCount(2));
        cut.Find($"article[data-request='{SaleId}'] .portal-button:not(.portal-button--danger)").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("başka bir onay yetkilisi"));
        cut.FindAll("article[data-request]").Should().HaveCount(2);
        session.IsSignedIn.Should().BeTrue();
    }

    [Fact]
    public void A_manager_without_the_right_to_approve_sees_requests_but_no_buttons()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER") with { CanApprove = false });
        api.Answer("/api/v1/android/approvals?status=pending", PendingBoth());

        var cut = Render<Onaylar>();

        cut.WaitForAssertion(() => cut.FindAll("article[data-request]").Should().HaveCount(2));
        cut.Find("#approvals-readonly");
        cut.FindAll("article .portal-button").Should().BeEmpty();
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
        cut.Find("tr[data-user=patron]").QuerySelector("button").Should().BeNull();
        cut.Find("tr[data-user=ali] button").TextContent.Trim().Should().Be("Devre dışı bırak");
    }

    [Fact]
    public void Disabling_a_user_patches_them_and_shows_the_new_state()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var aliId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        api.Answer("/api/v1/android/account/users", UserList());
        api.Answer($"/api/v1/android/account/users/{aliId}", new { id = aliId, username = "ali", fullName = "Ali Yılmaz", role = "SALES", isActive = false });

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=ali] button"));
        api.Answer("/api/v1/android/account/users", UserList(aliActive: false));

        cut.Find("tr[data-user=ali] button").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Be("Ali Yılmaz devre dışı bırakıldı."));
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
        api.Answer("/api/v1/android/account/users", new { id = Guid.NewGuid(), username = "sef", fullName = "Satış Şefi", role = "MANAGER", canApprove = true, isActive = true });

        cut.Find("#new-fullname").Change(" Satış Şefi ");
        cut.Find("#new-username").Change(" Sef ");
        cut.Find("#new-password").Change("parola123");
        cut.Find("#new-role").Change("MANAGER");
        cut.Find("#new-can-approve").Change(true);
        cut.Find("#user-create form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().StartWith("Satış Şefi eklendi."));
        var body = api.Requests.Single(r => r.Method == HttpMethod.Post).Body!;
        body.Should().Contain("\"username\":\"sef\"").And.Contain("\"role\":\"MANAGER\"").And.Contain("\"canApprove\":true");
        cut.Find("#new-username").GetAttribute("value").Should().BeNullOrEmpty();
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
}
