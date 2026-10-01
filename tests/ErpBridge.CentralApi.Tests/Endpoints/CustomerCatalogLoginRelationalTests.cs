using System.IdentityModel.Tokens.Jwt;
using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §5.2 / S6: the customer signs in to the company's catalog and gets an HttpOnly session cookie;
/// unknown company, unknown name and wrong password look the same; a name that failed too often waits even with the
/// right password, unless its browser carries the device cookie; every request re-checks the account, the company,
/// its module, its published switch and its subscription; the catalog token and the staff token do not cross.
/// </summary>
public sealed class CustomerCatalogLoginRelationalTests : IClassFixture<CatalogHostFactory>
{
    private const string Pass = "musteri123";

    private readonly CatalogHostFactory _factory;

    public CustomerCatalogLoginRelationalTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task Sign_in_sets_a_host_only_http_only_strict_cookie_and_answers_me()
    {
        var c = await OpenCatalogAsync(_factory);
        var accountId = await AccountAsync(_factory, c, "C1", "yilmaz", Pass, discountPercent: 10m, showStatement: true);
        var browser = Browser(_factory);

        var info = await OkAsync<CatalogInfoResponse>(await GetAsync(browser, $"/api/v1/catalog/{c.Code.ToLowerInvariant()}/info"));
        info.Code.Should().Be(c.Code);
        info.CompanyName.Should().StartWith("Katalog firması");

        var login = await LoginAsync(browser, c, " Yilmaz ", Pass);
        var me = (await OkAsync<CatalogLoginResponse>(login)).Me;
        me.Should().Match<CatalogMeDto>(m => m.Code == c.Code && m.Username == "yilmaz" && m.DiscountPercent == 10m
            && m.Customer.Code == "C1" && m.Customer.Name.StartsWith("Yılmaz Market") && m.Features.Order && m.Features.Statement && !m.Features.Invoices);
        me.PriceList.Should().BeEquivalentTo(new CatalogPriceListDto { No = 1, Name = "Perakende", IncludesVat = true });
        me.Balance.Should().NotBeNull("the statement is shown, so is the balance");
        login.Headers.CacheControl!.NoStore.Should().BeTrue();

        var session = SetCookieHeader(login, "__Host-kt_" + c.Code)!;
        session.Should().Contain("path=/").And.Contain("secure").And.Contain("httponly").And.Contain("samesite=strict");
        session.Should().NotContain("domain=").And.NotContain("max-age", "without 'remember me' it dies with the browser");
        var device = SetCookieHeader(login, "__Host-kt_dev")!;
        device.Should().Contain("path=/").And.Contain("secure").And.Contain("httponly").And.Contain("samesite=strict").And.Contain("max-age=");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(SetCookie(login, "__Host-kt_" + c.Code));
        token.Claims.Should().Contain(x => x.Type == "scope" && x.Value == "customer-catalog");
        token.Claims.Should().Contain(x => x.Type == "sub" && x.Value == accountId.ToString());
        token.Claims.Should().Contain(x => x.Type == "tv" && x.Value == "0");
        (token.ValidTo - token.ValidFrom).Should().BeCloseTo(TimeSpan.FromHours(12), TimeSpan.FromMinutes(1));

        var again = await OkAsync<CatalogMeDto>(await GetAsync(browser, Api(c) + "/me"));
        again.Username.Should().Be("yilmaz");
        (await ReadAsync(_factory, db => db.CatalogAccounts.AsNoTracking().SingleAsync(a => a.Id == accountId))).LastLoginAtMs.Should().NotBeNull();

        var remembered = await LoginAsync(Browser(_factory), c, "yilmaz", Pass, remember: true);
        SetCookieHeader(remembered, "__Host-kt_" + c.Code).Should().Contain("max-age=2592000");

        (await SendAsync(browser, HttpMethod.Post, Api(c) + "/logout")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetAsync(browser, Api(c) + "/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized, "the cookie is gone");
    }

    [Fact]
    public async Task Unknown_company_unknown_name_and_wrong_password_get_the_same_answer()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = Browser(_factory);

        var answers = new[]
        {
            await LoginAsync(browser, c, "yilmaz", "yanlis-sifre"),
            await LoginAsync(browser, c, "kimse", Pass),
            await SendAsync(browser, HttpMethod.Post, "/api/v1/catalog/ZZZZ9999/login", new { username = "yilmaz", password = Pass }),
        };
        foreach (var answer in answers)
        {
            answer.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var error = await answer.ReadAsJsonAsync<ApiError>();
            error.ErrorCode.Should().Be("INVALID_CREDENTIALS");
            error.Message.Should().Be("Kullanıcı adı ya da şifre hatalı.");
            SetCookieHeader(answer, "__Host-kt_" + c.Code).Should().BeNull();
        }
        await ShouldFailAsync(await GetAsync(browser, "/api/v1/catalog/ZZZZ9999/info"), HttpStatusCode.NotFound, "CATALOG_NOT_FOUND");
    }

    [Fact]
    public async Task A_slowed_name_gets_429_even_with_the_right_password_but_not_on_its_own_browser()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var own = Browser(_factory);
        (await LoginAsync(own, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK, "this browser gets the device cookie");

        var stranger = Browser(_factory);
        for (var i = 0; i < 5; i++)
            (await LoginAsync(stranger, c, "yilmaz", "tahmin-" + i)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        foreach (var password in new[] { Pass, "tahmin-5" })
        {
            var slowed = await LoginAsync(stranger, c, "yilmaz", password);
            slowed.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, "the password is not even checked while the name waits");
            (await slowed.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("RATE_LIMITED");
            slowed.Headers.RetryAfter.Should().NotBeNull();
        }

        (await LoginAsync(own, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK, "the customer's own browser is not locked out by a stranger");

        // A device cookie of another account, or a forged one, does not help.
        var forged = Browser(_factory);
        forged.DefaultRequestHeaders.Add("Cookie", "__Host-kt_dev=" + Guid.NewGuid().ToString("N") + ".9999999999999.x");
        (await LoginAsync(forged, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task The_own_browser_signs_in_even_when_the_company_budget_is_spent_and_a_stale_device_cookie_does_not()
    {
        var c = await OpenCatalogAsync(_factory);
        var accountId = await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var own = Browser(_factory);
        (await LoginAsync(own, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Many addresses together spent the company's sign-in budget for this minute.
        var gate = _factory.Services.GetRequiredService<ErpBridge.CentralApi.CustomerCatalog.CatalogLoginGate>();
        while (gate.Enter(c.Code) is null) { }
        (await LoginAsync(Browser(_factory), c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        (await LoginAsync(own, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK,
            "the device cookie is checked before the company's budget: the customer's own browser is not shut out by a crowd");

        // Staff set a new password: the device cookie of the old token version no longer exempts the browser.
        (await CustomerCatalogTestSupport.SendAsync(_factory, HttpMethod.Put, $"{Base}/accounts/{accountId}/password", c.Mudur, new { password = "personel-verdi" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await LoginAsync(own, c, "yilmaz", "personel-verdi")).StatusCode.Should().Be(HttpStatusCode.TooManyRequests,
            "an older device cookie is no exemption from the company's budget");
    }

    [Fact]
    public async Task A_stranger_failing_the_name_neither_stops_a_password_change_nor_the_browser_that_made_it()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var own = Browser(_factory);
        (await LoginAsync(own, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK);
        var stranger = Browser(_factory);
        for (var i = 0; i < 5; i++)
            (await LoginAsync(stranger, c, "yilmaz", "tahmin-" + i)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await LoginAsync(stranger, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var changed = await SendAsync(own, HttpMethod.Post, Api(c) + "/password", new { current = Pass, next = "yeni-sifre-1" });
        changed.StatusCode.Should().Be(HttpStatusCode.NoContent, "the signed-in customer is counted on the account, not on the name others fail");
        SetCookieHeader(changed, "__Host-kt_dev").Should().NotBeNull("the browser is trusted at the new token version");

        (await SendAsync(own, HttpMethod.Post, Api(c) + "/logout")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await LoginAsync(own, c, "yilmaz", "yeni-sifre-1")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await LoginAsync(stranger, c, "yilmaz", "yeni-sifre-1")).StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task A_new_password_ends_the_old_sessions_and_the_browser_keeps_a_fresh_one()
    {
        var c = await OpenCatalogAsync(_factory);
        var accountId = await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = Browser(_factory);
        var oldToken = SetCookie(await LoginAsync(browser, c, "yilmaz", Pass, remember: true), "__Host-kt_" + c.Code)!;
        var other = Browser(_factory);
        (await LoginAsync(other, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK);

        await ShouldFailAsync(await SendAsync(browser, HttpMethod.Post, Api(c) + "/password", new { current = "yanlis-sifre", next = "yeni-sifre-1" }),
            HttpStatusCode.BadRequest, "INVALID_CREDENTIALS");
        await ShouldFailAsync(await SendAsync(browser, HttpMethod.Post, Api(c) + "/password", new { current = Pass, next = "kisa" }),
            HttpStatusCode.BadRequest, "INVALID_PASSWORD");
        var changed = await SendAsync(browser, HttpMethod.Post, Api(c) + "/password", new { current = Pass, next = "yeni-sifre-1" });
        changed.StatusCode.Should().Be(HttpStatusCode.NoContent, await changed.Content.ReadAsStringAsync());
        SetCookieHeader(changed, "__Host-kt_" + c.Code).Should().Contain("max-age=", "a remembered session stays remembered");

        (await GetAsync(browser, Api(c) + "/me")).StatusCode.Should().Be(HttpStatusCode.OK, "this browser has the new cookie");
        await ShouldFailAsync(await GetAsync(other, Api(c) + "/me"), HttpStatusCode.Unauthorized, "SESSION_REVOKED");
        await ShouldFailAsync(await GetAsync(Browser(_factory), Api(c) + "/me", bearer: oldToken), HttpStatusCode.Unauthorized, "SESSION_REVOKED");
        (await LoginAsync(Browser(_factory), c, "yilmaz", "yeni-sifre-1")).StatusCode.Should().Be(HttpStatusCode.OK);

        // Staff setting a new password ends the customer's sessions too.
        (await CustomerCatalogTestSupport.SendAsync(_factory, HttpMethod.Put, $"{Base}/accounts/{accountId}/password", c.Mudur, new { password = "personel-verdi" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/me"), HttpStatusCode.Unauthorized, "SESSION_REVOKED");
    }

    [Fact]
    public async Task The_module_the_published_switch_the_account_and_the_subscription_are_checked_on_every_request()
    {
        var c = await OpenCatalogAsync(_factory);
        var accountId = await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = Browser(_factory);
        (await LoginAsync(browser, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK);

        await PublishAsync(_factory, c, enabled: false);
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/me"), HttpStatusCode.Forbidden, "CATALOG_UNAVAILABLE");
        await ShouldFailAsync(await LoginAsync(Browser(_factory), c, "yilmaz", Pass), HttpStatusCode.Forbidden, "CATALOG_UNAVAILABLE");
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/info"), HttpStatusCode.NotFound, "CATALOG_NOT_FOUND");
        await PublishAsync(_factory, c);

        await SetModulesAsync(_factory, c);
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/me"), HttpStatusCode.Forbidden, "CATALOG_UNAVAILABLE");
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/info"), HttpStatusCode.NotFound, "CATALOG_NOT_FOUND");
        await SetModulesAsync(_factory, c, TenantModules.CustomerCatalog);
        (await GetAsync(browser, Api(c) + "/me")).StatusCode.Should().Be(HttpStatusCode.OK);

        await SeedAsync(_factory, db =>
        {
            var subscription = db.TenantSubscriptions.Single(s => s.TenantId == c.Id && s.IsCurrent);
            subscription.EndsAtUtc = DateTimeOffset.UtcNow.AddDays(-30);
        });
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/me"), HttpStatusCode.Forbidden, "SUBSCRIPTION_EXPIRED");
        await ShouldFailAsync(await LoginAsync(Browser(_factory), c, "yilmaz", Pass), HttpStatusCode.Forbidden, "SUBSCRIPTION_EXPIRED");
        await SeedAsync(_factory, db => db.TenantSubscriptions.Single(s => s.TenantId == c.Id && s.IsCurrent).EndsAtUtc = DateTimeOffset.UtcNow.AddYears(1));

        (await CustomerCatalogTestSupport.SendAsync(_factory, HttpMethod.Patch, $"{Base}/accounts/{accountId}", c.Mudur, new { isActive = false }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/me"), HttpStatusCode.Forbidden, "ACCOUNT_INACTIVE");
        await ShouldFailAsync(await LoginAsync(Browser(_factory), c, "yilmaz", Pass), HttpStatusCode.Forbidden, "ACCOUNT_INACTIVE");
    }

    [Fact]
    public async Task Catalog_and_staff_tokens_do_not_cross_and_a_cookie_belongs_to_its_own_company()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = Browser(_factory);
        var token = SetCookie(await LoginAsync(browser, c, "yilmaz", Pass), "__Host-kt_" + c.Code)!;

        var staffClient = _factory.CreateClient();
        (await staffClient.GetAsync("/api/v1/android/account/me", token)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await staffClient.GetAsync("/api/v1/android/tasks", token)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await staffClient.GetAsync(Base + "/settings", token)).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await ShouldFailAsync(await GetAsync(Browser(_factory), Api(c) + "/me", bearer: c.Patron), HttpStatusCode.Unauthorized, "INVALID_TOKEN");
        await ShouldFailAsync(await GetAsync(Browser(_factory), Api(c) + "/me"), HttpStatusCode.Unauthorized, "INVALID_TOKEN");

        // Another company's catalog path does not take this company's session.
        var other = await OpenCatalogAsync(_factory);
        await ShouldFailAsync(await GetAsync(Browser(_factory), Api(other) + "/me", bearer: token), HttpStatusCode.Unauthorized, "INVALID_TOKEN");
        (await GetAsync(browser, Api(other) + "/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized, "the browser has no cookie for that company");
    }

    [Fact]
    public async Task A_changing_request_needs_the_page_header_and_the_catalog_origin()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = Browser(_factory);

        await ShouldFailAsync(await SendAsync(browser, HttpMethod.Post, Api(c) + "/login", new { username = "yilmaz", password = Pass }, pageHeaders: false),
            HttpStatusCode.Forbidden, "CSRF_REJECTED");
        (await LoginAsync(browser, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK);

        await ShouldFailAsync(await SendAsync(browser, HttpMethod.Post, Api(c) + "/cart/quote", new { lines = Array.Empty<object>() }, pageHeaders: false),
            HttpStatusCode.Forbidden, "CSRF_REJECTED");

        var foreign = new HttpRequestMessage(HttpMethod.Post, Api(c) + "/cart/quote") { Content = JsonContent(new { lines = Array.Empty<object>() }) };
        foreign.Headers.Add("X-Katalog", "1");
        foreign.Headers.Add("Origin", "https://baska.appsgo.cloud");
        await ShouldFailAsync(await browser.SendAsync(foreign), HttpStatusCode.Forbidden, "CSRF_REJECTED");

        var noOrigin = new HttpRequestMessage(HttpMethod.Post, Api(c) + "/cart/quote") { Content = JsonContent(new { lines = Array.Empty<object>() }) };
        noOrigin.Headers.Add("X-Katalog", "1");
        (await browser.SendAsync(noOrigin)).StatusCode.Should().Be(HttpStatusCode.OK, "a browser that sends no Origin passes on the header alone");
    }

    private static StringContent JsonContent(object body) =>
        new(System.Text.Json.JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");
}
