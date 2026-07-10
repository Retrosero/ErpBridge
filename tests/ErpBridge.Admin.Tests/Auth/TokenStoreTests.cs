using ErpBridge.Admin.Auth;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Admin.Tests;

/// <summary>
/// Unit tests for <see cref="TokenStore"/> — the in-memory holder for the
/// admin JWT. The store is the single source of truth for the panel's
/// authentication state, so its invariants need to stay tight.
/// </summary>
public class TokenStoreTests
{
    [Fact]
    public void Empty_store_returns_null_token_and_anonymous_identity()
    {
        var store = new TokenStore();

        store.GetToken().Should().BeNull();
        store.GetEmail().Should().BeNull();
        store.GetDisplayName().Should().BeNull();
        store.GetAdminId().Should().Be(Guid.Empty);
    }

    [Fact]
    public void Set_persists_every_field_and_fires_changed_event()
    {
        var store = new TokenStore();
        var fired = false;
        store.Changed += (_, _) => fired = true;

        var expires = DateTimeOffset.UtcNow.AddMinutes(15);
        var adminId = Guid.NewGuid();
        store.Set("jwt-value", adminId, "admin@example.com", "Admin", expires);

        store.GetToken().Should().Be("jwt-value");
        store.GetEmail().Should().Be("admin@example.com");
        store.GetDisplayName().Should().Be("Admin");
        store.GetAdminId().Should().Be(adminId);
        store.GetExpiresAtUtc().Should().Be(expires);
        fired.Should().BeTrue();
    }

    [Fact]
    public void Clear_resets_every_field_and_fires_changed_event()
    {
        var store = new TokenStore();
        store.Set("jwt-value", Guid.NewGuid(), "admin@example.com", "Admin", DateTimeOffset.UtcNow);
        var fired = false;
        store.Changed += (_, _) => fired = true;

        store.Clear();

        store.GetToken().Should().BeNull();
        store.GetEmail().Should().BeNull();
        store.GetDisplayName().Should().BeNull();
        store.GetAdminId().Should().Be(Guid.Empty);
        fired.Should().BeTrue();
    }
}