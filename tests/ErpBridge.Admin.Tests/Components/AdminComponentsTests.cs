using Bunit;
using ErpBridge.Admin.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Admin.Tests.Components;

public sealed class AdminComponentsTests : BunitContext
{
    [Fact]
    public void StatusBadge_renders_text_and_semantic_tone()
    {
        var cut = Render<AdminStatusBadge>(parameters => parameters
            .Add(p => p.Text, "Aktif")
            .Add(p => p.Tone, "success"));

        cut.Markup.Should().Contain("Aktif");
        cut.Find("span").ClassList.Should().Contain("admin-status--success");
    }

    [Fact]
    public void ErrorState_is_announced_as_alert()
    {
        var cut = Render<AdminState>(parameters => parameters
            .Add(p => p.Title, "Veriler alınamadı")
            .Add(p => p.Message, "Bağlantıyı kontrol edin.")
            .Add(p => p.Tone, "error"));

        cut.Find("[role=alert]").TextContent.Should().Contain("Veriler alınamadı");
        cut.Markup.Should().Contain("Bağlantıyı kontrol edin.");
    }

    [Fact]
    public void LoadingState_exposes_live_status()
    {
        var cut = Render<AdminLoading>(parameters => parameters.Add(p => p.Text, "Müşteriler yükleniyor…"));

        var status = cut.Find("[role=status]");
        status.GetAttribute("aria-live").Should().Be("polite");
        status.TextContent.Should().Contain("Müşteriler yükleniyor");
    }

    [Fact]
    public void PageHeader_renders_turkish_context_and_actions()
    {
        var cut = Render<AdminPageHeader>(parameters => parameters
            .Add(p => p.Title, "Müşteriler")
            .Add(p => p.Description, "Müşteri hesaplarını yönetin.")
            .Add(p => p.Actions, builder => builder.AddMarkupContent(0, "<button>Yeni müşteri</button>")));

        cut.Find("h1").TextContent.Should().Be("Müşteriler");
        cut.Markup.Should().Contain("ERPBIDGE YÖNETİM");
        cut.Find("button").TextContent.Should().Be("Yeni müşteri");
    }
}
