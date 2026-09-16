using System.Net;
using Bunit;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// The approval desk (plan step 5): accounting works through the queue from the keyboard, oldest request
/// first, and sees other approvers' decisions without reloading.
/// </summary>
public sealed class PortalApprovalDeskTests : PortalPageTestContext
{
    private const string Queue = "/api/v1/android/approvals?status=pending&take=500";
    private const string Cursor = "/api/v1/portal/events?approvalsSeq=0&wait=0";
    private const string Live = "/api/v1/portal/events?approvalsSeq=500&wait=25";

    private static Guid Id(int n) => Guid.Parse($"00000000-0000-0000-0000-{n:D12}");

    private static object Request(int n, string kind = "sale", string counterparty = "Bakkal", decimal amount = 100m, int minutesAgo = 10, string by = "Ali Yılmaz") => new
    {
        id = Id(n), kind, counterpartyName = counterparty, amount, status = "Pending",
        requestedByName = by, requestedAtUtc = PortalTestSetup.Now.AddMinutes(-minutesAgo),
        summary = new { },
    };

    private static object Detail(int n, string kind = "sale", string counterparty = "Bakkal", decimal amount = 100m) => new
    {
        request = Request(n, kind, counterparty, amount),
        documents = Array.Empty<object>(),
        events = new[] { new { action = "Submitted", byName = "Ali Yılmaz", atUtc = PortalTestSetup.Now.AddMinutes(-10) } },
        warnings = Array.Empty<object>(),
    };

