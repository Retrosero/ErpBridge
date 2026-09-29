using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Permissions;

/// <summary>An outcome with an HTTP status and, on failure, the error to return.</summary>
public sealed record PermissionResult<T>(int StatusCode, T? Value, ApiError? Error)
{
    public bool Succeeded => Error is null;

    public static PermissionResult<T> Ok(T value) => new(200, value, null);

    public static PermissionResult<T> Fail(int status, string code, string message) =>
        new(status, default, new ApiError { ErrorCode = code, Message = message });
}

/// <summary>
/// Reading and changing role templates and personal overrides (GOAL_YETKILER). The caller checks who may do it
/// (user management); this validates every value against the catalogue, stores only differences from the default,
/// and writes a <see cref="PermissionChange"/> for every real change in the same transaction.
/// </summary>
public sealed class PermissionService
{
    public const int MaxChanges = 200;

    /// <summary>Who is making a change, for the audit trail.</summary>
    public sealed record Actor(Guid UserId, string Name, string Client);

    // ---- role templates -------------------------------------------------------------------------

    public async Task<RolePermissionsResponse> RolesAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var rows = await db.TenantRolePermissions.AsNoTracking().Where(t => t.TenantId == tenantId).ToListAsync(ct);
        return new RolePermissionsResponse
        {
            Roles = MobileUserRoles.All.Select(role =>
            {
                if (role == MobileUserRoles.Admin)
                    return new RolePermissionsDto
                    {
                        Role = role,
                        Locked = true,
                        Values = PermissionCatalog.All.ToDictionary(
                            d => d.Key, d => d.Type == PermissionType.Bool ? PermissionValues.True : PermissionValues.Unlimited, StringComparer.Ordinal),
                    };
                var mine = rows.Where(r => r.Role == role).ToDictionary(r => r.Key, r => r.Value, StringComparer.Ordinal);
                return new RolePermissionsDto
                {
                    Role = role,
                    Values = PermissionCatalog.All.ToDictionary(d => d.Key, d => mine.GetValueOrDefault(d.Key) ?? d.Defaults[role], StringComparer.Ordinal),
                    Customized = PermissionCatalog.All.Where(d => mine.ContainsKey(d.Key)).Select(d => d.Key).ToArray(),
                };
            }).ToArray(),
        };
    }

    public async Task<PermissionResult<RolePermissionsResponse>> UpdateRoleAsync(
        CentralApiDbContext db, Guid tenantId, Actor actor, string role, IReadOnlyDictionary<string, string?> values, CancellationToken ct)
    {
        var normalizedRole = role.Trim().ToUpperInvariant();
        if (normalizedRole == MobileUserRoles.Admin)
            return PermissionResult<RolePermissionsResponse>.Fail(409, "ADMIN_ROLE_LOCKED", "Admin rolü her zaman bütün yetkilere sahiptir; değiştirilemez.");
        if (!MobileUserRoles.IsValid(normalizedRole))
            return PermissionResult<RolePermissionsResponse>.Fail(400, "INVALID_ROLE", $"Bilinmeyen rol: {role}.");

        var parsed = Parse(values, allowLocked: false);
        if (parsed.Error is { } error) return new PermissionResult<RolePermissionsResponse>(parsed.Status, null, error);

        var rows = await db.TenantRolePermissions.Where(t => t.TenantId == tenantId && t.Role == normalizedRole).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var (definition, value) in parsed.Values)
        {
            var row = rows.FirstOrDefault(r => r.Key == definition.Key);
            // Only differences from the catalogue default are stored: a value equal to it removes the row.
            var stored = value is null || value == definition.Defaults[normalizedRole] ? null : value;
            var previous = row?.Value;
            if (previous == stored) continue;
            if (stored is null)
            {
                if (row is null) continue;
                db.TenantRolePermissions.Remove(row);
            }
            else if (row is null)
            {
                db.TenantRolePermissions.Add(new TenantRolePermission
                {
                    TenantId = tenantId, Role = normalizedRole, Key = definition.Key, Value = stored, UpdatedAtUtc = now, UpdatedByUserId = actor.UserId,
                });
            }
            else
            {
                row.Value = stored;
                row.UpdatedAtUtc = now;
                row.UpdatedByUserId = actor.UserId;
            }
            db.PermissionChanges.Add(Change(tenantId, actor, "role", definition.Key, previous, stored, now, role: normalizedRole));
        }
        await db.SaveChangesAsync(ct);
        return PermissionResult<RolePermissionsResponse>.Ok(await RolesAsync(db, tenantId, ct));
    }

    // ---- personal overrides ---------------------------------------------------------------------

    public async Task<PermissionResult<UserPermissionsResponse>> UserAsync(CentralApiDbContext db, Guid tenantId, Guid userId, CancellationToken ct)
    {
        var user = await LoadUserAsync(db, tenantId, userId, tracking: false, ct);
        if (user is null) return PermissionResult<UserPermissionsResponse>.Fail(404, "USER_NOT_FOUND", "Kullanıcı bulunamadı.");
        var permissions = await PermissionLoader.LoadAsync(db, user, ct);
        return PermissionResult<UserPermissionsResponse>.Ok(new UserPermissionsResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Roles = MobileUserRoles.All.Where(RolePermissions.Of(user).Contains).ToArray(),
            Locked = permissions.IsAdmin,
            Items = permissions.Entries.Select(e => new UserPermissionItemDto
            {
                Key = e.Key,
                Value = e.Value,
                Source = e.Source.ToString().ToLowerInvariant(),
                RoleValue = e.RoleValue,
                RoleSources = e.RoleSources.ToArray(),
                Override = e.Override,
            }).ToArray(),
        });
    }

    public async Task<PermissionResult<UserPermissionsResponse>> UpdateUserAsync(
        CentralApiDbContext db, Guid tenantId, Actor actor, Guid userId, IReadOnlyDictionary<string, string?> overrides, CancellationToken ct)
    {
        var user = await LoadUserAsync(db, tenantId, userId, tracking: true, ct);
        if (user is null) return PermissionResult<UserPermissionsResponse>.Fail(404, "USER_NOT_FOUND", "Kullanıcı bulunamadı.");
        if (RolePermissions.IsAdmin(user))
            return PermissionResult<UserPermissionsResponse>.Fail(409, "ADMIN_USER_LOCKED", "Admin her zaman bütün yetkilere sahiptir; kişiye özel ayar yapılamaz.");

        var parsed = Parse(overrides, allowLocked: true);
        if (parsed.Error is { } error) return new PermissionResult<UserPermissionsResponse>(parsed.Status, null, error);

        var isManager = RolePermissions.Has(user, MobileUserRoles.Manager);
        var now = DateTimeOffset.UtcNow;
        foreach (var (definition, value) in parsed.Values)
        {
            // A manager's approval rights are the two switches the approval centre already reads.
            if (definition.Key is PermissionKeys.ApprovalsDecide or PermissionKeys.ApprovalsManageRules)
            {
                if (!isManager)
                    return PermissionResult<UserPermissionsResponse>.Fail(409, "PERMISSION_LOCKED", $"{definition.Label} yalnız Yönetici rolündeki kişiye verilir.");
                var granted = value == PermissionValues.True;
                var before = definition.Key == PermissionKeys.ApprovalsDecide ? user.CanApprove : user.CanManageApprovalRules;
                if (before == granted) continue;
                if (definition.Key == PermissionKeys.ApprovalsDecide) user.CanApprove = granted; else user.CanManageApprovalRules = granted;
                user.UpdatedAtUtc = now;
                db.PermissionChanges.Add(Change(tenantId, actor, "user", definition.Key, Bool(before), Bool(granted), now, user: user));
                continue;
            }
            if (definition.Locked)
                return PermissionResult<UserPermissionsResponse>.Fail(409, "PERMISSION_LOCKED", $"{definition.Label} kişiye özel değiştirilemez.");

            var row = user.PermissionOverrides.FirstOrDefault(o => o.Key == definition.Key);
            var previous = row?.Value;
            if (previous == value) continue;
            if (value is null)
            {
                if (row is null) continue;
                db.MobileUserPermissionOverrides.Remove(row);
            }
            else if (row is null)
            {
                db.MobileUserPermissionOverrides.Add(new MobileUserPermissionOverride
                {
                    UserId = user.Id, Key = definition.Key, Value = value, UpdatedAtUtc = now, UpdatedByUserId = actor.UserId,
                });
            }
            else
            {
                row.Value = value;
                row.UpdatedAtUtc = now;
                row.UpdatedByUserId = actor.UserId;
            }
            db.PermissionChanges.Add(Change(tenantId, actor, "user", definition.Key, previous, value, now, user: user));
        }
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        return await UserAsync(db, tenantId, userId, ct);
    }

    /// <summary>A change to the user's roles, recorded next to their permission changes.</summary>
    public static PermissionChange RolesChange(Guid tenantId, Actor actor, MobileUser user, IEnumerable<string> before, IEnumerable<string> after) =>
        Change(tenantId, actor, "roles", "roles", string.Join(",", before), string.Join(",", after), DateTimeOffset.UtcNow, user: user);

    // ---- audit ----------------------------------------------------------------------------------

    public async Task<PermissionChangesResponse> ChangesAsync(CentralApiDbContext db, Guid tenantId, Guid? userId, string? role, int? take, CancellationToken ct)
    {
        var limit = Math.Clamp(take ?? 50, 1, MaxChanges);
        var query = db.PermissionChanges.AsNoTracking().Where(c => c.TenantId == tenantId);
        if (userId is { } id) query = query.Where(c => c.TargetUserId == id);
        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalized = role.Trim().ToUpperInvariant();
            query = query.Where(c => c.Role == normalized);
        }
        var rows = await query.OrderByDescending(c => c.Id).Take(limit).ToListAsync(ct);
        return new PermissionChangesResponse
        {
            Changes = rows.Select(c => new PermissionChangeDto
            {
                Id = c.Id, ActorName = c.ActorName, Client = c.Client, Scope = c.Scope, Role = c.Role,
                TargetUserId = c.TargetUserId, TargetUserName = c.TargetUserName, Key = c.Key,
                OldValue = c.OldValue, NewValue = c.NewValue, CreatedAtUtc = c.CreatedAtUtc,
            }).ToArray(),
        };
    }

    // ---- helpers --------------------------------------------------------------------------------

    private sealed record Parsed(int Status, ApiError? Error, List<(PermissionDefinition Definition, string? Value)> Values);

    /// <summary>Every key known, every value valid for its type; null = remove. One bad entry refuses the whole patch.</summary>
    private static Parsed Parse(IReadOnlyDictionary<string, string?> values, bool allowLocked)
    {
        var result = new List<(PermissionDefinition, string?)>();
        foreach (var (key, raw) in values)
        {
            var definition = PermissionCatalog.Find(key?.Trim() ?? string.Empty);
            if (definition is null) return Fail(400, "UNKNOWN_PERMISSION", $"Bilinmeyen yetki: {key}.");
            if (definition.Locked && !allowLocked) return Fail(409, "PERMISSION_LOCKED", $"{definition.Label} rol şablonunda değiştirilemez.");
            if (raw is null)
            {
                result.Add((definition, null));
                continue;
            }
            var value = PermissionValues.Normalize(definition, raw);
            if (value is null) return Fail(400, "INVALID_PERMISSION_VALUE", $"{definition.Label}: geçersiz değer '{raw}'.");
            result.Add((definition, value));
        }
        return new Parsed(200, null, result);

        static Parsed Fail(int status, string code, string message) => new(status, new ApiError { ErrorCode = code, Message = message }, []);
    }

    private static Task<MobileUser?> LoadUserAsync(CentralApiDbContext db, Guid tenantId, Guid userId, bool tracking, CancellationToken ct)
    {
        var query = db.MobileUsers.Include(u => u.Roles).Include(u => u.PermissionOverrides)
            .Where(u => u.Id == userId && u.TenantId == tenantId && u.DeletedAtUtc == null);
        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(ct);
    }

    private static string Bool(bool value) => value ? PermissionValues.True : PermissionValues.False;

    private static PermissionChange Change(Guid tenantId, Actor actor, string scope, string key, string? oldValue, string? newValue, DateTimeOffset now,
        string? role = null, MobileUser? user = null) => new()
    {
        TenantId = tenantId,
        ActorUserId = actor.UserId,
        ActorName = Clip(actor.Name, 120),
        Client = actor.Client,
        Scope = scope,
        Role = role,
        TargetUserId = user?.Id,
        TargetUserName = user is null ? null : Clip(user.FullName, 120),
        Key = key,
        OldValue = oldValue is null ? null : Clip(oldValue, 256),
        NewValue = newValue is null ? null : Clip(newValue, 256),
        CreatedAtUtc = now,
    };

    private static string Clip(string value, int max) => value.Length <= max ? value : value[..max];
}
