using System.Security.Cryptography;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Warehouse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Warehouse TV boards (plan step 7, Faz 49).
///
/// <para><b>Pairing.</b> The TV asks for a six-digit code and a secret (<c>POST /display/pairings</c>, anonymous),
/// shows the code and polls <c>POST /display/pairings/{code}/token</c> with the secret. A manager enters the
/// code and a name in the portal (<c>POST /portal/displays</c>); the TV's next poll gets a <c>scope=display</c>
/// token, once, and the code is deleted. Codes live ten minutes.</para>
///
/// <para><b>Board.</b> The token reads only <c>/display/board</c> and <c>/display/events</c>. Every call checks
/// the device row: a revoked TV (or an inactive company) gets 401 <c>DISPLAY_REVOKED</c> and goes back to its
/// pairing screen; a company without a current subscription gets 403 <c>SUBSCRIPTION_*</c> and the screen waits,
/// still paired. A TV takes no seat.</para>
/// </summary>
public static class DisplayEndpoints
{
    public static readonly TimeSpan PairingLifetime = TimeSpan.FromMinutes(10);

    /// <summary>Cards per column the board receives; the column head still shows the full count.</summary>
    public const int MaxCardsPerColumn = 200;
    public const int MaxNameLength = 80;

    /// <summary>The board's long-poll never outlives a revocation by more than this (the acceptance asks ≤ 1 min).</summary>
    public const int MaxWaitSeconds = 25;

    private static readonly TimeSpan LastSeenResolution = TimeSpan.FromMinutes(1);

    public static IEndpointRouteBuilder MapDisplayEndpoints(this IEndpointRouteBuilder routes)
    {
        var pairing = routes.MapGroup("/api/v1/display/pairings")
            .WithTags("Display")
            .AllowAnonymous()
            .RequireRateLimiting(Program.AnonymousRateLimitPolicy);
        pairing.MapPost("", CreatePairingAsync).WithName("DisplayPairingCreate");
        pairing.MapPost("/{code}/token", TakeTokenAsync).WithName("DisplayPairingToken");

        var board = routes.MapGroup("/api/v1/display")
            .WithTags("Display")
            .RequireAuthorization(Program.DisplayPolicy)
            .RequireRateLimiting(Program.PerDisplayRateLimitPolicy);
        board.MapGet("/board", BoardAsync).WithName("DisplayBoard");
        board.MapGet("/events", EventsAsync).WithName("DisplayEvents");

        var portal = routes.MapGroup("/api/v1/portal/displays")
            .WithTags("Display")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        portal.MapGet("", ListAsync).WithName("PortalDisplaysList");
        portal.MapPost("", PairAsync).WithName("PortalDisplaysPair");
        portal.MapPost("/{id:guid}/revoke", RevokeAsync).WithName("PortalDisplaysRevoke");
        return routes;
    }

    // ---- pairing (the TV) ---------------------------------------------------------------

    private static async Task<IResult> CreatePairingAsync([FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        // The table only ever holds codes of the last ten minutes; clearing old ones frees their numbers.
        // Loaded rather than filtered in SQL: the table is tiny and SQLite cannot compare DateTimeOffset.
        var existing = await db.DisplayPairingCodes.ToListAsync(ct);
        db.DisplayPairingCodes.RemoveRange(existing.Where(c => c.ExpiresAtUtc <= now));
        var taken = existing.Where(c => c.ExpiresAtUtc > now).Select(c => c.Code).ToHashSet(StringComparer.Ordinal);

        string code;
        do code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
        while (taken.Contains(code));

        var secret = JwtIssuer.GenerateRefreshToken();
        db.DisplayPairingCodes.Add(new DisplayPairingCode
        {
            Code = code,
            PairingSecretHash = JwtIssuer.HashRefreshToken(secret),
            ExpiresAtUtc = now.Add(PairingLifetime),
        });
        await db.SaveChangesAsync(ct);
        return JsonResults.Status(StatusCodes.Status201Created, new DisplayPairingResponse { Code = code, Secret = secret, ExpiresAtUtc = now.Add(PairingLifetime) });
    }

    private static async Task<IResult> TakeTokenAsync(string code, [FromBody] DisplayTokenRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] IJwtIssuer jwt, CancellationToken ct)
    {
        var pairing = await db.DisplayPairingCodes.FirstOrDefaultAsync(c => c.Code == code, ct);
        // A wrong secret looks exactly like an unknown code: the six digits alone reveal nothing.
        if (pairing is null || string.IsNullOrEmpty(body?.Secret)
            || !CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.ASCII.GetBytes(pairing.PairingSecretHash),
                System.Text.Encoding.ASCII.GetBytes(JwtIssuer.HashRefreshToken(body.Secret))))
            return Error(404, "PAIRING_NOT_FOUND", "No pairing with this code and secret.");
        if (pairing.DisplayDeviceId is not { } deviceId)
        {
            return pairing.ExpiresAtUtc <= DateTimeOffset.UtcNow
                ? Error(410, "PAIRING_EXPIRED", "The code expired; ask for a new one.")
                : JsonResults.Ok(new DisplayTokenResponse { Status = "waiting" });
        }

