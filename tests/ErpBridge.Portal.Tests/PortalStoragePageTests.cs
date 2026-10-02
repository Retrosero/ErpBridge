using System.Net;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_DEPOLAMA_R2 P1: the /depolama page — the quota bar turns yellow at 80 % and red at 95 %, "Alan aç" sends only the
/// chosen candidates after a confirmation (the XML group says it deletes for good), the trash shows the items a restore
/// could not bring back with their reasons, emptying the trash asks first, and only who manages storage sees the page.
/// </summary>
public sealed class PortalStoragePageTests : PortalPageTestContext
{
    private const long Gb = 1L << 30;
    private const string Usage = "/api/v1/storage/usage";
    private const string Summary = "/api/v1/storage/cleanup/summary";
    private const string Trash = "/api/v1/storage/trash";
    private const string Xml = "/api/v1/storage/xml-images/status";

    private static readonly Guid PhotoZ = Guid.NewGuid();
    private static readonly Guid PhotoY = Guid.NewGuid();
    private static readonly Guid ItemA = Guid.NewGuid();
    private static readonly Guid ItemB = Guid.NewGuid();

    private static object UsageOf(long used, long quota = 5 * Gb) => new
    {
        available = true, usedBytes = used, quotaBytes = quota, freeBytes = Math.Max(0, quota - used), trashedBytes = 300L << 20, trashedCount = 2,
        areas = new[]
        {
            new { area = "product", usedBytes = 2 * Gb, fileCount = 1200 },
            new { area = "xml", usedBytes = 1 * Gb, fileCount = 800 },
            new { area = "task", usedBytes = 200L << 20, fileCount = 40 },
        },
    };

    private static object SummaryOf() => new
    {
        days = 90,
        groups = new object[]
        {
            new { group = "missing_products", label = "Artık olmayan ürünlerin görselleri", count = 2, bytes = 3L << 20, purgesDirectly = false },
            new { group = "out_of_stock", label = "Stokta olmayan ürünlerin görselleri", count = 0, bytes = 0L, purgesDirectly = false },
            new { group = "xml_unused", label = "Kullanılmayan XML görselleri", count = 5, bytes = 10L << 20, purgesDirectly = true },
        },
    };

    private static object TrashOf(params object[] items) => new { page = 1, pageSize = 50, total = items.Length, totalBytes = 2L << 20, trashDays = 7, items };

    private static object Item(Guid id, string label, bool restorable = true, int daysLeft = 6) => new
    {
        id, area = "product", kind = "product_image", label, sizeBytes = 1L << 20, fileCount = 2, source = "user", trashedAtMs = 1_759_300_000_000,
        trashedByName = "Ali Saha", daysLeft, restorable, thumbUrl = $"https://img.test/{label}-s.webp",
    };

    private static object XmlOf(bool module) => new
    {
        configured = true, moduleEnabled = module, downloadImages = true, storageAvailable = true, status = "ok", finishedAtMs = 1_759_300_000_000,
        imageCount = 120, imageBytes = 40L << 20, productCount = 60,
    };

