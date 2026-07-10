using System.Net;
using System.Net.Http.Json;
using ErpBridge.Admin.Api;
using ErpBridge.Admin.Auth;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Admin.Tests;

/// <summary>
/// Unit tests for <see cref="CentralApiClient"/> with a mocked HttpMessageHandler.
/// We cover happy-path returns, 401 -> token clear + exception, and 4xx ->
/// ApiCallException carrying the server's errorCode.
/// </summary>
public class CentralApiClientTests
{
    [Fact]
    public async Task LoginAsync_returns_token_on_200()
    {
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new AdminLoginResponse
            {
                AdminId = Guid.NewGuid(),
                Token = "jwt-abc",
                Email = "admin@example.com",
                DisplayName = "Admin",
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(15),
            })
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://centralapi.test/") };
        var client = new CentralApiClient(http, new TokenStore());

        var resp = await client.LoginAsync("admin@example.com", "secret");

        resp.Token.Should().Be("jwt-abc");
        resp.Email.Should().Be("admin@example.com");
    }

    [Fact]
    public async Task ListTenantsAsync_returns_list_on_200()
    {
        var tenants = new[]
        {
            new TenantDto { Id = Guid.NewGuid(), Name = "Acme", IsActive = true, CreatedAtUtc = DateTimeOffset.UtcNow },
            new TenantDto { Id = Guid.NewGuid(), Name = "Beta", IsActive = false, CreatedAtUtc = DateTimeOffset.UtcNow },
        };
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(tenants)
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://centralapi.test/") };
        var client = new CentralApiClient(http, new TokenStore());

        var resp = await client.ListTenantsAsync();

        resp.Should().HaveCount(2);
        resp[0].Name.Should().Be("Acme");
    }

    [Fact]
    public async Task Unauthorized_response_clears_token_and_throws_UnauthorizedApiException()
    {
        var tokens = new TokenStore();
        tokens.Set("old-jwt", Guid.NewGuid(), "a@b.com", "X", DateTimeOffset.UtcNow);
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://centralapi.test/") };
        var client = new CentralApiClient(http, tokens);

        var act = () => client.ListTenantsAsync();

        await act.Should().ThrowAsync<UnauthorizedApiException>();
        tokens.GetToken().Should().BeNull("401 must clear the token store");
    }

    [Fact]
    public async Task Error_response_with_api_error_envelope_throws_ApiCallException_with_errorCode()
    {
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new ApiErrorDto { ErrorCode = "TENANT_DUPLICATE", Message = "Name already used" })
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://centralapi.test/") };
        var client = new CentralApiClient(http, new TokenStore());

        var act = () => client.CreateTenantAsync("Acme");

        var ex = await act.Should().ThrowAsync<ApiCallException>();
        ex.Which.ErrorCode.Should().Be("TENANT_DUPLICATE");
        ex.Which.Message.Should().Contain("Name already used");
    }

    [Fact]
    public async Task Successful_request_attaches_bearer_header_from_token_store()
    {
        var tokens = new TokenStore();
        tokens.Set("my-jwt", Guid.NewGuid(), "a@b.com", "X", DateTimeOffset.UtcNow.AddMinutes(15));
        string? capturedAuthHeader = null;
        var handler = new StubHttpHandler(req =>
        {
            capturedAuthHeader = req.Headers.Authorization?.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<TenantDto>()) };
        });
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://centralapi.test/") };
        var client = new CentralApiClient(http, tokens);

        await client.ListTenantsAsync();

        capturedAuthHeader.Should().Be("Bearer my-jwt");
    }
}

/// <summary>
/// Minimal HttpMessageHandler stub. Tests instantiate the inner factory with
/// the request and return the canned response.
/// </summary>
internal sealed class StubHttpHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _factory;

    public StubHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> factory)
    {
        _factory = factory;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_factory(request));
}