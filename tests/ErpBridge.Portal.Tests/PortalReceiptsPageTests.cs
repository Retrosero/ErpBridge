using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>GOAL_DEPOLAMA_R2 S5: the /fisler page shows the phones' expense and vehicle receipts with their document.</summary>
public sealed class PortalReceiptsPageTests : PortalPageTestContext
{
    private static string DefaultQuery() =>
        $"/api/v1/portal/expense-receipts?from={Fmt.IsoDay(Fmt.Today().AddDays(-30))}&to={Fmt.IsoDay(Fmt.Today())}";

    private static readonly Guid FuelId = Guid.NewGuid();
    private static readonly Guid FuelFile = Guid.NewGuid();

    private static object Receipts(params object[] items) => new { from = "2026-09-01", to = "2026-10-01", items, truncated = false };

    private static object Fuel() => new
    {
        id = FuelId, fileId = FuelFile, documentId = "K-1", kind = "expense", sizeBytes = 1000, createdAtMs = 1_759_300_000_000, createdByName = "Ali Saha",
        url = "https://r2.test/private/ABC/expense/fuel.jpg?X-Amz-Signature=1",
        document = new { type = "expense", status = "succeeded", amount = 125.5m, description = "Yakıt", counterparty = "Gider: Yakıt", occurredAt = "01.10.2026 10:15" },
    };

    private static object Service() => new
    {
        id = Guid.NewGuid(), fileId = Guid.NewGuid(), documentId = "K-2", kind = "vehicle_maintenance", sizeBytes = 1000, createdAtMs = 1_759_200_000_000,
        createdByName = "Veli Saha", url = (string?)null, document = (object?)null,
    };

    [Fact]
    public void Lists_receipts_with_their_document_and_opens_one_with_a_fresh_address()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(DefaultQuery(), Receipts(Fuel(), Service()));
        api.Answer($"/api/v1/storage/files/{FuelFile:D}/link", new { url = "https://r2.test/private/ABC/expense/fuel.jpg?fresh=1", expiresAtMs = 1L });

        var cut = Render<Fisler>();

        cut.WaitForAssertion(() => cut.FindAll("#receipts .receipt-card").Count.Should().Be(2));
        var fuel = cut.Find($"[data-receipt='{FuelId}']");
        fuel.TextContent.Should().Contain("Gider").And.Contain("125,50").And.Contain("Yakıt").And.Contain("Ali Saha");
        fuel.QuerySelector("img")!.GetAttribute("src").Should().Contain("fuel.jpg");
        var service = cut.FindAll(".receipt-card")[1];
        service.TextContent.Should().Contain("Araç bakımı").And.Contain("K-2").And.Contain("Belge henüz sunucuya ulaşmadı");
        service.QuerySelector("img").Should().BeNull("no address while the store is not set up");

        fuel.QuerySelector("button")!.Click();

        cut.WaitForAssertion(() => cut.Find("#receipt-large").GetAttribute("src").Should().Be("https://r2.test/private/ABC/expense/fuel.jpg?fresh=1"));
        cut.Find("#receipt-open-tab").GetAttribute("target").Should().Be("_blank");
        cut.Find("#receipt-close").Click();
        cut.FindAll("#receipt-viewer").Should().BeEmpty();
    }

    [Fact]
    public void An_empty_range_shows_the_empty_state()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(DefaultQuery(), Receipts());

        var cut = Render<Fisler>();

        cut.WaitForAssertion(() => cut.Find("#receipts-empty"));
    }
}