    private FakeCentralApi Setup(long used, object? trash = null, bool xmlModule = true, string role = "ADMIN")
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: role));
        api.Answer(Usage, UsageOf(used));
        api.Answer(Summary, SummaryOf());
        api.Answer(Trash, trash ?? TrashOf());
        api.Answer(Xml, XmlOf(xmlModule));
        return api;
    }

    [Theory]
    [InlineData(3.2, "ok", "3,2 GB / 5,0 GB", null)]
    [InlineData(4.1, "warn", "4,1 GB / 5,0 GB", "#storage-warn")]
    [InlineData(4.8, "full", "4,8 GB / 5,0 GB", "#storage-full")]
    public void The_quota_bar_turns_yellow_at_80_and_red_at_95_percent(double usedGb, string level, string text, string? alert)
    {
        Setup((long)(usedGb * Gb));

        var cut = Render<Depolama>();

        cut.WaitForAssertion(() => cut.Find("#storage-quota").GetAttribute("data-level").Should().Be(level));
        cut.Find("#storage-used").TextContent.Should().Be(text);
        cut.Find("#storage-trash-share").TextContent.Should().Contain("300,0 MB");
        cut.FindAll("#storage-warn").Should().HaveCount(alert == "#storage-warn" ? 1 : 0);
        cut.FindAll("#storage-full").Should().HaveCount(alert == "#storage-full" ? 1 : 0);
        cut.FindAll("#storage-areas .storage-area").Select(a => a.QuerySelector(".text-muted")!.TextContent).Should().Equal("Ürün", "XML", "Görev");
        cut.Find("[data-area='product']").TextContent.Should().Contain("2,0 GB").And.Contain("1.200 dosya");
    }

    [Fact]
    public void Chosen_candidates_go_to_the_trash_only_after_the_confirmation()
    {
        var api = Setup(1 * Gb);
        api.Answer("/api/v1/storage/cleanup/candidates?group=missing_products", new
        {
            group = "missing_products", label = "Artık olmayan ürünlerin görselleri", page = 1, pageSize = 50, total = 2, totalBytes = 3L << 20,
            items = new object[]
            {
                new { id = PhotoZ, kind = "product_image", label = "Z9", area = "product", sizeBytes = 2L << 20, thumbUrl = "https://img.test/z9-s.webp", extra = "Ürün fotoğrafı" },
                new { id = PhotoY, kind = "catalog_image", label = "Y1", area = "catalog", sizeBytes = 1L << 20, thumbUrl = (string?)null, extra = "Katalog görseli" },
            },
        });
        api.Answer("/api/v1/storage/cleanup", new
        {
            group = "missing_products", trashedCount = 1, trashedBytes = 2L << 20, purgedCount = 0, purgedBytes = 0L, remaining = 0,
            message = "1 kayıt çöpe taşındı (2 MB; çöp boşaltılınca yer açılır).",
        });

        var cut = Render<Depolama>();
        cut.WaitForAssertion(() => cut.Find("[data-group='missing_products'] .group-size").TextContent.Should().Contain("2 kayıt").And.Contain("3,0 MB"));
        cut.Find("[data-group='out_of_stock'] .group-toggle").HasAttribute("disabled").Should().BeTrue("an empty group has nothing to show");

        cut.Find("[data-group='missing_products'] .group-toggle").Click();
        cut.WaitForAssertion(() => cut.FindAll("#cleanup-items .storage-item").Should().HaveCount(2));
        cut.Find($"[data-candidate='{PhotoZ}'] img").GetAttribute("src").Should().Be("https://img.test/z9-s.webp");
        cut.Find("#cleanup-selected").HasAttribute("disabled").Should().BeTrue("nothing is chosen yet");

        cut.Find($"[data-candidate='{PhotoZ}'] input").Change(true);
        cut.Find("#cleanup-selected").Click();

        cut.Find("#cleanup-confirm-text").TextContent.Should().Contain("1 kayıt (2,0 MB) çöpe taşınacak").And.Contain("7 gün");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post, "nothing is sent before the confirmation");
        cut.Find("#cleanup-confirm").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("1 kayıt çöpe taşındı"));
        var body = JsonDocument.Parse(api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/storage/cleanup").Body!).RootElement;
        body.GetProperty("group").GetString().Should().Be("missing_products");
        body.GetProperty("ids").EnumerateArray().Select(e => e.GetGuid()).Should().Equal(PhotoZ);
        cut.FindAll("#cleanup-items").Should().BeEmpty("the group closes after the clean-up");
        api.Requests.Count(r => r.PathAndQuery == Usage).Should().Be(2, "the figures are read again");
    }

    [Fact]
    public void The_xml_group_warns_that_its_pictures_are_deleted_for_good()
    {
        var api = Setup(1 * Gb);
        api.Answer("/api/v1/storage/cleanup/candidates?group=xml_unused", new
        {
            group = "xml_unused", label = "Kullanılmayan XML görselleri", page = 1, pageSize = 50, total = 5, totalBytes = 10L << 20,
            items = new[] { new { id = PhotoZ, kind = "xml_image", label = "A", area = "xml", sizeBytes = 2L << 20, thumbUrl = (string?)null, extra = "XML modülü kapalı" } },
        });
        api.Answer("/api/v1/storage/cleanup", new { group = "xml_unused", trashedCount = 0, trashedBytes = 0L, purgedCount = 5, purgedBytes = 10L << 20, remaining = 0, message = "5 XML görseli kalıcı silindi." });

        var cut = Render<Depolama>();
        cut.WaitForAssertion(() => cut.Find("[data-group='xml_unused'] .group-size").TextContent.Should().Contain("kalıcı silinir"));
        cut.Find("[data-group='xml_unused'] .group-toggle").Click();
        cut.WaitForAssertion(() => cut.Find("#cleanup-all"));
        cut.Find("#cleanup-selected").TextContent.Should().Contain("Seçilenleri kalıcı sil");

        cut.Find("#cleanup-all").Click();

        cut.Find("#cleanup-confirm-text").TextContent.Should().Contain("5 XML görseli (10,0 MB) kalıcı silinir; XML'den yeniden indirilebilir");
        cut.Find("#cleanup-confirm").TextContent.Trim().Should().Be("Kalıcı sil");
        cut.Find("#cleanup-confirm").Click();
        cut.WaitForAssertion(() => cut.Find("#page-notice"));
        var body = JsonDocument.Parse(api.Requests.Single(r => r.PathAndQuery == "/api/v1/storage/cleanup").Body!).RootElement;
        body.GetProperty("all").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void A_restore_shows_which_items_could_not_come_back_and_why()
    {
        var api = Setup(1 * Gb, TrashOf(Item(ItemA, "A1"), Item(ItemB, "B1", restorable: false, daysLeft: 0)));
        api.Answer("/api/v1/storage/trash/restore", new
        {
            restored = 1, restoredBytes = 1L << 20, failed = 1,
            items = new object[]
            {
                new { id = ItemA, label = "A1", restored = true, reason = (string?)null },
                new { id = ItemB, label = "B1", restored = false, reason = "Bu dosyaların kaydı silinmiş; geri alınamaz." },
            },
        });

        var cut = Render<Depolama>();
        cut.WaitForAssertion(() => cut.FindAll("#trash-items .storage-item").Should().HaveCount(2));
        cut.Find($"[data-trash='{ItemA}'] .days-left").TextContent.Should().Be("6 gün kaldı");
        cut.Find($"[data-trash='{ItemB}'] .days-left").TextContent.Should().Be("Bugün kalıcı silinecek");
        cut.Find($"[data-trash='{ItemB}']").TextContent.Should().Contain("Geri alınamaz");
        cut.Find($"[data-trash='{ItemA}']").TextContent.Should().Contain("Ali Saha").And.Contain("Kullanıcı sildi");

        cut.Find("#storage-trash .select-page").Change(true);
        cut.Find("#trash-restore").TextContent.Should().Contain("(2)");
        cut.Find("#trash-restore").Click();

        cut.WaitForAssertion(() => cut.Find("#restore-failures"));
        cut.Find($"#restore-failures [data-failed='{ItemB}']").TextContent.Should().Contain("B1").And.Contain("kaydı silinmiş; geri alınamaz");
        cut.FindAll("#restore-failures li").Should().HaveCount(1);
        cut.Find("#page-notice").TextContent.Should().Be("1 kayıt geri alındı.");
        var body = JsonDocument.Parse(api.Requests.Single(r => r.PathAndQuery == "/api/v1/storage/trash/restore").Body!).RootElement;
        body.GetProperty("ids").EnumerateArray().Select(e => e.GetGuid()).Should().BeEquivalentTo([ItemA, ItemB]);
    }

    [Fact]
    public void Emptying_the_trash_asks_first_and_frees_the_space()
    {
        var api = Setup(1 * Gb, TrashOf(Item(ItemA, "A1")));
        api.Answer("/api/v1/storage/trash/purge", new { purged = 1, purgedBytes = 1L << 20, failed = 0 });

        var cut = Render<Depolama>();
        cut.WaitForAssertion(() => cut.Find("#trash-empty-all"));
        cut.Find("#trash-purge").HasAttribute("disabled").Should().BeTrue("nothing is chosen");
        cut.Find("#trash-empty-all").Click();

        cut.Find("#trash-confirm-text").TextContent.Should().Contain("1 kaydın hepsi kalıcı silinecek; geri alınamaz").And.Contain("2,0 MB");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post);
        cut.Find("#trash-cancel").Click();
        cut.FindAll("#trash-confirm-box").Should().BeEmpty();

        cut.Find("#trash-empty-all").Click();
        cut.Find("#trash-confirm").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Be("1 kayıt kalıcı silindi, 1,0 MB yer açıldı."));
        JsonDocument.Parse(api.Requests.Single(r => r.PathAndQuery == "/api/v1/storage/trash/purge").Body!).RootElement.GetProperty("all").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void The_xml_panel_shows_the_sync_and_asks_for_one_now_only_with_the_module()
    {
        var api = Setup(1 * Gb);
        api.Answer("/api/v1/storage/xml-images/sync", new
        {
            configured = true, moduleEnabled = true, downloadImages = true, storageAvailable = true, requestedAtMs = 1_759_400_000_000,
            imageCount = 120, imageBytes = 40L << 20, productCount = 60,
        }, HttpStatusCode.Accepted);

        var cut = Render<Depolama>();
        cut.WaitForAssertion(() => cut.Find("#xml-counts").TextContent.Should().Contain("120 görsel").And.Contain("60 ürün").And.Contain("40,0 MB"));
        cut.Find("#xml-state").TextContent.Should().Contain("Son eşitleme tamamlandı");
        cut.Find("#xml-sync").Click();

        cut.WaitForAssertion(() => cut.Find("#xml-state").TextContent.Should().Contain("Eşitleme sırada"));
        cut.Find("#xml-sync").HasAttribute("disabled").Should().BeTrue("a run is already waiting");
        api.Requests.Should().Contain(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/storage/xml-images/sync");
    }

    [Fact]
    public void Without_the_xml_module_the_xml_panel_is_hidden()
    {
        Setup(1 * Gb, xmlModule: false);

        var cut = Render<Depolama>();

        cut.WaitForAssertion(() => cut.Find("#storage-trash"));
        cut.FindAll("#storage-xml").Should().BeEmpty();
    }

    [Fact]
    public void Only_who_manages_storage_reaches_the_page_and_its_menu_item()
    {
        PortalRoles.Allows(["ACCOUNTING"], new Dictionary<string, bool> { ["action.storage.manage"] = true }, PortalArea.Storage)
            .Should().BeFalse("the key is locked to ADMIN and MANAGER");
        PortalRoles.Allows(["MANAGER"], new Dictionary<string, bool> { ["action.storage.manage"] = false }, PortalArea.Storage).Should().BeFalse();
        PortalRoles.Allows(["MANAGER"], null, PortalArea.Storage).Should().BeTrue();

        var api = Setup(1 * Gb, role: "ACCOUNTING");
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("depolama");

        Render<Depolama>();

        nav.Uri.Should().Be(nav.BaseUri + "muhasebe", "accounting lands on its own home page");
        api.Requests.Should().NotContain(r => r.PathAndQuery.StartsWith("/api/v1/storage", StringComparison.Ordinal));
    }

    [Fact]
    public void The_menu_shows_depolama_to_a_manager_and_not_to_accounting()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER"), popoverProvider: false);
        var cut = Render<ErpBridge.Portal.MainLayout>(p => p.Add(l => l.Body, (RenderFragment)(b => b.AddContent(0, "içerik"))));
        cut.FindAll("#nav-storage").Should().ContainSingle().Which.GetAttribute("href").Should().Be("depolama");
    }

    [Fact]
    public void The_menu_hides_depolama_from_accounting()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "ACCOUNTING"), popoverProvider: false);
        var cut = Render<ErpBridge.Portal.MainLayout>(p => p.Add(l => l.Body, (RenderFragment)(b => b.AddContent(0, "içerik"))));
        cut.FindAll("#nav-storage").Should().BeEmpty();
    }
}