        var device = await db.DisplayDevices.Include(d => d.Tenant).FirstOrDefaultAsync(d => d.Id == deviceId, ct);
        db.DisplayPairingCodes.Remove(pairing);
        if (device is null || device.RevokedAtUtc is not null)
        {
            await db.SaveChangesAsync(ct);
            return Error(404, "PAIRING_NOT_FOUND", "No pairing with this code and secret.");
        }
        device.TokenHash = pairing.PairingSecretHash;
        device.LastSeenAtUtc = DateTimeOffset.UtcNow;
        // Deleting the code in the same save makes the token a one-time answer: a second poll finds nothing.
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error(404, "PAIRING_NOT_FOUND", "No pairing with this code and secret.");
        }
        var token = jwt.IssueForDisplay(device.Id, device.TenantId);
        return JsonResults.Ok(new DisplayTokenResponse
        {
            Status = "paired",
            Token = token.Token,
            TenantName = device.Tenant?.Name ?? string.Empty,
            DisplayName = device.Name,
        });
    }

    // ---- the board (the TV) ----------------------------------------------------------------

    private static async Task<IResult> BoardAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (device, error) = await AuthorizeDisplayAsync(http, db, ct);
        if (error is not null) return error;
        var cursor = await FulfillmentService.LatestSeqAsync(db, device!.TenantId, ct);
        var items = new List<FulfillmentDto>();
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        // Per status: a long pending queue must not hide what is being prepared or waits on the ramp (Codex, PR #61).
        foreach (var status in FulfillmentStatuses.Open)
        {
            var open = db.OrderFulfillments.AsNoTracking().Where(f => f.TenantId == device.TenantId && f.Status == status);
            counts[status] = await open.CountAsync(ct);
            items.AddRange((await open.OrderBy(f => f.QueuedSeq).Take(MaxCardsPerColumn).ToListAsync(ct)).Select(FulfillmentService.ToDto));
        }
        return JsonResults.Ok(new DisplayBoardResponse
        {
            TenantName = device.Tenant?.Name ?? string.Empty,
            DisplayName = device.Name,
            Settings = await FulfillmentService.SettingsAsync(db, device.TenantId, ct),
            LatestSeq = cursor,
            ServerTimeUtc = DateTimeOffset.UtcNow,
            Items = [.. items],
            Counts = counts,
        });
    }

    /// <summary>
    /// Long-poll on the company's warehouse queue: at once when it moved past <c>sinceSeq</c>, otherwise after
    /// up to <c>wait</c> seconds (0-25). Revocation is checked on entry, so a revoked TV learns it within one wait.
    /// </summary>
    private static async Task<IResult> EventsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] ITenantEventHub events,
        long? sinceSeq, int? wait, CancellationToken ct)
    {
        var (device, error) = await AuthorizeDisplayAsync(http, db, ct);
        if (error is not null) return error;
        var since = sinceSeq ?? 0;
        var seconds = Math.Clamp(wait ?? 0, 0, MaxWaitSeconds);
        var waiting = seconds > 0 ? events.WaitAsync(device!.TenantId, TimeSpan.FromSeconds(seconds), http.RequestAborted) : null;
        var latest = await FulfillmentService.LatestSeqAsync(db, device!.TenantId, ct);
        if (latest <= since && waiting is not null)
        {
            await waiting;
            latest = await FulfillmentService.LatestSeqAsync(db, device.TenantId, ct);
        }
        return JsonResults.Ok(new PortalEventsResponse { LatestSeq = latest, Changed = latest > since });
    }

    // ---- the portal (managers) -----------------------------------------------------------

    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        var devices = await db.DisplayDevices.AsNoTracking().Where(d => d.TenantId == tenant!.Id).OrderBy(d => d.Name).ToListAsync(ct);
        return JsonResults.Ok(devices.Select(ToDto).ToArray());
    }

    private static async Task<IResult> PairAsync(HttpContext http, [FromBody] PairDisplayRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, user, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        var name = body?.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > MaxNameLength)
            return Error(400, "INVALID_DISPLAY_NAME", $"name is required (at most {MaxNameLength} characters).");
        var code = body?.Code?.Replace(" ", string.Empty, StringComparison.Ordinal).Trim();

        var pairing = string.IsNullOrEmpty(code) ? null : await db.DisplayPairingCodes.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code, ct);
        if (pairing is null || pairing.DisplayDeviceId is not null || pairing.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            return NoWaitingScreen();

        var now = DateTimeOffset.UtcNow;
        var device = new DisplayDevice
        {
            TenantId = tenant!.Id,
            Name = name,
            TokenHash = pairing.PairingSecretHash,
            CreatedByUserId = user!.Id,
            CreatedAtUtc = now,
        };
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        // One claim per code: two managers entering the same code at once must not both create a screen
        // (the loser's would never connect). The WHERE on the empty device is the lock (Codex, PR #61).
        var claimed = db.Database.IsRelational()
            ? await db.DisplayPairingCodes.Where(c => c.Code == pairing.Code && c.DisplayDeviceId == null)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.DisplayDeviceId, device.Id).SetProperty(c => c.ClaimedAtUtc, now), ct)
            : await ClaimTrackedAsync(db, pairing.Code, device.Id, now, ct);
        if (claimed == 0) return NoWaitingScreen();
        db.DisplayDevices.Add(device);
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return JsonResults.Status(StatusCodes.Status201Created, ToDto(device));
    }

    private static async Task<IResult> RevokeAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] ITenantEventHub events, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeManagerAsync(http, db, ct);
        if (error is not null) return error;
        var device = await db.DisplayDevices.FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenant!.Id, ct);
        if (device is null) return Error(404, "DISPLAY_NOT_FOUND", "Display not found.");
        device.RevokedAtUtc ??= DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        // Wake the boards so a revoked one hears about it now rather than at the end of its wait.
        events.Publish(tenant!.Id, TenantEventTopics.Warehouse);
        return JsonResults.Ok(ToDto(device));
    }

    // ---- helpers ----------------------------------------------------------------------------

    private static IResult NoWaitingScreen() =>
        Error(404, "PAIRING_NOT_FOUND", "No TV is waiting with this code. Check the code on the screen; it changes every ten minutes.");

    /// <summary>The in-memory test host has no conditional update; the claim is a plain tracked change there.</summary>
    private static async Task<int> ClaimTrackedAsync(CentralApiDbContext db, string code, Guid deviceId, DateTimeOffset now, CancellationToken ct)
    {
        var row = await db.DisplayPairingCodes.FirstOrDefaultAsync(c => c.Code == code && c.DisplayDeviceId == null, ct);
        if (row is null) return 0;
        row.DisplayDeviceId = deviceId;
        row.ClaimedAtUtc = now;
        return 1;
    }

    private static async Task<(DisplayDevice? Device, IResult? Error)> AuthorizeDisplayAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        if (!Guid.TryParse(http.User.FindFirst("sub")?.Value, out var deviceId) || !http.User.TryGetTenantId(out var tenantId))
            return (null, Error(401, "DISPLAY_REVOKED", "This screen is no longer paired."));
        var device = await db.DisplayDevices.Include(d => d.Tenant)
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.TenantId == tenantId, ct);
        if (device is null || device.RevokedAtUtc is not null || device.Tenant is not { IsActive: true })
            return (null, Error(401, "DISPLAY_REVOKED", "This screen is no longer paired."));
        // Same rule as the company's users: no board without a current subscription (Codex, PR #61). 403 rather
        // than 401 — the screen keeps its pairing and comes back once the subscription is renewed.
        var seats = http.RequestServices.GetRequiredService<ErpBridge.CentralApi.Mobile.MobileSeatService>();
        var status = ErpBridge.CentralApi.Mobile.MobileSeatService.SubscriptionStatus(await seats.GetCurrentSubscriptionAsync(tenantId, ct), DateTimeOffset.UtcNow);
        if (!ErpBridge.CentralApi.Mobile.MobileSeatService.AllowsWork(status))
            return (null, Error(403, status == "none" ? "SUBSCRIPTION_REQUIRED" : "SUBSCRIPTION_EXPIRED", "The company has no active subscription."));

        var now = DateTimeOffset.UtcNow;
        if (device.LastSeenAtUtc is not { } seen || now - seen >= LastSeenResolution)
        {
            device.LastSeenAtUtc = now;
            await db.SaveChangesAsync(ct);
        }
        return (device, null);
    }

    private static async Task<(Tenant? Tenant, MobileUser? User, IResult? Error)> AuthorizeManagerAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access;
        if (!RolePermissions.CanManageWarehouse(access.User!))
            return (null, null, Error(403, "WAREHOUSE_MANAGER_REQUIRED", "Only administrators and managers pair and revoke screens."));
        return access;
    }

    private static DisplayDeviceDto ToDto(DisplayDevice d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        CreatedAtUtc = d.CreatedAtUtc,
        LastSeenAtUtc = d.LastSeenAtUtc,
        RevokedAtUtc = d.RevokedAtUtc,
    };

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
