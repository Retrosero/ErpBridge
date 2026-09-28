using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Targets;

/// <summary>
/// Regions and teams (GOAL_HEDEF_RUT K2). Anyone with the reports right reads what their scope shows; only an
/// administrator changes the structure. A user belongs to at most one team (moving them to another removes the
/// old membership); responsibility goes to administrators and managers only.
/// </summary>
public sealed class TeamService
{
    public const int MaxNameLength = 100;
    public const int MaxMembers = 500;

    public sealed class Rejected(int status, string code, string message) : Exception(message)
    {
        public int Status { get; } = status;

        public string Code { get; } = code;
    }

    public async Task<TeamsResponse> ListAsync(CentralApiDbContext db, TeamScope scope, CancellationToken ct)
    {
        var tenantId = scope.User.TenantId;
        var directory = await TeamDirectory.LoadAsync(db, tenantId, ct);
        var users = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .Where(u => u.TenantId == tenantId && u.DeletedAtUtc == null)
            .ToListAsync(ct);
        return new TeamsResponse
        {
            CanEdit = RolePermissions.CanManageUsers(scope.User),
            WholeCompany = scope.WholeCompany,
            Teams = directory.Teams.Values
                .Where(t => scope.SeesTeam(t.Id))
                .OrderBy(t => t.Kind == SalesTeamKinds.Region ? 0 : 1).ThenBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(t => ToDto(t, directory))
                .ToArray(),
            People = users
                .Where(u => scope.SeesUser(u.Id))
                .OrderBy(u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, StringComparer.CurrentCultureIgnoreCase)
                .Select(u => new TeamPersonDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Roles = [.. RolePermissions.Of(u)],
                    TeamId = directory.TeamOfUser.TryGetValue(u.Id, out var team) ? team : null,
                    IsActive = u.IsActive,
                })
                .ToArray(),
        };
    }

    private static TeamDto ToDto(SalesTeam team, TeamDirectory directory) => new()
    {
        Id = team.Id,
        Name = team.Name,
        Kind = team.Kind,
        ParentId = team.ParentId,
        IsActive = team.IsActive,
        MemberIds = [.. directory.TeamOfUser.Where(p => p.Value == team.Id).Select(p => p.Key)],
        ManagerIds = [.. directory.ManagersOf.GetValueOrDefault(team.Id) ?? []],
    };

    /// <summary>Creates (<paramref name="id"/> null) or updates a team or region, members and managers included.</summary>
    public async Task<TeamDto> SaveAsync(CentralApiDbContext db, MobileUser actor, Guid? id, TeamSaveRequest request, CancellationToken ct)
    {
        if (!RolePermissions.CanManageUsers(actor))
            throw new Rejected(StatusCodes.Status403Forbidden, "TEAM_ADMIN_REQUIRED", "Ekip yapısını yalnız yönetici (admin) değiştirebilir.");
        var tenantId = actor.TenantId;
        var now = TargetService.NowMs();
        var relational = db.Database.IsRelational();
        await using var transaction = relational ? await db.Database.BeginTransactionAsync(ct) : null;

        SalesTeam team;
        if (id is { } existingId)
        {
            team = await db.SalesTeams.FirstOrDefaultAsync(t => t.Id == existingId && t.TenantId == tenantId, ct)
                   ?? throw new Rejected(StatusCodes.Status404NotFound, "TEAM_NOT_FOUND", "Ekip bulunamadı.");
            if (request.Kind is { } kind && !string.Equals(kind.Trim(), team.Kind, StringComparison.OrdinalIgnoreCase))
                throw Invalid("Bölge ve ekip türü sonradan değiştirilemez.");
        }
        else
        {
            var kind = request.Kind?.Trim().ToUpperInvariant();
            if (!SalesTeamKinds.IsValid(kind)) throw Invalid("Tür bölge (REGION) ya da ekip (TEAM) olmalı.");
            team = new SalesTeam { TenantId = tenantId, Kind = kind!, CreatedAtMs = now };
            db.SalesTeams.Add(team);
        }

        {
            var name = request.Name?.Trim() ?? string.Empty;
            if (name.Length == 0 || name.Length > MaxNameLength) throw Invalid($"Ad 1–{MaxNameLength} karakter olmalı.");
            var taken = await db.SalesTeams.AnyAsync(t => t.TenantId == tenantId && t.Id != team.Id && t.Kind == team.Kind && t.Name.ToLower() == name.ToLower(), ct);
            if (taken) throw new Rejected(StatusCodes.Status409Conflict, "TEAM_NAME_TAKEN", "Bu adla bir kayıt zaten var.");
            team.Name = name;
        }
        if (request.IsActive is { } active) team.IsActive = active;

        // Name and region are always sent whole: an absent region takes the team out of its region.
        if (team.Kind == SalesTeamKinds.Team)
        {
            if (request.ParentId is { } parentId
                && !await db.SalesTeams.AnyAsync(t => t.Id == parentId && t.TenantId == tenantId && t.Kind == SalesTeamKinds.Region, ct))
                throw Invalid("Üst bölge bulunamadı.");
            team.ParentId = request.ParentId;
        }
        else if (request.ParentId is not null) throw Invalid("Bölge başka bir bölgeye bağlanamaz.");
        else if (request.MemberIds is { Length: > 0 }) throw Invalid("Kişiler bölgeye değil ekibe eklenir.");
        team.UpdatedAtMs = now;
        await db.SaveChangesAsync(ct);

        if (request.MemberIds is { } memberIds && team.Kind == SalesTeamKinds.Team)
        {
            var wanted = memberIds.Distinct().ToList();
            if (wanted.Count > MaxMembers) throw Invalid($"Bir ekipte en çok {MaxMembers} kişi olabilir.");
            var known = await db.MobileUsers.Where(u => u.TenantId == tenantId && u.DeletedAtUtc == null && wanted.Contains(u.Id)).Select(u => u.Id).ToListAsync(ct);
            if (known.Count != wanted.Count) throw Invalid("Listede firmada olmayan bir kullanıcı var.");
            // One team per user: whoever joins leaves their old team; whoever is left out leaves this one.
            var stale = await db.SalesTeamMembers.Where(m => m.TenantId == tenantId && (m.TeamId == team.Id || wanted.Contains(m.UserId))).ToListAsync(ct);
            db.SalesTeamMembers.RemoveRange(stale);
            await db.SaveChangesAsync(ct);
            foreach (var userId in wanted)
                db.SalesTeamMembers.Add(new SalesTeamMember { TenantId = tenantId, UserId = userId, TeamId = team.Id, AddedAtMs = now });
            await db.SaveChangesAsync(ct);
        }

        if (request.ManagerIds is { } managerIds)
        {
            var wanted = managerIds.Distinct().ToList();
            var candidates = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
                .Where(u => u.TenantId == tenantId && u.DeletedAtUtc == null && wanted.Contains(u.Id)).ToListAsync(ct);
            if (candidates.Count != wanted.Count || candidates.Any(u => !RolePermissions.CanManageTargets(u)))
                throw Invalid("Sorumlu yalnız yönetici ya da admin rolündeki bir kişi olabilir.");
            db.SalesTeamManagers.RemoveRange(await db.SalesTeamManagers.Where(m => m.TeamId == team.Id).ToListAsync(ct));
            await db.SaveChangesAsync(ct);
            foreach (var userId in wanted)
                db.SalesTeamManagers.Add(new SalesTeamManager { TenantId = tenantId, TeamId = team.Id, UserId = userId });
            await db.SaveChangesAsync(ct);
        }

        if (transaction is not null) await transaction.CommitAsync(ct);
        return ToDto(team, await TeamDirectory.LoadAsync(db, tenantId, ct));
    }

    /// <summary>
    /// Removes a team with its memberships and managers; its targets are marked deleted (history stays in the log).
    /// A region with teams under it is refused.
    /// </summary>
    public async Task DeleteAsync(CentralApiDbContext db, MobileUser actor, Guid id, CancellationToken ct)
    {
        if (!RolePermissions.CanManageUsers(actor))
            throw new Rejected(StatusCodes.Status403Forbidden, "TEAM_ADMIN_REQUIRED", "Ekip yapısını yalnız yönetici (admin) değiştirebilir.");
        var tenantId = actor.TenantId;
        var team = await db.SalesTeams.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId, ct)
                   ?? throw new Rejected(StatusCodes.Status404NotFound, "TEAM_NOT_FOUND", "Ekip bulunamadı.");
        if (await db.SalesTeams.AnyAsync(t => t.TenantId == tenantId && t.ParentId == id, ct))
            throw new Rejected(StatusCodes.Status409Conflict, "TEAM_HAS_CHILDREN", "Önce bu bölgedeki ekipleri başka bölgeye taşıyın ya da silin.");

        var relational = db.Database.IsRelational();
        await using var transaction = relational ? await db.Database.BeginTransactionAsync(ct) : null;
        db.SalesTeamMembers.RemoveRange(await db.SalesTeamMembers.Where(m => m.TenantId == tenantId && m.TeamId == id).ToListAsync(ct));
        db.SalesTeamManagers.RemoveRange(await db.SalesTeamManagers.Where(m => m.TeamId == id).ToListAsync(ct));
        var now = TargetService.NowMs();
        var actorName = string.IsNullOrWhiteSpace(actor.FullName) ? actor.Username : actor.FullName;
        foreach (var target in await db.SalesTargets.Where(t => t.TenantId == tenantId && t.OwnerKind == TargetOwnerKinds.Team && t.OwnerId == id && !t.IsDeleted).ToListAsync(ct))
        {
            // The same trail a deletion from the targets page leaves (Codex, PR #214).
            db.SalesTargetEvents.Add(new SalesTargetEvent
            {
                TenantId = tenantId, TargetId = target.Id, Action = "DELETE", OldValue = target.Value,
                ActorUserId = actor.Id, ActorName = actorName.Length <= 120 ? actorName : actorName[..120], OccurredAtMs = now,
            });
            target.IsDeleted = true;
            target.UpdatedAtMs = now;
            target.UpdatedByUserId = actor.Id;
        }
        db.SalesTeams.Remove(team);
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
    }

    private static Rejected Invalid(string message) => new(StatusCodes.Status400BadRequest, "TEAM_INVALID", message);
}
