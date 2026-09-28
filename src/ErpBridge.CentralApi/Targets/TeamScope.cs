using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Targets;

/// <summary>
/// A company's regions, teams, members and responsible managers, read once per request.
/// </summary>
public sealed class TeamDirectory
{
    public required IReadOnlyDictionary<Guid, SalesTeam> Teams { get; init; }

    /// <summary>User → team.</summary>
    public required IReadOnlyDictionary<Guid, Guid> TeamOfUser { get; init; }

    /// <summary>Team or region → responsible managers.</summary>
    public required IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> ManagersOf { get; init; }

    public static async Task<TeamDirectory> LoadAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct)
    {
        var teams = await db.SalesTeams.AsNoTracking().Where(t => t.TenantId == tenantId).ToDictionaryAsync(t => t.Id, ct);
        var members = await db.SalesTeamMembers.AsNoTracking().Where(m => m.TenantId == tenantId).ToListAsync(ct);
        var managers = await db.SalesTeamManagers.AsNoTracking().Where(m => m.TenantId == tenantId).ToListAsync(ct);
        return new TeamDirectory
        {
            Teams = teams,
            TeamOfUser = members.Where(m => teams.ContainsKey(m.TeamId)).ToDictionary(m => m.UserId, m => m.TeamId),
            ManagersOf = managers.Where(m => teams.ContainsKey(m.TeamId))
                .GroupBy(m => m.TeamId).ToDictionary(g => g.Key, g => (IReadOnlyList<Guid>)g.Select(m => m.UserId).ToList()),
        };
    }

    /// <summary>The team itself and, for a region, every team under it.</summary>
    public IEnumerable<Guid> Expand(Guid teamId)
    {
        if (!Teams.TryGetValue(teamId, out var team)) yield break;
        yield return teamId;
        if (team.Kind != SalesTeamKinds.Region) yield break;
        foreach (var child in Teams.Values.Where(t => t.ParentId == teamId)) yield return child.Id;
    }

    /// <summary>Users of the team or, for a region, of every team under it.</summary>
    public IReadOnlySet<Guid> MembersOf(Guid teamId)
    {
        var teams = Expand(teamId).ToHashSet();
        return TeamOfUser.Where(p => teams.Contains(p.Value)).Select(p => p.Key).ToHashSet();
    }
}

/// <summary>
/// Who a user may see and set targets for (GOAL_HEDEF_RUT K3). One class, so the panel, the phone
/// and the route planner cannot disagree:
/// <list type="bullet">
/// <item>ADMIN — the whole company.</item>
/// <item>MANAGER responsible for at least one team or region — those teams (a region with its teams),
/// their members, and the manager.</item>
/// <item>MANAGER responsible for nothing — the whole company, as before teams existed.</item>
/// <item>Anyone else — only themselves, read-only.</item>
/// </list>
/// </summary>
public sealed class TeamScope
{
    private TeamScope(MobileUser user, TeamDirectory directory, bool wholeCompany, IReadOnlySet<Guid> teams, IReadOnlySet<Guid> users)
    {
        User = user;
        Directory = directory;
        WholeCompany = wholeCompany;
        TeamIds = teams;
        UserIds = users;
    }

    public MobileUser User { get; }

    public TeamDirectory Directory { get; }

    /// <summary>Sees every user and team; may set the company-wide target.</summary>
    public bool WholeCompany { get; }

    /// <summary>Visible teams and regions; every team when <see cref="WholeCompany"/>.</summary>
    public IReadOnlySet<Guid> TeamIds { get; }

    /// <summary>Visible users, the user included; empty means "all" only when <see cref="WholeCompany"/>.</summary>
    public IReadOnlySet<Guid> UserIds { get; }

    public bool CanManage => RolePermissions.CanManageTargets(User);

    public bool SeesUser(Guid userId) => WholeCompany || UserIds.Contains(userId);

    public bool SeesTeam(Guid teamId) => WholeCompany ? Directory.Teams.ContainsKey(teamId) : TeamIds.Contains(teamId);

    /// <summary>Setting a target for an owner: the manage right plus the owner inside the scope.</summary>
    public bool MayWrite(string ownerKind, Guid ownerId) => CanManage && ownerKind switch
    {
        TargetOwnerKinds.User => SeesUser(ownerId),
        TargetOwnerKinds.Team => SeesTeam(ownerId),
        TargetOwnerKinds.Company => WholeCompany,
        _ => false,
    };

    public static async Task<TeamScope> LoadAsync(CentralApiDbContext db, MobileUser user, CancellationToken ct) =>
        For(user, await TeamDirectory.LoadAsync(db, user.TenantId, ct));

    public static TeamScope For(MobileUser user, TeamDirectory directory)
    {
        if (RolePermissions.IsAdmin(user))
            return new TeamScope(user, directory, true, directory.Teams.Keys.ToHashSet(), new HashSet<Guid>());

        if (RolePermissions.Has(user, MobileUserRoles.Manager))
        {
            var responsible = directory.ManagersOf.Where(p => p.Value.Contains(user.Id)).Select(p => p.Key).ToList();
            if (responsible.Count == 0)
                return new TeamScope(user, directory, true, directory.Teams.Keys.ToHashSet(), new HashSet<Guid>());
            var teams = responsible.SelectMany(directory.Expand).ToHashSet();
            var users = directory.TeamOfUser.Where(p => teams.Contains(p.Value)).Select(p => p.Key).ToHashSet();
            users.Add(user.Id);
            return new TeamScope(user, directory, false, teams, users);
        }

        return new TeamScope(user, directory, false, new HashSet<Guid>(), new HashSet<Guid> { user.Id });
    }
}
