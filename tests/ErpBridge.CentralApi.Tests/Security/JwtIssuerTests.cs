using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Options;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Tests.Security;

/// <summary>
/// Unit tests for <see cref="JwtIssuer"/>: every token must carry the
/// expected claims (<c>sub</c> = agentId, <c>tenant</c> = tenantId,
/// <c>scope</c> = agent) and a tampered signature must fail validation.
/// </summary>
public class JwtIssuerTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public JwtIssuerTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public void Issued_token_is_parseable_and_carries_expected_claims()
    {
        using var scope = _factory.Services.CreateScope();
        var issuer = scope.ServiceProvider.GetRequiredService<IJwtIssuer>();
        var agentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var issued = issuer.Issue(agentId, tenantId);

        issued.Token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(issued.Token);

        token.Subject.Should().Be(agentId.ToString());
        token.Claims.Should().Contain(c => c.Type == "tenant" && c.Value == tenantId.ToString());
        token.Claims.Should().Contain(c => c.Type == "scope" && c.Value == "agent");
    }

    [Fact]
    public void Validate_succeeds_for_freshly_issued_token()
    {
        using var scope = _factory.Services.CreateScope();
        var issuer = scope.ServiceProvider.GetRequiredService<IJwtIssuer>();
        var issued = issuer.Issue(Guid.NewGuid(), Guid.NewGuid());

        var principal = issuer.Validate(issued.Token);

        principal.Should().NotBeNull();
        principal!.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void Validate_returns_null_for_garbage()
    {
        using var scope = _factory.Services.CreateScope();
        var issuer = scope.ServiceProvider.GetRequiredService<IJwtIssuer>();

        issuer.Validate("not.a.real.jwt").Should().BeNull();
        issuer.Validate("").Should().BeNull();
        issuer.Validate("garbage").Should().BeNull();
    }

    [Fact]
    public void Validate_returns_null_for_tampered_token()
    {
        using var scope = _factory.Services.CreateScope();
        var issuer = scope.ServiceProvider.GetRequiredService<IJwtIssuer>();
        var issued = issuer.Issue(Guid.NewGuid(), Guid.NewGuid());

        // Flip the last character of the signature segment.
        var tampered = issued.Token.Substring(0, issued.Token.Length - 1)
            + (issued.Token[^1] == 'A' ? 'B' : 'A');

        issuer.Validate(tampered).Should().BeNull();
    }

    [Fact]
    public void Validate_returns_null_for_short_signing_key()
    {
        // Spin up an issuer with a too-short signing key directly. The
        // production-time <see cref="JwtIssuer.Issue"/> would throw, but
        // the validate path is supposed to fail-safe.
        var opts = new JwtOptions { SigningKey = "short" };
        var monitor = new TestOptionsMonitor<JwtOptions>(opts);
        var issuer = new JwtIssuer(monitor);

        issuer.Validate("anything").Should().BeNull();
    }

    [Fact]
    public void Validate_returns_null_when_issuer_signature_key_changes()
    {
        var opts = new JwtOptions { SigningKey = "FirstKey_FirstKey_FirstKey_FirstKey_FirstKey" };
        var monitor = new TestOptionsMonitor<JwtOptions>(opts);
        var a = new JwtIssuer(monitor);
        var issued = a.Issue(Guid.NewGuid(), Guid.NewGuid());

        // Swap the key, try to validate.
        monitor.Set(new JwtOptions { SigningKey = "SecondKey_SecondKey_SecondKey_SecondKey_SecondKey" });
        a.Validate(issued.Token).Should().BeNull();
    }

    /// <summary>Lightweight <see cref="IOptionsMonitor{T}"/> for tests that don't go through DI.</summary>
    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private T _value;

        public TestOptionsMonitor(T initial) => _value = initial;

        public T CurrentValue => _value;
        public T Get(string? name) => _value;
        public void Set(T value) => _value = value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
