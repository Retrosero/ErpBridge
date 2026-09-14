using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Mobile;

/// <summary>Outcome of a seat operation: a value, or an HTTP status with a stable error code.</summary>
public sealed record SeatResult<T>(T? Value, int StatusCode, ApiError? Error)
{
    public static SeatResult<T> Ok(T value) => new(value, StatusCodes.Status200OK, null);

    public static SeatResult<T> Fail(int status, string code, string message) =>
        new(default, status, new ApiError { ErrorCode = code, Message = message });

    public bool Succeeded => Error is null;
}

/// <summary>
/// The single owner of the seat rules. Both the phone (tenant administrators)
/// and the admin console (operators, after a payment) go through this class,
/// so the rules cannot drift apart:
/// <list type="bullet">
/// <item>One active, non-deleted user is one seat — administrators included.</item>
/// <item>A user can be created or re-activated only while a seat is free and
///   the subscription is active or in its grace period.</item>
/// <item>Seats cannot be reduced below the number of active users; someone is
///   deactivated or deleted first.</item>
/// <item>A tenant always keeps at least one active administrator.</item>
/// </list>
/// Every seat-changing method runs in a transaction that first bumps
/// <see cref="Tenant.SeatLockVersion"/>; the UPDATE holds the tenant row lock,
/// so concurrent requests are serialized and cannot both take the last seat.
/// </summary>
public sealed partial class MobileSeatService
{
    /// <summary>Days after <see cref="TenantSubscription.EndsAtUtc"/> during which users can still work.</summary>
    public const int GraceDays = 7;

    public const int MinPasswordLength = 6;

    /// <summary>BCrypt only hashes the first 72 bytes; longer passwords would silently collide.</summary>
    public const int MaxPasswordLength = 72;

    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly CentralApiDbContext _db;

    public MobileSeatService(CentralApiDbContext db) => _db = db;

    /// <summary><c>active</c>, <c>grace</c>, <c>expired</c> or <c>none</c> at <paramref name="now"/>.</summary>
    public static string SubscriptionStatus(TenantSubscription? subscription, DateTimeOffset now)
    {
        if (subscription is null) return "none";
        if (subscription.EndsAtUtc is not { } ends || now <= ends) return "active";
        return now <= ends.AddDays(GraceDays) ? "grace" : "expired";
    }

    /// <summary>True when users of the tenant may sign in and be added.</summary>
    public static bool AllowsWork(string status) => status is "active" or "grace";