    private FakeCentralApi Desk(object[] queue, bool canApprove = true)
    {
        var state = PortalTestSetup.State(role: "SALES", roles: ["ACCOUNTING"]) with { CanApprove = canApprove };
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: state);
        api.Answer(Cursor, new { latestSeq = 0, approvalsSeq = 500, changed = true });
        api.Answer(Live, new { latestSeq = 0, approvalsSeq = 500, changed = false });
        api.Answer(Queue, queue);
        return api;
    }

    private static Task Key(IRenderedComponent<Muhasebe> cut, string key, bool shift = false, bool inField = false) =>
        cut.InvokeAsync(() => cut.Instance.OnKey(key, shift, ctrl: false, inField));

    private static string? CurrentRow(IRenderedComponent<Muhasebe> cut) =>
        cut.FindAll(".desk-row.is-current").SingleOrDefault()?.GetAttribute("data-request");

    [Fact]
    public async Task Ten_requests_are_approved_from_the_keyboard_oldest_first()
    {
        // Listed newest first by the server; the desk puts the longest-waiting request on top.
        var queue = Enumerable.Range(1, 10).Select(n => Request(n, counterparty: $"Müşteri {n}", minutesAgo: n * 5)).ToArray();
        var api = Desk(queue);
        foreach (var n in Enumerable.Range(1, 10))
        {
            api.Answer($"/api/v1/android/approvals/{Id(n)}", Detail(n, counterparty: $"Müşteri {n}"));
            api.Answer($"/api/v1/android/approvals/{Id(n)}/approve", Request(n));
        }

        var cut = Render<Muhasebe>();
        cut.WaitForAssertion(() => cut.FindAll(".desk-row").Should().HaveCount(10));
        CurrentRow(cut).Should().Be(Id(10).ToString());

        for (var i = 0; i < 10; i++) await Key(cut, "a");

        cut.WaitForAssertion(() => cut.Find("#desk-empty"));
        api.Requests.Where(r => r.Method == HttpMethod.Post).Select(r => r.PathAndQuery)
            .Should().Equal(Enumerable.Range(1, 10).Reverse().Select(n => $"/api/v1/android/approvals/{Id(n)}/approve"));
        cut.Find("#page-notice").TextContent.Should().Contain("Müşteri 1 — satış talebi onaylandı.");
        cut.FindAll("#page-error").Should().BeEmpty();
    }

    [Fact]
    public async Task Arrow_keys_open_the_next_request_and_a_note_goes_with_the_rejection()
    {
        var api = Desk([Request(1, minutesAgo: 30, counterparty: "Market Can"), Request(2, minutesAgo: 20, counterparty: "Bakkal Veli", amount: 1250m), Request(3, minutesAgo: 5, counterparty: "Büfe Ece")]);
        api.Answer($"/api/v1/android/approvals/{Id(1)}", Detail(1, counterparty: "Market Can"));
        api.Answer($"/api/v1/android/approvals/{Id(3)}", Detail(3, counterparty: "Büfe Ece"));
        api.Answer($"/api/v1/android/approvals/{Id(2)}", new
        {
            request = new
            {
                id = Id(2), kind = "sale", counterpartyName = "Bakkal Veli", amount = 1250m, status = "Pending",
                requestedByName = "Ali Yılmaz", requestedAtUtc = PortalTestSetup.Now.AddMinutes(-20),
                summary = new { paymentType = "Cari Borç", description = "Hafta sonu teslim" },
            },
            documents = new[]
            {
                new { documentType = "sales_order", externalId = "SO-2", payload = new { customerCode = "C-002", lines = new[] { new { productCode = "CAY-1", productTitle = "Çay 1 kg", quantity = 5, unitPrice = 250, lineTotal = 1250 } } } },
            },
            events = new[] { new { action = "Submitted", byName = "Ali Yılmaz", atUtc = PortalTestSetup.Now.AddMinutes(-20) } },
            warnings = new[] { new { stockCode = "CAY-1", title = "Çay 1 kg", requested = 5, onHand = 2 } },
        });
        api.Answer("/api/v1/portal/balances?search=C-002", new { rows = new[] { new { customerCode = "C-002", title = "Bakkal Veli", balance = 3400.5m } } });
        api.Answer($"/api/v1/android/approvals/{Id(2)}/reject", Request(2));

        var cut = Render<Muhasebe>();
        cut.WaitForAssertion(() => CurrentRow(cut).Should().Be(Id(1).ToString()));

        await Key(cut, "ArrowDown");

        cut.WaitForAssertion(() => cut.Find("#desk-lines").TextContent.Should().Contain("Çay 1 kg"));
        CurrentRow(cut).Should().Be(Id(2).ToString());
        cut.Find("#desk-detail-title").TextContent.Should().Be("Bakkal Veli");
        cut.Find("#desk-warnings").TextContent.Should().Contain("istenen 5, stokta 2");
        cut.Find("#desk-balance").TextContent.Should().Contain("3.400,50 TL");
        cut.Find("#desk-detail").TextContent.Should().Contain("Cari Borç").And.Contain("Hafta sonu teslim");

        await Key(cut, "n");
        cut.Find("#desk-dialog").GetAttribute("data-dialog").Should().Be("Note");
        cut.Find("#desk-dialog-input").Input("fiyat listesi eski");
        await Key(cut, "Enter", inField: true);
        cut.Find("#desk-current-note").TextContent.Should().Contain("fiyat listesi eski");

        await Key(cut, "r");
        cut.Find("#desk-dialog-input").GetAttribute("value").Should().Be("fiyat listesi eski");
        await Key(cut, "Enter", inField: true);

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Bakkal Veli — satış talebi reddedildi."));
        api.Requests.Single(r => r.Method == HttpMethod.Post).Body.Should().Contain("\"note\":\"fiyat listesi eski\"");
        cut.FindAll(".desk-row").Should().HaveCount(2);
        CurrentRow(cut).Should().Be(Id(3).ToString(), "the desk moves on to the next request in line");
    }

    [Fact]
    public async Task Marked_requests_are_approved_together_after_a_confirmation()
    {
        var api = Desk([Request(1, minutesAgo: 30, amount: 100m), Request(2, minutesAgo: 20, amount: 250m), Request(3, minutesAgo: 10, amount: 999m)]);
        foreach (var n in new[] { 1, 2, 3 })
        {
            api.Answer($"/api/v1/android/approvals/{Id(n)}", Detail(n));
            api.Answer($"/api/v1/android/approvals/{Id(n)}/approve", Request(n));
        }
        var cut = Render<Muhasebe>();
        cut.WaitForAssertion(() => cut.FindAll(".desk-row").Should().HaveCount(3));

        await Key(cut, "A", shift: true);
        cut.Find("#page-info").TextContent.Should().Contain("Boşluk");
        cut.FindAll("#desk-dialog").Should().BeEmpty();

        await Key(cut, " ");
        await Key(cut, "ArrowDown");
        await Key(cut, " ");
        cut.Find("#desk-count").TextContent.Should().Contain("2 seçili");

        await Key(cut, "A", shift: true);
        cut.Find("#desk-bulk-summary").TextContent.Should().Contain("350,00 TL");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post, "nothing is approved before the confirmation");
        await Key(cut, "Enter");

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("2 talep onaylandı"));
        api.Requests.Where(r => r.Method == HttpMethod.Post).Select(r => r.PathAndQuery)
            .Should().Equal($"/api/v1/android/approvals/{Id(1)}/approve", $"/api/v1/android/approvals/{Id(2)}/approve");
        cut.FindAll(".desk-row").Select(r => r.GetAttribute("data-request")).Should().Equal(Id(3).ToString());
    }

    [Fact]
    public async Task A_request_another_approver_just_decided_is_explained_and_leaves_the_queue()
    {
        var api = Desk([Request(1, minutesAgo: 30, counterparty: "Market Can"), Request(2, minutesAgo: 10)]);
        api.Answer($"/api/v1/android/approvals/{Id(1)}", Detail(1, counterparty: "Market Can"));
        api.Answer($"/api/v1/android/approvals/{Id(2)}", Detail(2));
        api.Fail($"/api/v1/android/approvals/{Id(1)}/approve", HttpStatusCode.Conflict, "APPROVAL_ALREADY_DECIDED");
        var cut = Render<Muhasebe>();
        cut.WaitForAssertion(() => CurrentRow(cut).Should().Be(Id(1).ToString()));
        api.Answer(Queue, new[] { Request(2, minutesAgo: 10) });

        await Key(cut, "a");

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("başka bir yetkili"));
        cut.FindAll(".desk-row").Should().ContainSingle();
        CurrentRow(cut).Should().Be(Id(2).ToString());
    }

    [Fact]
    public void Another_approvers_decision_reaches_the_open_desk_without_a_reload()
    {
        var api = Desk([Request(1, minutesAgo: 30, counterparty: "Market Can"), Request(2, minutesAgo: 10, counterparty: "Büfe Ece")]);
        api.Answer($"/api/v1/android/approvals/{Id(1)}", Detail(1, counterparty: "Market Can"));
        api.Answer($"/api/v1/android/approvals/{Id(2)}", Detail(2, counterparty: "Büfe Ece"));
        var cut = Render<Muhasebe>();
        cut.WaitForAssertion(() => CurrentRow(cut).Should().Be(Id(1).ToString()));
        cut.WaitForAssertion(() => cut.Find("#desk-live").TextContent.Trim().Should().Be("Canlı"));

        api.Answer(Queue, new[] { Request(2, minutesAgo: 10, counterparty: "Büfe Ece") });
        api.Answer(Live, new { latestSeq = 0, approvalsSeq = 501, changed = true });

        cut.WaitForAssertion(() => cut.FindAll(".desk-row").Should().ContainSingle(), TimeSpan.FromSeconds(5));
        cut.Find("#page-info").TextContent.Should().Contain("başka bir yetkili");
        CurrentRow(cut).Should().Be(Id(2).ToString());
    }

    [Fact]
    public async Task Search_and_the_kind_filter_narrow_the_queue()
    {
        var api = Desk([Request(1, "collection", "Bakkal Veli", minutesAgo: 30), Request(2, "sale", "Market Can", minutesAgo: 20), Request(3, "sale", "Bakkal Ece", minutesAgo: 10, by: "Veli Saha")]);
        foreach (var n in new[] { 1, 2, 3 }) api.Answer($"/api/v1/android/approvals/{Id(n)}", Detail(n));
        var cut = Render<Muhasebe>();
        cut.WaitForAssertion(() => cut.FindAll(".desk-row").Should().HaveCount(3));

        cut.Find("#desk-search").Input("veli");
        cut.FindAll(".desk-row").Select(r => r.GetAttribute("data-request")).Should().Equal(Id(1).ToString(), Id(3).ToString());

        await Key(cut, "Escape", inField: true);
        await Key(cut, "f");
        cut.Find("#desk-filter").TextContent.Trim().Should().Be("Satış");
        cut.FindAll(".desk-row").Select(r => r.GetAttribute("data-request")).Should().Equal(Id(3).ToString());
        CurrentRow(cut).Should().Be(Id(3).ToString(), "the open request stays visible");

        await Key(cut, "f");
        await Key(cut, "f");
        cut.Find("#desk-filter").TextContent.Trim().Should().Be("Tüm türler");
    }

    [Fact]
    public async Task Without_the_right_to_approve_the_desk_only_shows()
    {
        var api = Desk([Request(1)], canApprove: false);
        api.Answer($"/api/v1/android/approvals/{Id(1)}", Detail(1));
        var cut = Render<Muhasebe>();
        cut.WaitForAssertion(() => cut.Find("#desk-readonly"));

        await Key(cut, "a");
        await Key(cut, "r");

        cut.FindAll("#desk-approve").Should().BeEmpty();
        cut.FindAll("#desk-dialog").Should().BeEmpty();
        cut.Find("#page-error").TextContent.Should().Contain("yetkiniz yok");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post);
    }

    [Fact]
    public async Task The_help_lists_the_shortcuts_and_escape_closes_it()
    {
        Desk([]);
        var cut = Render<Muhasebe>();
        cut.WaitForAssertion(() => cut.Find("#desk-empty"));

        await Key(cut, "?");
        cut.Find("#desk-dialog").TextContent.Should().Contain("İşaretlileri onayla");
        await Key(cut, "Escape");
        cut.FindAll("#desk-dialog").Should().BeEmpty();
    }
}
