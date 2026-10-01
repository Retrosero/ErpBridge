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
/// <param name="serverMessage">
/// The server's own wording: used as it is for the few codes whose detail only the server knows, and for a code the
/// panel has no text for when the server worded it in Turkish (<see cref="PortalMessages.For(string, string?)"/>).
/// </param>
public sealed class PortalApiException(string code, int status, string? serverMessage = null) : Exception(serverMessage ?? PortalMessages.For(code))
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

    /// <summary>The central API's address; a catalog image falls back to it when the public catalog host is not set up.</summary>
    public Uri? BaseAddress => http.BaseAddress;

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

    public Task<LedgerResponse> LedgerAsync(string code, DateOnly? from, DateOnly? to, IEnumerable<string> kinds, bool includeVoided, int page, int pageSize, CancellationToken ct = default) =>
        GetAsync<LedgerResponse>("api/v1/portal/customers/ledger" + Query(
            [("code", code), ("from", from is { } f ? Day(f) : null), ("to", to is { } t ? Day(t) : null),
             .. kinds.Select(k => ("kind", (string?)k)),
             ("includeVoided", includeVoided ? "true" : null),
             ("page", page.ToString(CultureInfo.InvariantCulture)), ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))]), ct);

    public Task<CustomerDocumentDto> CustomerDocumentAsync(string code, string key, CancellationToken ct = default) =>
        GetAsync<CustomerDocumentDto>("api/v1/portal/customers/document" + Query(("code", code), ("key", key)), ct);

    /// <summary>Who changed what from the portal (GOAL_PANEL_ERPSIZ E7b); <paramref name="entity"/>+<paramref name="entityKey"/>
    /// gives one card/payment's "Geçmiş" (no date bound), omitting them the company-wide /denetim list (date-bounded).</summary>
    public Task<AuditResponse> AuditAsync(
        string? entity, string? entityKey, DateOnly? from, DateOnly? to, Guid? userId, int page, int pageSize, CancellationToken ct = default) =>
        GetAsync<AuditResponse>("api/v1/portal/native/audit" + Query(
            ("entity", entity), ("key", entityKey), ("from", from is { } f ? Day(f) : null), ("to", to is { } t ? Day(t) : null),
            ("userId", userId?.ToString()), ("page", page.ToString(CultureInfo.InvariantCulture)), ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), ct);

    /// <summary>GOAL_PANEL_ERPSIZ E4e — every customer-side movement of the company; <paramref name="kinds"/> empty means all.</summary>
    public Task<CompanyMovementsResponse> CompanyMovementsAsync(
        DateOnly? from, DateOnly? to, IEnumerable<string> kinds, string? customer, Guid? userId, decimal? minAmount, decimal? maxAmount,
        bool includeVoided, int page, int pageSize, CancellationToken ct = default) =>
        GetAsync<CompanyMovementsResponse>("api/v1/portal/movements" + Query(
            [("from", from is { } f ? Day(f) : null), ("to", to is { } t ? Day(t) : null),
             .. kinds.Select(k => ("kind", (string?)k)),
             ("customer", customer), ("userId", userId?.ToString()),
             ("minAmount", minAmount?.ToString(CultureInfo.InvariantCulture)), ("maxAmount", maxAmount?.ToString(CultureInfo.InvariantCulture)),
             ("includeVoided", includeVoided ? "true" : null),
             ("page", page.ToString(CultureInfo.InvariantCulture)), ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))]), ct);

    /// <summary>Company-wide collections/payments (GOAL_PANEL_ERPSIZ E3c); <paramref name="kinds"/> empty means both.</summary>
    public Task<PaymentsResponse> PaymentsAsync(
        DateOnly? from, DateOnly? to, IEnumerable<string> kinds, string? customer, Guid? userId, int page, int pageSize, CancellationToken ct = default) =>
        GetAsync<PaymentsResponse>("api/v1/portal/payments" + Query(
            [("from", from is { } f ? Day(f) : null), ("to", to is { } t ? Day(t) : null),
             .. kinds.Select(k => ("kind", (string?)k)),
             ("customer", customer), ("userId", userId?.ToString()),
             ("page", page.ToString(CultureInfo.InvariantCulture)), ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))]), ct);

    public Task<StockSearchResponse> StockSearchAsync(StockFilter filter, CancellationToken ct = default) =>
        GetAsync<StockSearchResponse>("api/v1/portal/stock/search" + filter.ToApiQuery(), ct);

    public Task<StockFacetsResponse> StockFacetsAsync(CancellationToken ct = default) =>
        GetAsync<StockFacetsResponse>("api/v1/portal/stock/facets", ct);

    /// <summary>ERP-less tenant only (<see cref="Session.PortalSession.CanEditNativeData"/>). Fetches the card
    /// for the edit form — the list row already has most fields, but not the VAT rate or every price list.</summary>
    public Task<NativeStockCardDetailDto> NativeStockCardAsync(string code, CancellationToken ct = default) =>
        GetAsync<NativeStockCardDetailDto>($"api/v1/portal/native/stock-cards/{Uri.EscapeDataString(code)}", ct);

    /// <summary>GOAL_PANEL_ERPSIZ E1d — one part (≤ 500 cards) of an imported product file.</summary>
    public Task<CardBatchResultDto> ImportNativeStockCardsAsync(NativeStockCardBatchRequest request, CancellationToken ct = default) =>
        SendAsync<CardBatchResultDto>(HttpMethod.Post, "api/v1/portal/native/stock-cards/batch", request, ct);

    /// <summary>GOAL_PANEL_ERPSIZ E2c — one part (≤ 500 cards) of an imported customer file.</summary>
    public Task<CardBatchResultDto> ImportNativeCustomerCardsAsync(NativeCustomerCardBatchRequest request, CancellationToken ct = default) =>
        SendAsync<CardBatchResultDto>(HttpMethod.Post, "api/v1/portal/native/customer-cards/batch", request, ct);
    /// <summary>GOAL_PANEL_ERPSIZ E6a/E6c — one product's movements with the running stock, newest first.</summary>
    public Task<StockMovementsResponse> NativeStockMovementsAsync(
        string code, DateOnly? from, DateOnly? to, bool includeVoided, int page, int pageSize, CancellationToken ct = default) =>
        GetAsync<StockMovementsResponse>($"api/v1/portal/native/stock-cards/{Uri.EscapeDataString(code)}/movements" + Query(
            ("from", from is { } f ? Day(f) : null), ("to", to is { } t ? Day(t) : null), ("includeVoided", includeVoided ? "true" : null),
            ("page", page.ToString(CultureInfo.InvariantCulture)), ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))), ct);

    /// <summary>GOAL_PANEL_ERPSIZ E6c — the product a scanned barcode (or code) names exactly; 404 when none.</summary>
    public Task<NativeStockCardDetailDto> NativeStockCardByBarcodeAsync(string barcode, CancellationToken ct = default) =>
        GetAsync<NativeStockCardDetailDto>($"api/v1/portal/native/barcodes/{Uri.EscapeDataString(barcode)}", ct);

    /// <summary>GOAL_PANEL_ERPSIZ E6b/E6c — the difference to the booked stock becomes a movement.</summary>
    public Task<NativeJobResultDto> PostNativeStockCountAsync(NativeStockCountRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/stock-counts", request, ct);

    /// <summary>GOAL_PANEL_ERPSIZ E6b/E6c — cancels a whole count; <paramref name="key"/> may be one of its movement ids.</summary>
    public Task<NativeJobResultDto> VoidNativeStockCountAsync(string key, NativeLedgerVoidRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, $"api/v1/portal/native/stock-counts/{Uri.EscapeDataString(key)}/void", request, ct);

    public Task<NativeJobResultDto> SaveNativeStockCardAsync(NativeStockCardRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/stock-cards", request, ct);

    public Task<NativeJobResultDto> DeleteNativeStockCardAsync(string code, string operationId, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Delete,
            $"api/v1/portal/native/stock-cards/{Uri.EscapeDataString(code)}" + Query(("operationId", operationId)), null, ct);

    /// <summary>ERP-less tenant only. Create or edit shares this call; the customer is found by its code.</summary>
    public Task<NativeJobResultDto> SaveNativeCustomerCardAsync(NativeCustomerCardRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/customer-cards", request, ct);

    public Task<NativeJobResultDto> RecordNativeCollectionAsync(NativePaymentRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/collections", request, ct);

    public Task<NativeJobResultDto> RecordNativeDisbursementAsync(NativePaymentRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/disbursements", request, ct);

    /// <summary>GOAL_PANEL_ERPSIZ E4a/e — cancels a collection/disbursement/manual adjustment (D2 storno).</summary>
    public Task<NativeJobResultDto> VoidNativeLedgerEntryAsync(string key, NativeLedgerVoidRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, $"api/v1/portal/native/ledger/{Uri.EscapeDataString(key)}/void", request, ct);

    /// <summary>GOAL_PANEL_ERPSIZ E4c/e — void + a corrected re-booking of the same kind, in one transaction (D11).</summary>
    public Task<NativeJobResultDto> EditNativeLedgerEntryAsync(string key, NativeLedgerEditRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, $"api/v1/portal/native/ledger/{Uri.EscapeDataString(key)}/edit", request, ct);

    /// <summary>GOAL_PANEL_ERPSIZ E5b/E5e — company-wide sale/purchase/return invoices; <paramref name="status"/>
    /// is all, active (cancelled ones hidden) or voided.</summary>
    public Task<NativeDocumentsResponse> NativeDocumentsAsync(
        DateOnly? from, DateOnly? to, IEnumerable<string> kinds, string? customer, Guid? userId, string status, int page, int pageSize, CancellationToken ct = default) =>
        GetAsync<NativeDocumentsResponse>("api/v1/portal/native/documents" + Query(
            [("from", from is { } f ? Day(f) : null), ("to", to is { } t ? Day(t) : null),
             .. kinds.Select(k => ("kind", (string?)k)),
             ("customer", customer), ("userId", userId?.ToString()), ("status", status == "all" ? null : status),
             ("page", page.ToString(CultureInfo.InvariantCulture)), ("pageSize", pageSize.ToString(CultureInfo.InvariantCulture))]), ct);

    public Task<NativeDocumentDto> NativeDocumentAsync(string key, CancellationToken ct = default) =>
        GetAsync<NativeDocumentDto>($"api/v1/portal/native/documents/{Uri.EscapeDataString(key)}", ct);

    /// <summary>GOAL_PANEL_ERPSIZ E5a/E5e — a new sale (<c>sale</c>), purchase (<c>purchase</c>) or return (<c>sale_return</c>).</summary>
    public Task<NativeJobResultDto> PostNativeDocumentAsync(string kind, NativeDocumentRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/" + kind switch
        {
            "sale" => "sales-orders",
            "purchase" => "purchase-receipts",
            "sale_return" => "sales-returns",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Only a sale, purchase or return can be entered."),
        }, request, ct);

    /// <summary>GOAL_PANEL_ERPSIZ E5d/E5e — the whole corrected document; the original is cancelled in the same step.</summary>
    public Task<NativeJobResultDto> EditNativeDocumentAsync(string key, NativeDocumentRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, $"api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/edit", request, ct);

    /// <summary>GOAL_PANEL_ERPSIZ E5c/E5e — cancels a whole document (stock and ledger together).</summary>
    public Task<NativeJobResultDto> VoidNativeDocumentAsync(string key, NativeLedgerVoidRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, $"api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/void", request, ct);

    /// <summary>GOAL_PANEL_ERPSIZ E4b/e — a manual correction of a customer's balance; a reason is mandatory.</summary>
    public Task<NativeJobResultDto> PostNativeLedgerAdjustmentAsync(NativeLedgerAdjustmentRequest request, CancellationToken ct = default) =>
        SendAsync<NativeJobResultDto>(HttpMethod.Post, "api/v1/portal/native/ledger-adjustments", request, ct);

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

    /// <summary>Puts a rejected request back in the pending queue; the same call the phone's "Tekrar onaya al" makes.</summary>
    public Task<ApprovalDto> ReopenAsync(Guid requestId, string? note, CancellationToken ct = default) =>
        SendAsync<ApprovalDto>(HttpMethod.Post, $"api/v1/android/approvals/{requestId}/reopen", new { note }, ct);

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

    // ---- permissions (GOAL_YETKILER) ---------------------------------------------------

    public Task<PermissionCatalogDto> PermissionCatalogAsync(CancellationToken ct = default) =>
        GetAsync<PermissionCatalogDto>("api/v1/android/account/permissions/catalog", ct);

    public Task<RolePermissionsResponse> RolePermissionsAsync(CancellationToken ct = default) =>
        GetAsync<RolePermissionsResponse>("api/v1/android/account/roles/permissions", ct);

    public Task<RolePermissionsResponse> SaveRolePermissionsAsync(string role, UpdateRolePermissionsRequest request, CancellationToken ct = default) =>
        SendAsync<RolePermissionsResponse>(HttpMethod.Put, $"api/v1/android/account/roles/{Uri.EscapeDataString(role)}/permissions", request, ct);

    public Task<UserPermissionsDto> UserPermissionsAsync(Guid userId, CancellationToken ct = default) =>
        GetAsync<UserPermissionsDto>($"api/v1/android/account/users/{userId}/permissions", ct);

    public Task<UserPermissionsDto> SaveUserPermissionsAsync(Guid userId, UpdateUserPermissionsRequest request, CancellationToken ct = default) =>
        SendAsync<UserPermissionsDto>(HttpMethod.Put, $"api/v1/android/account/users/{userId}/permissions", request, ct);

    public Task<PermissionChangesResponse> PermissionChangesAsync(Guid? userId = null, CancellationToken ct = default) =>
        GetAsync<PermissionChangesResponse>("api/v1/android/account/permissions/changes" + Query(("userId", userId?.ToString())), ct);

    // ---- route plans (GOAL_HEDEF_RUT P4–P5) --------------------------------------

    public Task<RoutePlansResponse> RoutePlansAsync(CancellationToken ct = default) =>
        GetAsync<RoutePlansResponse>("api/v1/portal/routes", ct);

    public Task<RoutePlanDto> SaveRoutePlanAsync(string planId, RoutePlanDto plan, CancellationToken ct = default) =>
        SendAsync<RoutePlanDto>(HttpMethod.Put, $"api/v1/portal/routes/{Uri.EscapeDataString(planId)}", plan, ct);

    public Task DeleteRoutePlanAsync(string planId, CancellationToken ct = default) =>
        SendAsync<object>(HttpMethod.Delete, $"api/v1/portal/routes/{Uri.EscapeDataString(planId)}", null, ct, emptyOk: true);

    public Task<RouteComplianceResponse> RouteComplianceAsync(DateOnly from, DateOnly to, Guid? teamId, CancellationToken ct = default) =>
        GetAsync<RouteComplianceResponse>("api/v1/portal/routes/compliance" + Query(("from", Day(from)), ("to", Day(to)), ("teamId", teamId?.ToString())), ct);

    // ---- targets and teams (GOAL_HEDEF_RUT) --------------------------------------

    public Task<TeamsResponse> TeamsAsync(CancellationToken ct = default) =>
        GetAsync<TeamsResponse>("api/v1/portal/teams", ct);

    public Task<TeamDto> SaveTeamAsync(Guid? id, TeamSaveRequest request, CancellationToken ct = default) =>
        id is { } existing
            ? SendAsync<TeamDto>(HttpMethod.Put, $"api/v1/portal/teams/{existing}", request, ct)
            : SendAsync<TeamDto>(HttpMethod.Post, "api/v1/portal/teams", request, ct);

    public Task DeleteTeamAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<object>(HttpMethod.Delete, $"api/v1/portal/teams/{id}", null, ct, emptyOk: true);

    public Task<TargetBoardResponse> TargetsAsync(string periodType, string periodKey, Guid? teamId, CancellationToken ct = default) =>
        GetAsync<TargetBoardResponse>("api/v1/portal/targets" + Query(("periodType", periodType), ("periodKey", periodKey), ("teamId", teamId?.ToString())), ct);

    /// <summary>A save the server refuses item by item comes back with <see cref="TargetsSaveResponse.Errors"/> filled, not as an exception.</summary>
    public Task<TargetsSaveResponse> SaveTargetsAsync(TargetsSaveRequest request, CancellationToken ct = default) =>
        SendAsync<TargetsSaveResponse>(HttpMethod.Put, "api/v1/portal/targets", request, ct, errorBodyOk: true);

    public Task<TargetPreviewResponse> CopyTargetsAsync(TargetCopyRequest request, CancellationToken ct = default) =>
        SendAsync<TargetPreviewResponse>(HttpMethod.Post, "api/v1/portal/targets/copy", request, ct);

    public Task<TargetPreviewResponse> DistributeTargetAsync(TargetDistributeRequest request, CancellationToken ct = default) =>
        SendAsync<TargetPreviewResponse>(HttpMethod.Post, "api/v1/portal/targets/distribute", request, ct);

    public Task<TargetItemsResponse> TargetItemsAsync(string metric, string? q, CancellationToken ct = default) =>
        GetAsync<TargetItemsResponse>("api/v1/portal/targets/items" + Query(("metric", metric), ("q", q)), ct);

    public Task<TargetSettingsDto> TargetSettingsAsync(CancellationToken ct = default) =>
        GetAsync<TargetSettingsDto>("api/v1/portal/targets/settings", ct);

    public Task<TargetSettingsDto> SaveTargetSettingsAsync(TargetSettingsDto settings, CancellationToken ct = default) =>
        SendAsync<TargetSettingsDto>(HttpMethod.Put, "api/v1/portal/targets/settings", settings, ct);

    // ---- customer catalog (GOAL_MUSTERI_KATALOGU §5.1) ---------------------------------
    // Shared with the phone. Stock codes, customer codes and category keys go in the query or the body, never the path.

    private const string Catalog = "api/v1/customer-catalog/";

    /// <summary>
    /// Rows per page of the accounts and order lists. The contract sends only <c>page</c>, so the server's page size is
    /// fixed and the pager counts with the same number.
    /// </summary>
    public const int CatalogPageSize = 50;

    public Task<CatalogSettingsDto> CatalogSettingsAsync(CancellationToken ct = default) =>
        GetAsync<CatalogSettingsDto>(Catalog + "settings", ct);

    public Task<CatalogSettingsDto> SaveCatalogSettingsAsync(CatalogSettingsSaveRequest request, CancellationToken ct = default) =>
        SendAsync<CatalogSettingsDto>(HttpMethod.Put, Catalog + "settings", request, ct);

    public Task<CatalogCategoriesDto> CatalogCategoriesAsync(CancellationToken ct = default) =>
        GetAsync<CatalogCategoriesDto>(Catalog + "categories", ct);

    public Task<CatalogRevisionDto> SaveCatalogCategoriesAsync(CatalogCategoriesSaveRequest request, CancellationToken ct = default) =>
        SendAsync<CatalogRevisionDto>(HttpMethod.Put, Catalog + "categories", request, ct);

    /// <summary>A whole category (at most 5000), or with <paramref name="q"/> up to 50 matches across the catalog.</summary>
    public Task<CatalogProductsDto> CatalogProductsAsync(string? category, string? q = null, CancellationToken ct = default) =>
        GetAsync<CatalogProductsDto>(Catalog + "products" + Query(("category", category), ("q", q)), ct);

    public Task<CatalogRevisionDto> SaveCatalogProductsAsync(CatalogProductsSaveRequest request, CancellationToken ct = default) =>
        SendAsync<CatalogRevisionDto>(HttpMethod.Put, Catalog + "products", request, ct);

    public Task<CatalogAccountsDto> CatalogAccountsAsync(string? q, int page, CancellationToken ct = default) =>
        GetAsync<CatalogAccountsDto>(Catalog + "accounts" + Query(("q", q), ("page", page.ToString(CultureInfo.InvariantCulture))), ct);

    public Task<CatalogAccountLookupDto> CatalogAccountByCustomerAsync(string customerCode, CancellationToken ct = default) =>
        GetAsync<CatalogAccountLookupDto>(Catalog + "accounts/by-customer" + Query(("code", customerCode)), ct);

    public Task<CatalogAccountSavedDto> CreateCatalogAccountAsync(CatalogAccountCreateRequest request, CancellationToken ct = default) =>
        SendAsync<CatalogAccountSavedDto>(HttpMethod.Post, Catalog + "accounts", request, ct);

    public Task<CatalogAccountSavedDto> UpdateCatalogAccountAsync(Guid accountId, CatalogAccountPatchRequest request, CancellationToken ct = default) =>
        SendAsync<CatalogAccountSavedDto>(HttpMethod.Patch, $"{Catalog}accounts/{accountId}", request, ct);

    /// <summary>A null <paramref name="password"/> lets the server make one, returned once.</summary>
    public Task<CatalogPasswordDto> SetCatalogAccountPasswordAsync(Guid accountId, string? password, CancellationToken ct = default) =>
        SendAsync<CatalogPasswordDto>(HttpMethod.Put, $"{Catalog}accounts/{accountId}/password", new { password }, ct);

    public Task RevokeCatalogAccountSessionsAsync(Guid accountId, CancellationToken ct = default) =>
        SendAsync<object>(HttpMethod.Post, $"{Catalog}accounts/{accountId}/revoke-sessions", new { }, ct, emptyOk: true);

    public Task DeleteCatalogAccountAsync(Guid accountId, CancellationToken ct = default) =>
        SendAsync<object>(HttpMethod.Delete, $"{Catalog}accounts/{accountId}", null, ct, emptyOk: true);

    public Task<CatalogImageManifestDto> CatalogImageManifestAsync(CancellationToken ct = default) =>
        GetAsync<CatalogImageManifestDto>(Catalog + "images/manifest", ct);

    public async Task<CatalogImageDto> CreateCatalogImageAsync(CatalogImageCreateRequest request, CancellationToken ct = default) =>
        (await SendAsync<CatalogImageCreatedDto>(HttpMethod.Post, Catalog + "images", request, ct)).Image;

    /// <summary>One variant's bytes: <c>l</c> (≤ 1280 px, ≤ 1 MB) or <c>s</c> (≤ 400 px, ≤ 200 KB); JPEG, PNG or WebP.</summary>
    public Task UploadCatalogImageAsync(Guid imageId, string variant, byte[] content, string contentType, CancellationToken ct = default) =>
        PutBytesAsync($"{Catalog}images/{imageId}/{variant}", content, contentType, ct);

    /// <summary>The product's images in this order; the first is the cover.</summary>
    public Task OrderCatalogImagesAsync(string stockCode, IEnumerable<Guid> ids, CancellationToken ct = default) =>
        SendAsync<object>(HttpMethod.Put, Catalog + "images/order" + Query(("stockCode", stockCode)), new { ids = ids.ToArray() }, ct, emptyOk: true);

    public Task DeleteCatalogImageAsync(Guid imageId, CancellationToken ct = default) =>
        SendAsync<object>(HttpMethod.Delete, $"{Catalog}images/{imageId}", null, ct, emptyOk: true);

    /// <summary><paramref name="status"/> <c>NEW</c>, <c>CLAIMED</c>, <c>COMPLETED</c>, <c>REJECTED</c>, or null for all.</summary>
    public Task<CatalogOrdersDto> CatalogOrdersAsync(string? status, string? q, int page, CancellationToken ct = default) =>
        GetAsync<CatalogOrdersDto>(Catalog + "orders" + Query(("status", status), ("q", q), ("page", page.ToString(CultureInfo.InvariantCulture))), ct);

    public Task<CatalogOrderDetailDto> CatalogOrderAsync(Guid orderId, CancellationToken ct = default) =>
        GetAsync<CatalogOrderDetailDto>($"{Catalog}orders/{orderId}", ct);

    /// <summary><paramref name="force"/> takes it from someone else; the server allows that to a catalog manager only.</summary>
    public Task<CatalogOrderDetailDto> ClaimCatalogOrderAsync(Guid orderId, bool force = false, CancellationToken ct = default) =>
        SendAsync<CatalogOrderDetailDto>(HttpMethod.Post, $"{Catalog}orders/{orderId}/claim", new { force }, ct);

    public Task<CatalogOrderDetailDto> ReleaseCatalogOrderAsync(Guid orderId, CancellationToken ct = default) =>
        SendAsync<CatalogOrderDetailDto>(HttpMethod.Post, $"{Catalog}orders/{orderId}/release", new { }, ct);

    /// <summary>"Başka yerde girildi": the order was entered elsewhere, under <paramref name="documentRef"/> if known.</summary>
    public Task<CatalogOrderDetailDto> CompleteCatalogOrderAsync(Guid orderId, string? documentRef, CancellationToken ct = default) =>
        SendAsync<CatalogOrderDetailDto>(HttpMethod.Post, $"{Catalog}orders/{orderId}/complete", new { documentRef }, ct);

    public Task<CatalogOrderDetailDto> RejectCatalogOrderAsync(Guid orderId, string reason, CancellationToken ct = default) =>
        SendAsync<CatalogOrderDetailDto>(HttpMethod.Post, $"{Catalog}orders/{orderId}/reject", new { reason }, ct);

    /// <summary>A closed request (turned into a sale or rejected) back to "Yeni"; its document and reason are cleared. Catalog managers only.</summary>
    public Task<CatalogOrderDetailDto> ReopenCatalogOrderAsync(Guid orderId, CancellationToken ct = default) =>
        SendAsync<CatalogOrderDetailDto>(HttpMethod.Post, $"{Catalog}orders/{orderId}/reopen", new { }, ct);

    /// <summary>What "Siparişe çevir" would send now: today's prices against the request's, whose name, warehouse, missing mapping.</summary>
    public Task<CatalogOrderConversionDto> CatalogOrderConversionAsync(Guid orderId, CancellationToken ct = default) =>
        GetAsync<CatalogOrderConversionDto>($"{Catalog}orders/{orderId}/conversion", ct);

    /// <summary>
    /// Turns the request into a sale (the server builds the phone's document). <paramref name="expectedTotal"/> is the
    /// preview's total: a changed price is <c>PRICE_CHANGED</c>; null <paramref name="warehouseNo"/> keeps the default.
    /// </summary>
    public Task<CatalogOrderConvertResponse> ConvertCatalogOrderAsync(Guid orderId, int? warehouseNo, decimal expectedTotal, CancellationToken ct = default) =>
        SendAsync<CatalogOrderConvertResponse>(HttpMethod.Post, $"{Catalog}orders/{orderId}/convert", new { warehouseNo, expectedTotal }, ct);

    // ---- plumbing ------------------------------------------------------------

    private Task<T> GetAsync<T>(string path, CancellationToken ct) => SendAsync<T>(HttpMethod.Get, path, null, ct);

    /// <summary>Raw bytes with their media type (an image variant); a 204 is the success.</summary>
    private Task PutBytesAsync(string path, byte[] content, string contentType, CancellationToken ct)
    {
        var body = new ByteArrayContent(content);
        body.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return SendAsync<object>(HttpMethod.Put, path, body, ct, emptyOk: true);
    }

    /// <param name="body">Sent as JSON, or as is when it is already <see cref="HttpContent"/>.</param>
    /// <param name="emptyOk">A success without a body (204) is fine; the default value comes back.</param>
    /// <param name="errorBodyOk">A 400/403/409 whose body is a <typeparamref name="T"/> with its own error list comes back as such.</param>
    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct, bool emptyOk = false, bool errorBodyOk = false)
    {
        if (!session.IsSignedIn) throw new SessionEndedException("INVALID_TOKEN");
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        if (body is HttpContent raw) request.Content = raw;
        else if (body is not null) request.Content = JsonContent.Create(body, options: Json);

        using var response = await http.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
            if (emptyOk && (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)) return default!;
            return (await response.Content.ReadFromJsonAsync<T>(Json, ct))!;
        }
        if (errorBodyOk && response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Forbidden or HttpStatusCode.Conflict)
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            if (text.Contains("\"errors\"", StringComparison.Ordinal))
                return JsonSerializer.Deserialize<T>(text, Json)!;
            response.Content = new StringContent(text, System.Text.Encoding.UTF8, "application/json");
        }

        var (code, message) = await ReadErrorWithMessageAsync(response, ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized || SessionEndingCodes.Contains(code))
            throw new SessionEndedException(code);
        throw new PortalApiException(code, (int)response.StatusCode,
            ServerWordedCodes.Contains(code) ? message : PortalMessages.Knows(code) ? null : PortalMessages.For(code, message));
    }

    /// <summary>Codes whose message the server words in Turkish with a detail the panel cannot know (who, which stop).</summary>
    private static readonly HashSet<string> ServerWordedCodes = ["ROUTE_INVALID"];

    private static async Task<(string Code, string? Message)> ReadErrorWithMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorDto>(Json, ct);
            if (!string.IsNullOrWhiteSpace(error?.ErrorCode)) return (error.ErrorCode, string.IsNullOrWhiteSpace(error.Message) ? null : error.Message);
        }
        catch (JsonException)
        {
            // Not an error envelope (proxy page, empty body).
        }
        return (response.StatusCode == HttpStatusCode.Unauthorized ? "INVALID_TOKEN" : $"HTTP_{(int)response.StatusCode}", null);
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
