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

    [Fact]
    public void A_manager_is_created_with_the_approval_rights_the_operator_ticks()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN")]));

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#user-form-role"));
        cut.FindAll("#user-form-can-approve").Should().BeEmpty("rights are offered for managers only");
        cut.Find("#user-form-username").Change("mehmet");
        cut.Find("#user-form-fullname").Change("Mehmet Müdür");
        cut.Find("#user-form-password").Change("parola123");
        cut.Find("#user-form-role").Change("MANAGER");
        cut.Find("#user-form-can-approve").Change(true);
        cut.Find("#user-form-create").Click();

        cut.WaitForAssertion(() => api.LastCreatedUserBody.Should().NotBeNull());
        var body = api.LastCreatedUserBody!.Value;
        body.GetProperty("role").GetString().Should().Be("MANAGER");
        body.GetProperty("canApprove").GetBoolean().Should().BeTrue();
        body.GetProperty("canManageApprovalRules").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public void Shows_the_company_approval_rules_and_requests_read_only()
    {
        var mehmet = User("mehmet", "MANAGER");
        mehmet.CanApprove = true;
        var overview = Overview(max: 3, users: [User("patron", "ADMIN"), mehmet]);
        overview.ApprovalRules = new ApprovalRulesDto { Rules = new() { ["sale"] = true, ["stock_count"] = false }, UpdatedByName = "Patron" };
        var api = Register(overview);
        api.Approvals =
        [
            new ApprovalRequestDto { Id = Guid.NewGuid(), Kind = "sale", CounterpartyName = "Bakkal Ali", Amount = 450, Status = "Pending", RequestedByName = "Ali", RequestedAtUtc = DateTimeOffset.UtcNow },
            new ApprovalRequestDto { Id = Guid.NewGuid(), Kind = "collection", CounterpartyName = "Bakkal Ali", Amount = 100, Status = "Rejected", RequestedByName = "Ali", DecidedByName = "Patron", DecisionNote = "tutar yanlış", RequestedAtUtc = DateTimeOffset.UtcNow },
        ];

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));

        cut.WaitForAssertion(() => cut.FindAll(".approval-row").Count.Should().Be(2));
        cut.Find(".approval-rule[data-kind='sale']").TextContent.Should().Contain("Onaya düşer");
        cut.Find(".approval-rule[data-kind='stock_count']").TextContent.Should().Contain("Doğrudan işlenir");
        cut.FindAll(".approval-row")[1].TextContent.Should().Contain("Reddedildi").And.Contain("tutar yanlış");
        cut.FindAll(".user-row")[1].QuerySelector(".user-rights")!.TextContent.Should().Be("Onay verir");
        cut.FindAll("button").Should().NotContain(b => b.TextContent.Contains("Onayla"), "the console never decides for a company");
    }

    [Fact]
    public void Ticking_the_xml_module_sends_the_whole_module_set()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN")]));

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#module-xml-import"));
        cut.Find("#module-xml-import").HasAttribute("checked").Should().BeFalse();
        cut.Find("#modules-save").HasAttribute("disabled").Should().BeTrue("nothing changed yet");
        cut.Find("#module-xml-import").Change(true);
        cut.Find("#modules-save").Click();

        cut.WaitForAssertion(() => api.LastModulesBody.Should().NotBeNull());
        api.LastModulesBody!.Value.GetProperty("modules").EnumerateArray().Select(m => m.GetString()).Should().Equal("xml_import");
    }

    [Fact]
    public void Choosing_the_change_feed_sends_the_sync_mode()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN")]));

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#sync-mode"));
        cut.Find("#sync-mode").GetAttribute("value").Should().Be("tables");
        cut.Find("#sync-mode-save").HasAttribute("disabled").Should().BeTrue("nothing changed yet");
        cut.Find("#sync-mode").Change("feed");
        cut.Find("#sync-mode-save").Click();

        cut.WaitForAssertion(() => api.LastSyncModeBody.Should().NotBeNull());
        api.LastSyncModeBody!.Value.GetProperty("syncMode").GetString().Should().Be("feed");
    }

    [Fact]
    public void Unticking_the_xml_module_switches_it_off()
    {
        var overview = Overview(max: 3, users: [User("patron", "ADMIN")]);
        overview.Modules = ["xml_import"];
        var api = Register(overview);

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#module-xml-import").HasAttribute("checked").Should().BeTrue());
        cut.Find("#module-xml-import").Change(false);
        cut.Find("#modules-save").Click();

        cut.WaitForAssertion(() => api.LastModulesBody.Should().NotBeNull());
        api.LastModulesBody!.Value.GetProperty("modules").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public void Ticking_the_customer_catalog_sends_it_with_the_modules_already_on()
    {
        var overview = Overview(max: 3, users: [User("patron", "ADMIN")]);
        overview.Modules = ["xml_import"];
        var api = Register(overview);

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#module-customer-catalog"));
        cut.Find("#module-customer-catalog").HasAttribute("checked").Should().BeFalse();
        cut.Find("#module-customer-catalog").ParentElement!.TextContent.Should().Contain("Müşteri kataloğu (web");
        cut.Find("#modules-save").HasAttribute("disabled").Should().BeTrue("nothing changed yet");
        cut.Find("#module-customer-catalog").Change(true);
        cut.Find("#modules-save").Click();

        cut.WaitForAssertion(() => api.LastModulesBody.Should().NotBeNull());
        api.LastModulesBody!.Value.GetProperty("modules").EnumerateArray().Select(m => m.GetString()).Should().Equal("xml_import", "customer_catalog");
    }

    [Fact]
    public void Unticking_the_customer_catalog_switches_only_it_off()
    {
        var overview = Overview(max: 3, users: [User("patron", "ADMIN")]);
        overview.Modules = ["customer_catalog", "xml_import"];
        var api = Register(overview);

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#module-customer-catalog").HasAttribute("checked").Should().BeTrue());
        cut.Find("#module-customer-catalog").Change(false);
        cut.Find("#modules-save").Click();

        cut.WaitForAssertion(() => api.LastModulesBody.Should().NotBeNull());
        api.LastModulesBody!.Value.GetProperty("modules").EnumerateArray().Select(m => m.GetString()).Should().Equal("xml_import");
    }

    [Fact]
    public void Shows_storage_used_over_quota_and_warns_from_eighty_percent()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN")]));
        api.Storage = Storage(used: 9L * StorageTexts.GigaByte / 2, quota: 5L * StorageTexts.GigaByte);
        api.Storage.Areas = [new StorageAreaUsageDto { Area = "product", UsedBytes = 3L * StorageTexts.GigaByte, FileCount = 1200 }, new StorageAreaUsageDto { Area = "task", UsedBytes = 0, FileCount = 0 }];

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));

        cut.WaitForAssertion(() => cut.Find(".storage-usage").TextContent.Should().Be("4,5 GB / 5 GB"));
        cut.Find("#storage-stat").ClassList.Should().Contain("admin-stat--warning");
        cut.Find("#storage-stat").TextContent.Should().Contain("%90 dolu").And.Contain("varsayılan kota");
        cut.FindAll(".storage-area").Select(a => a.GetAttribute("data-area")).Should().Equal("product");
        cut.Find(".storage-area").TextContent.Should().Contain("Ürün fotoğrafı").And.Contain("3 GB").And.Contain("1200 dosya");
        cut.Find("#storage-default").HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void Saving_a_company_quota_sends_bytes_and_the_default_sends_null()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN")]));
        api.Storage = Storage(used: 0, quota: 5L * StorageTexts.GigaByte);

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#storage-default"));
        cut.Find("#storage-default").Change(false);
        cut.Find("#storage-quota-gb").Change("12.5");
        cut.Find("#storage-save").Click();

        cut.WaitForAssertion(() => api.StorageBodies.Should().HaveCount(1));
        api.StorageBodies[0].GetProperty("quotaBytes").GetInt64().Should().Be(25L * StorageTexts.GigaByte / 2);
        cut.WaitForAssertion(() => cut.Find(".seat-notice--success").TextContent.Should().Contain("12,5 GB"));

        cut.WaitForAssertion(() => cut.Find("#storage-default").HasAttribute("checked").Should().BeFalse());
        cut.Find("#storage-default").Change(true);
        cut.Find("#storage-save").Click();
        cut.WaitForAssertion(() => api.StorageBodies.Should().HaveCount(2));
        api.StorageBodies[1].GetProperty("quotaBytes").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void Recount_shows_the_figures_before_and_after()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN")]));
        api.Storage = Storage(used: 2048, quota: 5L * StorageTexts.GigaByte);

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));
        cut.WaitForAssertion(() => cut.Find("#storage-recount"));
        cut.Find("#storage-recount").Click();

        cut.WaitForAssertion(() => cut.Find(".seat-notice--success").TextContent.Should().Contain("2 KB → 1 KB"));
        api.Recounts.Should().Be(1);
    }

    [Fact]
    public void A_server_without_r2_settings_says_uploads_are_off()
    {
        var api = Register(Overview(max: 3, users: [User("patron", "ADMIN")]));
        api.Storage = Storage(used: 0, quota: 5L * StorageTexts.GigaByte);
        api.Storage.Available = false;

        var cut = Render<TenantMobile>(p => p.Add(x => x.TenantId, TenantId));

        cut.WaitForAssertion(() => cut.Find("#storage-stat").TextContent.Should().Contain("Depo ayarlı değil"));
        cut.Find("#storage-empty").Should().NotBeNull();
    }

    // ---- helpers -----------------------------------------------------------

    private static TenantStorageDto Storage(long used, long quota) => new()
    {
        TenantId = TenantId,
        Available = true,
        UsedBytes = used,
        QuotaBytes = quota,
        DefaultQuotaBytes = 5L * StorageTexts.GigaByte,
    };

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
        public JsonElement? LastCreatedUserBody { get; private set; }
        public ApprovalRequestDto[] Approvals { get; set; } = [];
        public JsonElement? LastModulesBody { get; private set; }
        public JsonElement? LastSyncModeBody { get; private set; }
        public TenantStorageDto? Storage { get; set; }
        public List<JsonElement> StorageBodies { get; } = new();
        public int Recounts { get; private set; }

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
            if (request.Method == HttpMethod.Put && path == $"{mobileBase}/sync-mode")
            {
                LastSyncModeBody = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            if (request.Method == HttpMethod.Put && path == $"{mobileBase}/modules")
            {
                LastModulesBody = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            var storageBase = $"/api/v1/admin/tenants/{TenantId}/storage";
            if (path == storageBase && Storage is not null)
            {
                if (request.Method == HttpMethod.Put)
                {
                    var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
                    StorageBodies.Add(body);
                    var quota = body.GetProperty("quotaBytes");
                    Storage.CustomQuotaBytes = quota.ValueKind == JsonValueKind.Null ? null : quota.GetInt64();
                    Storage.QuotaBytes = Storage.CustomQuotaBytes ?? Storage.DefaultQuotaBytes;
                }
                return Json(HttpStatusCode.OK, Storage);
            }
            if (request.Method == HttpMethod.Post && path == $"{storageBase}/recount" && Storage is not null)
            {
                Recounts++;
                var before = Storage.UsedBytes;
                Storage.UsedBytes = 1024;
                return Json(HttpStatusCode.OK, new TenantStorageRecountDto { UsedBytesBefore = before, UsedBytesAfter = 1024, Storage = Storage });
            }
            if (request.Method == HttpMethod.Get && path == $"{mobileBase}/approvals")
                return Json(HttpStatusCode.OK, Approvals);
            if (request.Method == HttpMethod.Post && path == $"{mobileBase}/users")
            {
                LastCreatedUserBody = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
                return Json(HttpStatusCode.Created, new MobileUserDto { Id = Guid.NewGuid(), Username = "mehmet", Role = "MANAGER", IsActive = true });
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