    public async Task<TenantSubscription?> GetCurrentSubscriptionAsync(Guid tenantId, CancellationToken ct) =>
        await _db.TenantSubscriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsCurrent, ct);

    public async Task<SeatUsageDto> GetUsageAsync(Guid tenantId, CancellationToken ct)
    {
        var subscription = await GetCurrentSubscriptionAsync(tenantId, ct);
        var used = await CountSeatsInUseAsync(tenantId, ct);
        return ToUsage(subscription, used, DateTimeOffset.UtcNow);
    }

    public static SeatUsageDto ToUsage(TenantSubscription? subscription, int used, DateTimeOffset now) => new()
    {
        Max = subscription?.Seats ?? 0,
        Used = used,
        EndsAtUtc = subscription?.EndsAtUtc,
        Status = SubscriptionStatus(subscription, now),
    };

    public async Task<SeatResult<MobileUser>> CreateUserAsync(Guid tenantId, CreateMobileUserRequest body, CancellationToken ct)
    {
        var username = NormalizeUsername(body.Username);
        if (username is null)
            return SeatResult<MobileUser>.Fail(400, "INVALID_USERNAME", "username must be 3-64 characters: letters, digits, '.', '_' or '-'.");
        var fullName = body.FullName?.Trim();
        if (string.IsNullOrEmpty(fullName) || fullName.Length > 120)
            return SeatResult<MobileUser>.Fail(400, "INVALID_FULL_NAME", "fullName is required and must be at most 120 characters.");
        if (ValidatePassword(body.Password) is { } passwordError)
            return SeatResult<MobileUser>.Fail(400, "INVALID_PASSWORD", passwordError);
        var role = string.IsNullOrWhiteSpace(body.Role) ? MobileUserRoles.Sales : body.Role.Trim().ToUpperInvariant();
        if (!MobileUserRoles.IsValid(role))
            return SeatResult<MobileUser>.Fail(400, "INVALID_ROLE", "role must be ADMIN, MANAGER or SALES.");

        await using var transaction = await BeginSeatTransactionAsync(ct);
        if (!await LockTenantAsync(tenantId, ct))
            return SeatResult<MobileUser>.Fail(404, "TENANT_NOT_FOUND", "Tenant not found.");

        if (await SeatGateAsync(tenantId, ct) is { } gate) return SeatResult<MobileUser>.Fail(gate.Status, gate.Code, gate.Message);

        if (await _db.MobileUsers.AnyAsync(u => u.TenantId == tenantId && u.Username == username && u.DeletedAtUtc == null, ct))
            return SeatResult<MobileUser>.Fail(409, "USERNAME_TAKEN", "A user with this username already exists.");

        var now = DateTimeOffset.UtcNow;
        var user = new MobileUser
        {
            TenantId = tenantId,
            Username = username,
            FullName = fullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password),
            Role = role,
            CanApprove = role == MobileUserRoles.Manager && body.CanApprove == true,
            CanManageApprovalRules = role == MobileUserRoles.Manager && body.CanManageApprovalRules == true,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        _db.MobileUsers.Add(user);
        await _db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return SeatResult<MobileUser>.Ok(user);
    }

    public async Task<SeatResult<MobileUser>> UpdateUserAsync(Guid tenantId, Guid userId, UpdateMobileUserRequest body, CancellationToken ct)
    {
        string? fullName = null;
        if (body.FullName is not null)
        {
            fullName = body.FullName.Trim();
            if (fullName.Length is 0 or > 120)
                return SeatResult<MobileUser>.Fail(400, "INVALID_FULL_NAME", "fullName must be 1-120 characters.");
        }
        if (body.Password is not null && ValidatePassword(body.Password) is { } passwordError)
            return SeatResult<MobileUser>.Fail(400, "INVALID_PASSWORD", passwordError);
        string? role = null;
        if (body.Role is not null)
        {
            role = body.Role.Trim().ToUpperInvariant();
            if (!MobileUserRoles.IsValid(role))
                return SeatResult<MobileUser>.Fail(400, "INVALID_ROLE", "role must be ADMIN, MANAGER or SALES.");
        }

        await using var transaction = await BeginSeatTransactionAsync(ct);
        if (!await LockTenantAsync(tenantId, ct))
            return SeatResult<MobileUser>.Fail(404, "TENANT_NOT_FOUND", "Tenant not found.");

        var user = await _db.MobileUsers.FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.DeletedAtUtc == null, ct);
        if (user is null)
            return SeatResult<MobileUser>.Fail(404, "USER_NOT_FOUND", "Mobile user not found.");

        var activating = body.IsActive == true && !user.IsActive;
        if (activating && await SeatGateAsync(tenantId, ct) is { } gate)
            return SeatResult<MobileUser>.Fail(gate.Status, gate.Code, gate.Message);

        var losesAdmin = user.IsActive && user.Role == MobileUserRoles.Admin
            && (body.IsActive == false || (role is not null && role != MobileUserRoles.Admin));
        if (losesAdmin && !await HasOtherActiveAdminAsync(tenantId, user.Id, ct))
            return SeatResult<MobileUser>.Fail(409, "LAST_ADMIN", "The tenant must keep at least one active administrator.");

        if (fullName is not null) user.FullName = fullName;
        if (body.Password is not null) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password);
        if (role is not null) user.Role = role;
        if (body.IsActive is { } isActive) user.IsActive = isActive;
        // Approval rights belong to managers; an administrator has them by role and a
        // sales user has none, so the flags are cleared for every other role.
        if (user.Role == MobileUserRoles.Manager)
        {
            if (body.CanApprove is { } canApprove) user.CanApprove = canApprove;
            if (body.CanManageApprovalRules is { } canManage) user.CanManageApprovalRules = canManage;
        }
        else
        {
            user.CanApprove = false;
            user.CanManageApprovalRules = false;
        }
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return SeatResult<MobileUser>.Ok(user);
    }

    public async Task<SeatResult<bool>> DeleteUserAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        await using var transaction = await BeginSeatTransactionAsync(ct);
        if (!await LockTenantAsync(tenantId, ct))
            return SeatResult<bool>.Fail(404, "TENANT_NOT_FOUND", "Tenant not found.");

        var user = await _db.MobileUsers.FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.DeletedAtUtc == null, ct);
        if (user is null)
            return SeatResult<bool>.Fail(404, "USER_NOT_FOUND", "Mobile user not found.");
        if (user.IsActive && user.Role == MobileUserRoles.Admin && !await HasOtherActiveAdminAsync(tenantId, user.Id, ct))
            return SeatResult<bool>.Fail(409, "LAST_ADMIN", "The tenant must keep at least one active administrator.");

        var now = DateTimeOffset.UtcNow;
        user.IsActive = false;
        user.DeletedAtUtc = now;
        user.UpdatedAtUtc = now;
        await _db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return SeatResult<bool>.Ok(true);
    }

    /// <summary>
    /// Records a seat purchase as the new current subscription. Gives the tenant
    /// a sign-in code on its first subscription.
    /// </summary>
    public async Task<SeatResult<TenantSubscription>> SetSubscriptionAsync(
        Guid tenantId, SetSubscriptionRequest body, Guid? adminId, CancellationToken ct)
    {
        if (body.Seats is < 1 or > 10_000)
            return SeatResult<TenantSubscription>.Fail(400, "INVALID_SEATS", "seats must be between 1 and 10000.");
        var now = DateTimeOffset.UtcNow;
        if (body.EndsAtUtc is { } ends && ends <= now)
            return SeatResult<TenantSubscription>.Fail(400, "INVALID_END_DATE", "endsAtUtc must be in the future.");
        var source = string.IsNullOrWhiteSpace(body.Source) ? "manual" : body.Source.Trim().ToLowerInvariant();
        if (source.Length > 32)
            return SeatResult<TenantSubscription>.Fail(400, "INVALID_SOURCE", "source must be at most 32 characters.");
        if (body.Reference?.Trim().Length > 128)
            return SeatResult<TenantSubscription>.Fail(400, "INVALID_REFERENCE", "reference must be at most 128 characters.");
        if (body.Note?.Trim().Length > 500)
            return SeatResult<TenantSubscription>.Fail(400, "INVALID_NOTE", "note must be at most 500 characters.");

        await using var transaction = await BeginSeatTransactionAsync(ct);
        if (!await LockTenantAsync(tenantId, ct))
            return SeatResult<TenantSubscription>.Fail(404, "TENANT_NOT_FOUND", "Tenant not found.");

        var used = await CountSeatsInUseAsync(tenantId, ct);
        if (body.Seats < used)
            return SeatResult<TenantSubscription>.Fail(409, "SEATS_BELOW_ACTIVE_USERS",
                $"The tenant has {used} active users; deactivate or delete users before reducing seats to {body.Seats}.");

        var previous = await _db.TenantSubscriptions.Where(s => s.TenantId == tenantId && s.IsCurrent).ToListAsync(ct);
        if (previous.Count > 0)
        {
            foreach (var row in previous) row.IsCurrent = false;
            // Clear the old current row before inserting the new one: the partial
            // unique index would reject both rows being current within one batch.
            await _db.SaveChangesAsync(ct);
        }

        var subscription = new TenantSubscription
        {
            TenantId = tenantId,
            Seats = body.Seats,
            StartsAtUtc = now,
            EndsAtUtc = body.EndsAtUtc,
            Source = source,
            Reference = string.IsNullOrWhiteSpace(body.Reference) ? null : body.Reference.Trim(),
            Note = string.IsNullOrWhiteSpace(body.Note) ? null : body.Note.Trim(),
            IsCurrent = true,
            CreatedAtUtc = now,
            CreatedByAdminId = adminId,
        };
        _db.TenantSubscriptions.Add(subscription);

        var tenant = await _db.Tenants.FirstAsync(t => t.Id == tenantId, ct);
        if (string.IsNullOrEmpty(tenant.Code)) tenant.Code = await NewTenantCodeAsync(ct);

        await _db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return SeatResult<TenantSubscription>.Ok(subscription);
    }

    /// <summary>Lower-cased username when valid, otherwise <c>null</c>.</summary>
    public static string? NormalizeUsername(string? value)
    {
        var username = value?.Trim().ToLowerInvariant();
        return username is not null && UsernamePattern().IsMatch(username) ? username : null;
    }

    private static string? ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            return $"password must be at least {MinPasswordLength} characters.";
        return System.Text.Encoding.UTF8.GetByteCount(password) > MaxPasswordLength
            ? $"password must be at most {MaxPasswordLength} bytes."
            : null;
    }

    private Task<int> CountSeatsInUseAsync(Guid tenantId, CancellationToken ct) =>
        _db.MobileUsers.CountAsync(u => u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null, ct);

    private Task<bool> HasOtherActiveAdminAsync(Guid tenantId, Guid exceptUserId, CancellationToken ct) =>
        _db.MobileUsers.AnyAsync(u => u.TenantId == tenantId && u.Id != exceptUserId && u.IsActive
            && u.DeletedAtUtc == null && u.Role == MobileUserRoles.Admin, ct);

    /// <summary>Refuses a new active user when the subscription is not usable or every seat is taken.</summary>
    private async Task<(int Status, string Code, string Message)?> SeatGateAsync(Guid tenantId, CancellationToken ct)
    {
        var subscription = await GetCurrentSubscriptionAsync(tenantId, ct);
        var status = SubscriptionStatus(subscription, DateTimeOffset.UtcNow);
        if (status == "none")
            return (403, "SUBSCRIPTION_REQUIRED", "The tenant has no mobile seats.");
        if (status == "expired")
            return (403, "SUBSCRIPTION_EXPIRED", "The tenant's subscription has expired.");
        var used = await CountSeatsInUseAsync(tenantId, ct);
        if (used >= subscription!.Seats)
            return (409, "SEAT_LIMIT_REACHED", $"All {subscription.Seats} seats are in use; deactivate or delete a user first.");
        return null;
    }

    private async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction?> BeginSeatTransactionAsync(CancellationToken ct) =>
        _db.Database.IsRelational() ? await _db.Database.BeginTransactionAsync(ct) : null;

    /// <summary>Takes the tenant row lock for the rest of the transaction. False when the tenant does not exist.</summary>
    private async Task<bool> LockTenantAsync(Guid tenantId, CancellationToken ct)
    {
        if (!_db.Database.IsRelational())
            return await _db.Tenants.AnyAsync(t => t.Id == tenantId, ct);
        var updated = await _db.Tenants.Where(t => t.Id == tenantId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.SeatLockVersion, t => t.SeatLockVersion + 1), ct);
        return updated == 1;
    }

    private async Task<string> NewTenantCodeAsync(CancellationToken ct)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var code = string.Create(8, 0, static (span, _) =>
            {
                for (var i = 0; i < span.Length; i++) span[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
            });
            if (!await _db.Tenants.AnyAsync(t => t.Code == code, ct)) return code;
        }
        throw new InvalidOperationException("Could not allocate a unique tenant code.");
    }

    [GeneratedRegex("^[a-z0-9._-]{3,64}$")]
    private static partial Regex UsernamePattern();
}
