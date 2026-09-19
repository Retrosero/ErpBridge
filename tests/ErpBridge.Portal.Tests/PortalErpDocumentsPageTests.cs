using System.Net;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>Goal ERP yazım Y5a: the list of documents sent to the ERP and an administrator's retry.</summary>
public sealed class PortalErpDocumentsPageTests : PortalPageTestContext
{
    private static readonly Guid FailedJob = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid WrittenJob = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static string Query(string? state = null) =>
        $"/api/v1/portal/erp-documents?from={Fmt.IsoDay(Fmt.Today().AddDays(-6))}&to={Fmt.IsoDay(Fmt.Today())}"
        + (state is null ? string.Empty : $"&state={state}") + "&page=1";

    private static object Page(bool canRetry, params object[] items) => new { items, total = items.Length, page = 1, pageSize = 50 };

    private static object Failed(bool canRetry) => new
    {
        jobId = FailedJob, externalId = "MOB-TH-1", documentType = "collection", userName = "Ali Saha", customerCode = "120.002",
        customerName = "Market Veli", amount = 500m, state = "failed", errorCode = "ERP_MAPPING_MISSING",
        message = "Portal'da kasa kodu eşlemesi eksik.", attempt = 1, enqueuedAtUtc = DateTimeOffset.UtcNow, canRetry,
    };

    private static object Written() => new
    {
        jobId = WrittenJob, externalId = "MOB-SO-1", documentType = "sales_order", userName = "Ali Saha", customerCode = "120.001",
        customerName = "Bakkal Ali", amount = 820.8m, state = "written", erpDocumentNo = "T-1234", attempt = 1, enqueuedAtUtc = DateTimeOffset.UtcNow,
    };

    [Fact]
    public void The_list_shows_the_erp_number_or_the_reason_and_an_administrator_retries_a_failed_document()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = "erp" });
        api.Answer(Query(), Page(true, Failed(canRetry: true), Written()));
        api.Answer($"/api/v1/portal/erp-documents/{FailedJob}/retry", new { jobId = FailedJob, state = "pending" });

        var cut = Render<ErpBelgeler>();
        cut.WaitForAssertion(() => cut.FindAll("#erp-documents tbody tr").Count.Should().Be(2));

        var failed = cut.Find($"tr[data-job='{FailedJob}']");
        failed.TextContent.Should().Contain("Tahsilat").And.Contain("Market Veli").And.Contain("Portal'da kasa kodu eşlemesi eksik.").And.Contain("Hata");
        cut.Find($"tr[data-job='{WrittenJob}']").TextContent.Should().Contain("T-1234").And.Contain("Yazıldı");
        cut.FindAll($"tr[data-job='{WrittenJob}'] .erp-retry").Should().BeEmpty();

        failed.QuerySelector(".erp-retry")!.Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("yeniden gönderildi"));
        api.Requests.Should().Contain(r => r.Method == HttpMethod.Post && r.PathAndQuery == $"/api/v1/portal/erp-documents/{FailedJob}/retry");
    }

    private static object Disbursement() => new
    {
        jobId = Guid.Parse("66666666-6666-6666-6666-666666666666"), externalId = "K-9", documentType = "disbursement",
        userName = "Ali Saha", customerCode = "SERHAN", customerName = "Serhan Kalay", amount = 40m, state = "failed",
        errorCode = "UNSUPPORTED_DOCUMENT_TYPE", message = "No writer is configured for document type 'disbursement'.",
        attempt = 3, enqueuedAtUtc = DateTimeOffset.UtcNow, canRetry = true,
    };

    /// <summary>
    /// The screen is Turkish, so the document key must never reach it. It used to: the label fell back to the
    /// raw key and an operator read "disbursement". An English message from the agent is replaced too, by the
    /// sentence that belongs to its error code.
    /// </summary>
    [Fact]
    public void Every_document_type_and_reason_is_shown_in_turkish()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = "erp" });
        api.Answer(Query(), Page(true, Disbursement()));

        var cut = Render<ErpBelgeler>();
        cut.WaitForAssertion(() => cut.FindAll("#erp-documents tbody tr").Count.Should().Be(1));

        var row = cut.Find("#erp-documents tbody tr");
        row.TextContent.Should().Contain("Tediye").And.NotContain("disbursement");
        row.TextContent.Should().Contain("aktarım yok").And.NotContain("No writer");

        // The type filter offers the same Turkish names, so an operator can narrow to them.
        cut.Find("#erp-type").TextContent.Should().Contain("Tediye").And.Contain("Alış faturası").And.Contain("Sayım");
    }

    /// <summary>
    /// On a phone the table is read as cards, and a card row needs its own heading: every cell carries the
    /// Turkish column name in <c>data-label</c>, which the stylesheet renders. Without it the operator would
    /// scroll sideways through a nine-column table.
    /// </summary>
    [Fact]
    public void Each_cell_carries_its_turkish_heading_so_the_phone_can_show_cards()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = "erp" });
        api.Answer(Query(), Page(true, Written()));

        var cut = Render<ErpBelgeler>();
        cut.WaitForAssertion(() => cut.FindAll("#erp-documents tbody tr").Count.Should().Be(1));

        cut.Find("#erp-documents").ClassList.Should().Contain("data-table--cards");
        var labels = cut.FindAll("#erp-documents tbody td[data-label]").Select(c => c.GetAttribute("data-label")).ToList();
        labels.Should().Contain(["Tarih", "Tür", "Gönderen", "Cari", "Tutar", "Durum", "ERP no / neden", "Deneme"]);
    }

    [Fact]
    public void Filtering_by_state_asks_the_server_again()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "ACCOUNTING") with { DataSource = "erp" });
        api.Answer(Query(), Page(false, Failed(canRetry: false), Written()));
        api.Answer(Query("failed"), Page(false, Failed(canRetry: false)));

        var cut = Render<ErpBelgeler>();
        cut.WaitForAssertion(() => cut.FindAll("#erp-documents tbody tr").Count.Should().Be(2));
        cut.FindAll(".erp-retry").Should().BeEmpty("accounting reads but does not retry");

        cut.Find("#erp-state").Change("failed");
        cut.Find("#erp-documents-filter").Submit();

        cut.WaitForAssertion(() => cut.FindAll("#erp-documents tbody tr").Count.Should().Be(1));
    }

    [Fact]
    public void A_retry_the_server_refuses_is_explained()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = "erp" });
        api.Answer(Query(), Page(true, Failed(canRetry: true)));
        api.Fail($"/api/v1/portal/erp-documents/{FailedJob}/retry", HttpStatusCode.Conflict, "JOB_NOT_RETRYABLE");

        var cut = Render<ErpBelgeler>();
        cut.WaitForAssertion(() => cut.Find(".erp-retry"));
        cut.Find(".erp-retry").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("yeniden gönderilmedi"));
    }

    [Fact]
    public void A_company_without_an_erp_sees_why_there_is_nothing()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = "native" });

        var cut = Render<ErpBelgeler>();

        cut.WaitForAssertion(() => cut.Find("#erp-not-connected"));
    }
}
