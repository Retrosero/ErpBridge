using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.Portal.Session;

namespace ErpBridge.Portal.Api;

/// <summary>The session ended on the server (token expired, user disabled, device blocked): sign in again.</summary>
public sealed class SessionEndedException(string code) : Exception(PortalMessages.For(code))
{
    public string Code { get; } = code;
}

/// <summary>The central API refused the call with an error envelope.</summary>
public sealed class PortalApiException(string code, int status) : Exception(PortalMessages.For(code))
{
    public string Code { get; } = code;
    public int Status { get; } = status;
}

/// <summary>
/// Calls the central API with the token of the circuit's <see cref="PortalSession"/>.
/// The session is scoped, so every call carries the signed-in manager's own token.
/// </summary>
public sealed class PortalApiClient(HttpClient http, PortalSession session)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Codes after which the session cannot continue.</summary>
    private static readonly HashSet<string> SessionEndingCodes =
    [
        "INVALID_TOKEN", "SESSION_REVOKED", "USER_INACTIVE", "DEVICE_REVOKED",
        "TENANT_INACTIVE", "SUBSCRIPTION_EXPIRED", "SUBSCRIPTION_REQUIRED",
    ];

    /// <summary>
    /// One device row per portal user, so an operator can block portal access like a phone
    /// without the login piling up a new device every time.
    /// </summary>
    public static string DeviceIdFor(string username) => "web-portal:" + username.Trim().ToLowerInvariant();

    public async Task<LoginResponse> LoginAsync(string tenantCode, string username, string password, bool rememberMe = false, CancellationToken ct = default)
    {
        var body = new LoginRequest
        {
            TenantCode = tenantCode.Trim(),
            Username = username.Trim(),
            Password = password,
            DeviceId = DeviceIdFor(username),
            AppVersion = "portal",
            RememberMe = rememberMe,
        };
        using var response = await http.PostAsJsonAsync("api/v1/android/account/login", body, Json, ct);
        if (!response.IsSuccessStatusCode)
        {
            // A wrong password is not an ended session; show the message on the form.
            var error = await ReadErrorAsync(response, ct);
            throw new PortalApiException(error, (int)response.StatusCode);
        }
        return (await response.Content.ReadFromJsonAsync<LoginResponse>(Json, ct))!;
    }

    /// <summary>The signed-in user as the server sees them now: roles an administrator changed take effect.</summary>
    public Task<SessionDto> MeAsync(CancellationToken ct = default) =>
        GetAsync<SessionDto>("api/v1/android/account/me", ct);

    public Task<SummaryResponse> SummaryAsync(DateOnly day, CancellationToken ct = default) =>
        GetAsync<SummaryResponse>($"api/v1/portal/summary?date={Day(day)}", ct);

    public Task<ActivityResponse> ActivityAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
        GetAsync<ActivityResponse>($"api/v1/portal/activity?from={Day(from)}&to={Day(to)}", ct);

    public Task<VisitsResponse> VisitsAsync(DateOnly day, CancellationToken ct = default) =>
        GetAsync<VisitsResponse>($"api/v1/portal/visits?date={Day(day)}", ct);

    public Task<BalancesResponse> BalancesAsync(string? search, CancellationToken ct = default) =>
        GetAsync<BalancesResponse>("api/v1/portal/balances" + Query(("search", search)), ct);

    public Task<CustomersResponse> CustomersAsync(string? search, string balance, string sort, bool descending, int page, int pageSize, CancellationToken ct = default) =>
        GetAsync<CustomersResponse>("api/v1/portal/customers" + Query(
            ("q", search), ("balance", balance == "all" ? null : balance), ("sort", sort), ("dir", descending ? "desc" : "asc"),
            ("page", page.ToString(CultureInfo.InvariantCulture)), ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), ct);

    public Task<CustomerCardDto> CustomerCardAsync(string code, CancellationToken ct = default) =>
        GetAsync<CustomerCardDto>("api/v1/portal/customers/card" + Query(("code", code)), ct);

    public Task<LedgerResponse> LedgerAsync(string code, DateOnly? from, DateOnly? to, IEnumerable<string> kinds, int page, int pageSize, CancellationToken ct = default) =>
        GetAsync<LedgerResponse>("api/v1/portal/customers/ledger" + Query(
            [("code", code), ("from", from is { } f ? Day(f) : null), ("to", to is { } t ? Day(t) : null),
             .. kinds.Select(k => ("kind", (string?)k)),
             ("page", page.ToString(CultureInfo.InvariantCulture)), ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))]), ct);

    public Task<CustomerDocumentDto> CustomerDocumentAsync(string code, string key, CancellationToken ct = default) =>
        GetAsync<CustomerDocumentDto>("api/v1/portal/customers/document" + Query(("code", code), ("key", key)), ct);

    public Task<StockSearchResponse> StockSearchAsync(StockFilter filter, CancellationToken ct = default) =>
        GetAsync<StockSearchResponse>("api/v1/portal/stock/search" + filter.ToApiQuery(), ct);

    public Task<StockFacetsResponse> StockFacetsAsync(CancellationToken ct = default) =>
        GetAsync<StockFacetsResponse>("api/v1/portal/stock/facets", ct);

    /// <summary>ERP-less tenant only (<see cref="Session.PortalSession.CanEditNativeData"/>). Fetches the card
    /// for the edit form — the list row already has most fields, but not the VAT rate or every price list.</summary>
    public Task<NativeStockCardDetailDto> NativeStockCardAsync(string code, CancellationToken ct = default) =>
        GetAsync<NativeStockCardDetailDto>($"api/v1/portal/native/stock-cards/{Uri.EscapeDataString(code)}", ct);

    public Task<NativeJobResultDto> SaveNativeStockCardAsync(NativeStockCardRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/stock-cards", request, ct);

    public Task<NativeJobResultDto> DeleteNativeStockCardAsync(string code, string operationId, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Delete,
            $"api/v1/portal/native/stock-cards/{Uri.EscapeDataString(code)}" + Query(("operationId", operationId)), null, ct);

    /// <summary>ERP-less tenant only. Create or edit shares this call; the customer is found by its code.</summary>
    public Task<NativeJobResultDto> SaveNativeCustomerCardAsync(NativeCustomerCardRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/customer-cards", request, ct);

    /// <summary>One page of requests, newest first; <paramref name="after"/> is the last request on screen.</summary>
    public Task<ApprovalDto[]> ApprovalsAsync(string status, string? kind, ApprovalDto? after, int take, CancellationToken ct = default) =>
        GetAsync<ApprovalDto[]>("api/v1/android/approvals" + Query(
            ("status", status), ("kind", kind),
            ("beforeSeq", after?.RequestedSeq.ToString(CultureInfo.InvariantCulture)), ("beforeExternalId", after?.ExternalId),
            ("take", take.ToString(CultureInfo.InvariantCulture))), ct);

    public Task<ApprovalSummaryDto> ApprovalSummaryAsync(CancellationToken ct = default) =>
        GetAsync<ApprovalSummaryDto>("api/v1/android/approvals/summary", ct);

    /// <summary>
    /// The approval desk's queue, oldest first, up to the server's 500: with more pending, the requests
    /// waiting longest are the ones on the page.
    /// </summary>
    public Task<ApprovalDto[]> ApprovalQueueAsync(CancellationToken ct = default) =>
        GetAsync<ApprovalDto[]>("api/v1/android/approvals?status=pending&order=oldest&take=500", ct);

    public Task<ApprovalDetailDto> ApprovalDetailAsync(Guid requestId, CancellationToken ct = default) =>
        GetAsync<ApprovalDetailDto>($"api/v1/android/approvals/{requestId}", ct);

    /// <summary>
    /// Long-poll for approval changes after <paramref name="approvalsVersion"/> (-1: none seen yet): answers at
    /// once when the version differs, otherwise after up to <paramref name="waitSeconds"/> (the server caps it at
    /// 25, below the client timeout).
    /// </summary>
    public Task<PortalEventsDto> ApprovalEventsAsync(long approvalsVersion, int waitSeconds, CancellationToken ct = default) =>
        GetAsync<PortalEventsDto>($"api/v1/portal/events?approvalsVersion={approvalsVersion}&wait={waitSeconds}", ct);

    public Task<ApprovalDto> DecideAsync(Guid requestId, bool approve, string? note, CancellationToken ct = default) =>
        SendAsync<ApprovalDto>(HttpMethod.Post, $"api/v1/android/approvals/{requestId}/{(approve ? "approve" : "reject")}", new { note }, ct);

    public Task<DisplayDeviceDto[]> DisplaysAsync(CancellationToken ct = default) =>
        GetAsync<DisplayDeviceDto[]>("api/v1/portal/displays", ct);

    public Task<DisplayDeviceDto> PairDisplayAsync(string code, string name, CancellationToken ct = default) =>
        SendAsync<DisplayDeviceDto>(HttpMethod.Post, "api/v1/portal/displays", new { code, name }, ct);

    public Task<DisplayDeviceDto> RevokeDisplayAsync(Guid displayId, CancellationToken ct = default) =>
        SendAsync<DisplayDeviceDto>(HttpMethod.Post, $"api/v1/portal/displays/{displayId}/revoke", new { }, ct);

    /// <summary><paramref name="status"/>: <c>open</c> (pending, preparing, packed), <c>all</c> or a comma-separated list.</summary>
    public Task<FulfillmentListDto> FulfillmentsAsync(string status, bool newest = false, int take = 500, CancellationToken ct = default) =>
        GetAsync<FulfillmentListDto>("api/v1/portal/fulfillments" + Query(
            ("status", status), ("take", take.ToString(CultureInfo.InvariantCulture)), ("newest", newest ? "true" : null)), ct);

    public Task<WarehouseDashboardDto> WarehouseDashboardAsync(DateOnly day, CancellationToken ct = default) =>
        GetAsync<WarehouseDashboardDto>($"api/v1/portal/warehouse/dashboard?date={Day(day)}", ct);

    public Task<WarehousePerformanceDto> WarehousePerformanceAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
        GetAsync<WarehousePerformanceDto>($"api/v1/portal/warehouse/performance?from={Day(from)}&to={Day(to)}", ct);

    public Task<FulfillmentDetailDto> FulfillmentDetailAsync(Guid id, CancellationToken ct = default) =>
        GetAsync<FulfillmentDetailDto>($"api/v1/portal/fulfillments/{id}", ct);

    /// <summary>start, pack, load, undo, cancel or reassign.</summary>
    public Task<FulfillmentDto> FulfillmentActionAsync(Guid id, string action, string? note = null, string? vehiclePlate = null, Guid? assigneeUserId = null, CancellationToken ct = default) =>
        SendAsync<FulfillmentDto>(HttpMethod.Post, $"api/v1/portal/fulfillments/{id}/{action}", new { note, vehiclePlate, assigneeUserId }, ct);

    public Task<PortalEventsDto> WarehouseEventsAsync(long sinceSeq, int waitSeconds, CancellationToken ct = default) =>
        GetAsync<PortalEventsDto>($"api/v1/portal/events?sinceSeq={sinceSeq}&wait={waitSeconds}", ct);

    public Task<WarehouseBackfillDto> WarehouseBackfillAsync(int days, CancellationToken ct = default) =>
        SendAsync<WarehouseBackfillDto>(HttpMethod.Post, "api/v1/portal/warehouse/backfill", new { days }, ct);

    public Task<WarehouseSettingsDto> WarehouseSettingsAsync(CancellationToken ct = default) =>
        GetAsync<WarehouseSettingsDto>("api/v1/portal/warehouse/settings", ct);

    public Task<WarehouseSettingsDto> SaveWarehouseSettingsAsync(WarehouseSettingsDto settings, CancellationToken ct = default) =>
        SendAsync<WarehouseSettingsDto>(HttpMethod.Put, "api/v1/portal/warehouse/settings", settings, ct);

    public Task<ErpWriteSettingsDto> ErpSettingsAsync(CancellationToken ct = default) =>
        GetAsync<ErpWriteSettingsDto>("api/v1/portal/erp-settings", ct);

    public Task<ErpWriteSettingsDto> SaveErpSettingsAsync(ErpWriteSettingsDto settings, CancellationToken ct = default) =>
        SendAsync<ErpWriteSettingsDto>(HttpMethod.Put, "api/v1/portal/erp-settings", settings, ct);

    public Task<UserErpMappingDto> UserErpMappingAsync(Guid userId, CancellationToken ct = default) =>
        GetAsync<UserErpMappingDto>($"api/v1/portal/users/{userId}/erp-mapping", ct);

    public Task<UserErpMappingDto> SaveUserErpMappingAsync(Guid userId, UserErpMappingDto mapping, CancellationToken ct = default) =>
        SendAsync<UserErpMappingDto>(HttpMethod.Put, $"api/v1/portal/users/{userId}/erp-mapping", mapping, ct);

    public Task<ErpLookupsResponse> ErpLookupsAsync(CancellationToken ct = default) =>
        GetAsync<ErpLookupsResponse>("api/v1/portal/erp-lookups", ct);

    public Task<ErpDocumentsResponse> ErpDocumentsAsync(
        DateOnly from, DateOnly to, string? state, string? documentType, string? customer, int page, CancellationToken ct = default) =>
        GetAsync<ErpDocumentsResponse>("api/v1/portal/erp-documents" + Query(
            ("from", Day(from)), ("to", Day(to)), ("state", state), ("documentType", documentType), ("customer", customer),
            ("page", page.ToString(CultureInfo.InvariantCulture))), ct);

    public Task<ErpRetryResponse> RetryErpDocumentAsync(Guid jobId, CancellationToken ct = default) =>
        SendAsync<ErpRetryResponse>(HttpMethod.Post, $"api/v1/portal/erp-documents/{jobId}/retry", new { }, ct);

    public Task<UserListResponse> UsersAsync(CancellationToken ct = default) =>
        GetAsync<UserListResponse>("api/v1/android/account/users", ct);

    public Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default) =>
        SendAsync<UserDto>(HttpMethod.Post, "api/v1/android/account/users", request, ct);

    public Task<UserDto> SetUserActiveAsync(Guid userId, bool active, CancellationToken ct = default) =>
        SendAsync<UserDto>(HttpMethod.Patch, $"api/v1/android/account/users/{userId}", new { isActive = active }, ct);

    public Task<UserDto> SetUserRolesAsync(Guid userId, UpdateUserRolesRequest request, CancellationToken ct = default) =>
        SendAsync<UserDto>(HttpMethod.Patch, $"api/v1/android/account/users/{userId}", request, ct);

    // ---- plumbing ------------------------------------------------------------

    private Task<T> GetAsync<T>(string path, CancellationToken ct) => SendAsync<T>(HttpMethod.Get, path, null, ct);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        if (!session.IsSignedIn) throw new SessionEndedException("INVALID_TOKEN");
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        if (body is not null) request.Content = JsonContent.Create(body, options: Json);

        using var response = await http.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<T>(Json, ct))!;

        var code = await ReadErrorAsync(response, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized || SessionEndingCodes.Contains(code))
            throw new SessionEndedException(code);
        throw new PortalApiException(code, (int)response.StatusCode);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(Json, ct);
            if (!string.IsNullOrWhiteSpace(error?.ErrorCode)) return error.ErrorCode;
        }
        catch (JsonException)
        {
            // Not an error envelope (proxy page, empty body).
        }
        return response.StatusCode == HttpStatusCode.Unauthorized ? "INVALID_TOKEN" : $"HTTP_{(int)response.StatusCode}";
    }

    private static string Day(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Query(params (string Name, string? Value)[] values)
    {
        var parts = values.Where(v => !string.IsNullOrWhiteSpace(v.Value))
            .Select(v => $"{v.Name}={Uri.EscapeDataString(v.Value!.Trim())}")
            .ToList();
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }
}
