using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using ErpBridge.Admin.Api;
using ErpBridge.Admin.Auth;
using ErpBridge.Admin.Pages;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Admin.Tests.Pages;

/// <summary>
/// bUnit tests for <see cref="TenantMobile"/> — the operator screen where a
/// payment is recorded as seats and a company's mobile users and phones are
/// managed. The fake handler records every call so the tests can check what
/// the page actually sent to the central API.
/// </summary>
public sealed class TenantMobilePageTests : BunitContext
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void Shows_company_code_seat_usage_subscription_and_users()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN"), User("ali", "SALES")]));

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));

        cut.WaitForAssertion(() => cut.Find(".tenant-code").TextContent.Should().Be("ABCD2345"));
        cut.Find(".seat-usage").TextContent.Should().Be("2 / 3");
        cut.Find(".subscription-status").TextContent.Should().Be("Aktif");
        cut.FindAll(".user-row").Select(r => r.GetAttribute("data-username")).Should().Equal("patron", "ali");
        cut.Find("#user-form-create").Should().NotBeNull("a free seat allows adding a user");
    }

    [Fact]
    public void A_full_company_hides_the_add_user_form_and_explains_why()
    {
        Register(Overview(max: 2, users: [User("patron", "ADMIN"), User("ali", "SALES")]));

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));

        cut.WaitForAssertion(() => cut.Find("#user-form-blocked").TextContent.Should().Contain("Tüm koltuklar dolu"));
        cut.FindAll("#user-form-create").Should().BeEmpty();
    }

    [Fact]
    public void Reducing_seats_below_active_users_shows_the_operator_what_to_do()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN"), User("ali", "SALES")]));
        api.SubscriptionResponse = () => Json(HttpStatusCode.Conflict, new { errorCode = "SEATS_BELOW_ACTIVE_USERS", message = "english" });

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#seat-form-seats"));
        cut.Find("#seat-form-seats").Change("1");
        cut.Find("#seat-form-save").Click();

        cut.WaitForAssertion(() => cut.Find(".seat-notice--error").TextContent.Should().Contain("aktif kullanıcı sayısının altına indirilemez"));
        api.LastSubscriptionBody.Should().NotBeNull();
        api.LastSubscriptionBody!.Value.GetProperty("seats").GetInt32().Should().Be(1);
    }

    [Fact]
    public void Deleting_a_user_needs_a_second_confirming_click()
    {
        var ali = User("ali", "SALES");
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN"), ali]));

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.FindAll(".user-row").Count.Should().Be(2));

        cut.FindAll(".user-row")[1].QuerySelector(".user-delete")!.Click();
        api.DeletedUserIds.Should().BeEmpty("the first click only asks for confirmation");

        cut.Find(".user-delete-confirm").Click();
        cut.WaitForAssertion(() => api.DeletedUserIds.Should().Equal(ali.Id));
    }

    // ---- helpers -----------------------------------------------------------

    private FakeApi Register(TenantMobileOverviewDto overview)
    {
        var api = new FakeApi(overview);
        var tokenStore = new TokenStore();
        Services.AddSingleton(tokenStore);
        Services.AddSingleton(new CentralApiClient(new HttpClient(api) { BaseAddress = new Uri("https://centralapi.test/") }, tokenStore));
        return api;
    }

    private static TenantMobileOverviewDto Overview(int max, MobileUserDto[] users) => new()
    {
        TenantId = TenantId,
        TenantCode = "ABCD2345",
        Seats = new SeatUsageDto { Max = max, Used = users.Count(u => u.IsActive), Status = "active", EndsAtUtc = DateTimeOffset.UtcNow.AddMonths(6) },
        Subscriptions = [new SubscriptionDto { Id = Guid.NewGuid(), Seats = max, Source = "bank_transfer", IsCurrent = true, CreatedAtUtc = DateTimeOffset.UtcNow }],
        Users = users,
    };

    private static MobileUserDto User(string username, string role) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        FullName = username.ToUpperInvariant(),
        Role = role,
        IsActive = true,
        CreatedAtUtc = DateTimeOffset.UtcNow,
    };

    private static HttpResponseMessage Json(HttpStatusCode status, object body) =>
        new(status) { Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json") };

    private sealed class FakeApi : HttpMessageHandler
    {
        private readonly TenantMobileOverviewDto _overview;

        public FakeApi(TenantMobileOverviewDto overview) => _overview = overview;

        public Func<HttpResponseMessage>? SubscriptionResponse { get; set; }
        public JsonElement? LastSubscriptionBody { get; private set; }
        public List<Guid> DeletedUserIds { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            var mobileBase = $"/api/v1/admin/tenants/{TenantId}/mobile";

            if (request.Method == HttpMethod.Get && path == "/api/v1/admin/tenants")
                return Json(HttpStatusCode.OK, new[] { new TenantDto { Id = TenantId, Name = "Acme Gıda", IsActive = true } });
            if (request.Method == HttpMethod.Get && path == mobileBase)
                return Json(HttpStatusCode.OK, _overview);
            if (request.Method == HttpMethod.Put && path == $"{mobileBase}/subscription")
            {
                LastSubscriptionBody = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
                return SubscriptionResponse?.Invoke() ?? Json(HttpStatusCode.OK, _overview.Subscriptions[0]);
            }
            if (request.Method == HttpMethod.Delete && path.StartsWith($"{mobileBase}/users/", StringComparison.Ordinal))
            {
                DeletedUserIds.Add(Guid.Parse(path[(path.LastIndexOf('/') + 1)..]));
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
