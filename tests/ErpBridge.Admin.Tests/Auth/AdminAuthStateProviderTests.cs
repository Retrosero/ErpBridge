using System.Security.Claims;
using ErpBridge.Admin.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Xunit;

namespace ErpBridge.Admin.Tests;

/// <summary>
/// Unit tests for <see cref="AdminAuthStateProvider"/>. The provider must
/// produce an anonymous principal when no token is set, and a fully-decorated
/// <c>scope=admin</c> principal when the token store carries a valid token.
/// </summary>
public class AdminAuthStateProviderTests
{
    [Fact]
    public async Task GetAuthenticationStateAsync_returns_anonymous_when_no_token()
    {
        var store = new TokenStore();
        var provider = new AdminAuthStateProvider(store);

        var state = await provider.GetAuthenticationStateAsync();

        state.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_returns_admin_principal_when_token_set()
    {
        var store = new TokenStore();
        var adminId = Guid.NewGuid();
        store.Set("jwt", adminId, "admin@example.com", "Admin", DateTimeOffset.UtcNow.AddMinutes(15));
        var provider = new AdminAuthStateProvider(store);

        var state = await provider.GetAuthenticationStateAsync();

        state.User.Identity?.IsAuthenticated.Should().BeTrue();
        state.User.Identity?.AuthenticationType.Should().Be("admin-bearer");
        state.User.HasClaim("scope", "admin").Should().BeTrue();
        state.User.IsInRole("admin").Should().BeTrue();
        state.User.FindFirstValue(ClaimTypes.Email).Should().Be("admin@example.com");
        state.User.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(adminId.ToString());
    }

    [Fact]
    public async Task TokenStore_changes_invoke_GetAuthenticationStateAsync_re_evaluation()
    {
        var store = new TokenStore();
        var provider = new AdminAuthStateProvider(store);

        var initial = await provider.GetAuthenticationStateAsync();
        initial.User.Identity?.IsAuthenticated.Should().BeFalse();

        store.Set("jwt", Guid.NewGuid(), "admin@example.com", "Admin", DateTimeOffset.UtcNow.AddMinutes(15));

        var after = await provider.GetAuthenticationStateAsync();
        after.User.Identity?.IsAuthenticated.Should().BeTrue();
    }
}