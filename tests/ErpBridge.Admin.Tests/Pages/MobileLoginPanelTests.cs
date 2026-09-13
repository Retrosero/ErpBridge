using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using ErpBridge.Admin.Api;
using ErpBridge.Admin.Auth;
using ErpBridge.Admin.Shared;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Admin.Tests.Pages;

/// <summary>
/// bUnit tests for <see cref="MobileLoginPanel"/> — the phone-login area on the
/// license page, where an operator creates the username and password a company
/// types on the phone together with its company code.
/// </summary>
public sealed class MobileLoginPanelTests : BunitContext
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void A_company_without_seats_is_asked_for_seats_first_and_saving_them_records_a_subscription()
    {
        var api = Register(Overview(status: "none", max: 0, users: []));

        var cut = Render<MobileLoginPanel>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find($"#ml-seats-{TenantId}"));
        cut.FindAll($"#ml-create-{TenantId}").Should().BeEmpty();

        cut.Find($"#ml-seats-{TenantId}").Change("3");
        api.OverviewAfterWrite = Overview(status: "active", max: 3, users: []);
        cut.Find($"#ml-seats-save-{TenantId}").Click();

        cut.WaitForAssertion(() => cut.Find($"#ml-create-{TenantId}"));
        api.LastSubscription!.Value.GetProperty("seats").GetInt32().Should().Be(3);
    }

    [Fact]
    public void Creating_a_phone_login_shows_the_company_code_and_username_to_hand_over()
    {
        var api = Register(Overview(status: "active", max: 3, users: []));

        var cut = Render<MobileLoginPanel>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find($"#ml-create-{TenantId}"));
        cut.Find($"#ml-username-{TenantId}").Change("Patron");
        cut.Find($"#ml-fullname-{TenantId}").Change("Firma Sahibi");
        cut.Find($"#ml-password-{TenantId}").Change("parola123");
        api.OverviewAfterWrite = Overview(status: "active", max: 3, users: [new MobileUserDto { Id = Guid.NewGuid(), Username = "patron", FullName = "Firma Sahibi", Role = "ADMIN", IsActive = true }]);
        cut.Find($"#ml-create-{TenantId}").Click();

        cut.WaitForAssertion(() => cut.Find(".mobile-login__created").TextContent.Should().Contain("ABCD2345").And.Contain("patron"));
        var body = api.LastCreatedUser!.Value;
        body.GetProperty("username").GetString().Should().Be("patron");
        body.GetProperty("role").GetString().Should().Be("ADMIN", "a company's first phone user is its administrator");
        cut.Markup.Should().NotContain("parola123", "the password is never shown back");
    }

    [Fact]
    public void A_company_with_every_seat_taken_cannot_add_a_login()
    {
        var users = new[]
        {
            new MobileUserDto { Id = Guid.NewGuid(), Username = "patron", FullName = "Patron", Role = "ADMIN", IsActive = true },
        };
        Register(Overview(status: "active", max: 1, users: users));

        var cut = Render<MobileLoginPanel>(p => p.Add(x => x.TenantId, TenantId));

        cut.WaitForAssertion(() => cut.Find($"#ml-full-{TenantId}").TextContent.Should().Contain("dolu"));
        cut.FindAll($"#ml-create-{TenantId}").Should().BeEmpty();
    }

    private FakeApi Register(TenantMobileOverviewDto overview)
    {
        var api = new FakeApi(overview);
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(api) { BaseAddress = new Uri("https://centralapi.test/") }, tokenStore));
        return api;
    }

    private static TenantMobileOverviewDto Overview(string status, int max, MobileUserDto[] users) => new()
    {
        TenantId = TenantId,
        TenantCode = status == "none" ? null : "ABCD2345",
        Seats = new SeatUsageDto { Max = max, Used = users.Count(u => u.IsActive), Status = status },
        Users = users,
    };

    private static HttpResponseMessage Json(HttpStatusCode status, object body) =>
        new(status) { Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json") };

    private sealed class FakeApi(TenantMobileOverviewDto overview) : HttpMessageHandler
    {
        private TenantMobileOverviewDto _overview = overview;

        public TenantMobileOverviewDto? OverviewAfterWrite { get; set; }
        public JsonElement? LastSubscription { get; private set; }
        public JsonElement? LastCreatedUser { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            var basePath = $"/api/v1/admin/tenants/{TenantId}/mobile";
            if (request.Method == HttpMethod.Get && path == basePath) return Json(HttpStatusCode.OK, _overview);
            if (request.Method == HttpMethod.Put && path == $"{basePath}/subscription")
            {
                LastSubscription = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
                _overview = OverviewAfterWrite ?? _overview;
                return Json(HttpStatusCode.OK, new SubscriptionDto { Id = Guid.NewGuid(), Seats = 3, IsCurrent = true });
            }
            if (request.Method == HttpMethod.Post && path == $"{basePath}/users")
            {
                LastCreatedUser = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
                _overview = OverviewAfterWrite ?? _overview;
                return Json(HttpStatusCode.Created, new MobileUserDto { Id = Guid.NewGuid(), Username = "patron", FullName = "Firma Sahibi", Role = "ADMIN", IsActive = true });
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
