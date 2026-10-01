using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Admin.Tests.Components;

public sealed class AdminLayoutTests : BunitContext
{
    [Fact]
    public void Keyboard_users_can_skip_navigation_and_dismiss_the_mobile_menu()
    {
        this.AddAuthorization().SetAuthorized("Yönetici");
        Services.GetRequiredService<NavigationManager>().NavigateTo("licenses?tenant=demo");
        var cut = Render<ErpBridge.Admin.MainLayout>();

        var target = cut.Find(".skip-link").GetAttribute("href");
        target.Should().Be("http://localhost/licenses?tenant=demo#main-content");
        cut.Find(new Uri(target!).Fragment).GetAttribute("tabindex").Should().Be("-1");
        cut.Find(".admin-menu-button").Click();
        cut.Find(".admin-menu-button").GetAttribute("aria-expanded").Should().Be("true");
        cut.Find(".admin-shell").KeyDown("Escape");
        cut.Find(".admin-menu-button").GetAttribute("aria-expanded").Should().Be("false");
        cut.Find(".admin-menu-button").Click();
        cut.Find("a[href='licenses']").Click();
        cut.Find(".admin-menu-button").GetAttribute("aria-expanded").Should().Be("false");
    }
}
