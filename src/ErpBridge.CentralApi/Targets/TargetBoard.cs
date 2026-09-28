using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Targets;

/// <summary>
/// Measures targets against facts (GOAL_HEDEF_RUT K6–K11): an owner's summary and target rows for a period,
/// with pace, forecast, what is needed per day, and the targets worked out rather than entered — a day's share
/// of the week/month target (K9) and the planned route stops as the visit target (K10). Pure: everything it
/// needs is handed in, so it is tested without a database.
/// </summary>
public sealed class TargetBoard
{
    private readonly TargetFactSet _facts;
    private readonly TeamDirectory _directory;
    private readonly IReadOnlyList<SalesTarget> _targets;
    private readonly IReadOnlyDictionary<Guid, string> _userNames;
    private readonly int _workDays;
    private readonly DateOnly _today;
    private readonly Dictionary<Guid, List<TargetFact>> _factsByUser = [];
    private readonly Dictionary<Guid, List<TargetFact>> _pendingByUser = [];

    /// <param name="targets">Every live target overlapping the periods asked about.</param>
    /// <param name="userNames">Display names of users who updated targets.</param>
    public TargetBoard(TargetFactSet facts, TeamDirectory directory, IReadOnlyList<SalesTarget> targets,
        IReadOnlyDictionary<Guid, string> userNames, int workDays, DateOnly today)
    {
        _facts = facts;
        _directory = directory;
        _targets = targets;
        _userNames = userNames;
        _workDays = workDays;
        _today = today;
        foreach (var fact in facts.Facts)
            if (facts.UserOf(fact) is { } user) Add(_factsByUser, user, fact);
        foreach (var fact in facts.Pending)
            if (facts.UserOf(fact) is { } user) Add(_pendingByUser, user, fact);
    }

    private static void Add(Dictionary<Guid, List<TargetFact>> map, Guid user, TargetFact fact)
    {
        if (!map.TryGetValue(user, out var list)) map[user] = list = [];
        list.Add(fact);
    }

    // ---- periods -----------------------------------------------------------------------------

    public (int Total, int Elapsed, int Left, DateOnly AsOf) WorkDaysOf(TargetPeriod period)
    {
        var total = TargetPeriod.WorkDaysBetween(period.Start, period.End, _workDays);
        var elapsed = _today < period.Start ? 0 : TargetPeriod.WorkDaysBetween(period.Start, Min(_today, period.End), _workDays);
        var left = _today > period.End ? 0 : TargetPeriod.WorkDaysBetween(Max(_today, period.Start), period.End, _workDays);
        return (total, elapsed, left, Min(Max(_today, period.Start), period.End));
    }

    /// <summary>The facts a board of <paramref name="period"/> needs: a day also needs its week and month (K9).</summary>
    public static (DateOnly From, DateOnly To) FactRange(TargetPeriod period)
    {
        if (period.Type != TargetPeriodTypes.Daily) return (period.Start, period.End);
        var week = TargetPeriod.Of(TargetPeriodTypes.Weekly, period.Start);
        var month = TargetPeriod.Of(TargetPeriodTypes.Monthly, period.Start);
        return (Min(week.Start, month.Start), Max(week.End, month.End));
    }

    // ---- owners ----------------------------------------------------------------------------------

    /// <summary>The users an owner's figures are made of; null for the company (every fact, attributed or not).</summary>
    public IReadOnlySet<Guid>? UsersOf(string ownerKind, Guid ownerId) => ownerKind switch
    {
        TargetOwnerKinds.User => new HashSet<Guid> { ownerId },
        TargetOwnerKinds.Team => _directory.MembersOf(ownerId),
        _ => null,
    };

    private IEnumerable<TargetFact> FactsOf(IReadOnlySet<Guid>? users, bool pending)
    {
        if (users is null) return pending ? _facts.Pending : _facts.Facts;
        var map = pending ? _pendingByUser : _factsByUser;
        return users.SelectMany(u => map.TryGetValue(u, out var list) ? list : []);
    }

    public OwnerProgressDto Build(string ownerKind, Guid ownerId, string name, TargetPeriod period)
    {
        var users = UsersOf(ownerKind, ownerId);
        var facts = FactsOf(users, pending: false).Where(f => period.Contains(f.Day)).ToList();
        var pending = FactsOf(users, pending: true).Where(f => period.Contains(f.Day)).ToList();
        var (planned, completed) = VisitCounts(users, period.Start, period.End);

        var summary = new TargetSummaryDto
        {
            Revenue = Round(Sum(facts, TargetFactKind.Sale) - Sum(facts, TargetFactKind.Return)),
            Returns = Round(Sum(facts, TargetFactKind.Return)),
            Collection = Round(Sum(facts, TargetFactKind.Collection)),
            DocumentCount = DocumentCount(facts),
            VisitsPlanned = planned,
            VisitsCompleted = completed,
            PendingRevenue = Round(Sum(pending, TargetFactKind.Sale) - Sum(pending, TargetFactKind.Return)),
            PendingCollection = Round(Sum(pending, TargetFactKind.Collection)),
        };

        var rows = new List<TargetRowDto>();
        var explicitRows = _targets
            .Where(t => t.OwnerKind == ownerKind && t.OwnerId == ownerId && t.PeriodType == period.Type && t.PeriodKey == period.Key)
            .OrderBy(t => TargetMetrics.All.ToList().IndexOf(t.Metric)).ThenBy(t => t.ItemCode, StringComparer.Ordinal)
            .ToList();
        foreach (var target in explicitRows)
            rows.Add(Row(target.Metric, target.Measure, target.ItemCode, target.ItemName, target.Value, period, users, false,
                target.Id, target.Note, target.UpdatedAtMs, _userNames.GetValueOrDefault(target.UpdatedByUserId), ChildrenSum(ownerKind, ownerId, target)));

        if (period.Type == TargetPeriodTypes.Daily)
            rows.AddRange(DerivedDaily(ownerKind, ownerId, period, users, explicitRows));

        if (!explicitRows.Any(t => t.Metric == TargetMetrics.Visit) && planned > 0)
            rows.Add(Row(TargetMetrics.Visit, TargetMeasures.Count, string.Empty, null, planned, period, users, true));

        OwnerProgressDto dto = new()
        {
            OwnerKind = ownerKind,
            OwnerId = ownerId,
            Name = name,
            Summary = summary,
            Targets = [.. rows],
        };
        if (ownerKind == TargetOwnerKinds.User)
        {
            if (_directory.TeamOfUser.TryGetValue(ownerId, out var teamId) && _directory.Teams.TryGetValue(teamId, out var team))
            {
                dto.TeamId = team.Id;
                dto.TeamName = team.Name;
            }
            if (_facts.UsersWithoutSalesperson.Contains(ownerId))
                dto.Warning = "Plasiyer kodu eşlenmemiş: kişinin ERP satışları ona yazılamıyor (Panel → ERP aktarım ayarları → kullanıcı eşlemesi).";
        }
        else if (ownerKind == TargetOwnerKinds.Team && _directory.Teams.TryGetValue(ownerId, out var self))
        {
            dto.TeamKind = self.Kind;
            if (self.ParentId is { } parent && _directory.Teams.TryGetValue(parent, out var region))
            {
                dto.TeamId = region.Id;
                dto.TeamName = region.Name;
            }
        }
        return dto;
    }

    /// <summary>
    /// K9: for every week/month target of the owner with no daily target of its own, today's share — what is
    /// left of it before today, spread over the working days left (today included). A month wins over a week.
    /// </summary>
    private IEnumerable<TargetRowDto> DerivedDaily(string ownerKind, Guid ownerId, TargetPeriod day, IReadOnlySet<Guid>? users, List<SalesTarget> explicitRows)
    {
        var have = explicitRows.Select(t => (t.Metric, t.Measure, t.ItemCode)).ToHashSet();
        foreach (var type in new[] { TargetPeriodTypes.Monthly, TargetPeriodTypes.Weekly })
        {
            var parent = TargetPeriod.Of(type, day.Start);
            foreach (var target in _targets.Where(t => t.OwnerKind == ownerKind && t.OwnerId == ownerId && t.PeriodType == type && t.PeriodKey == parent.Key))
            {
                if (target.Metric == TargetMetrics.Visit) continue; // the route plans each day's visits (K10)
                if (!have.Add((target.Metric, target.Measure, target.ItemCode))) continue;
                var before = day.Start > parent.Start
                    ? Measure(users, parent.Start, day.Start.AddDays(-1), target.Metric, target.Measure, target.ItemCode, pending: false)
                    : 0m;
                var daysLeft = Math.Max(1, TargetPeriod.WorkDaysBetween(day.Start, parent.End, _workDays));
                var share = Math.Max(0m, (target.Value - before) / daysLeft);
                yield return Row(target.Metric, target.Measure, target.ItemCode, target.ItemName, Round(share), day, users, true, note: target.Note);
            }
        }
    }

    private TargetRowDto Row(string metric, string measure, string itemCode, string? itemName, decimal value, TargetPeriod period,
        IReadOnlySet<Guid>? users, bool derived, Guid? id = null, string? note = null, long? updatedAtMs = null, string? updatedBy = null,
        decimal? childrenSum = null)
    {
        var actual = Measure(users, period.Start, period.End, metric, measure, itemCode, pending: false);
        var pending = Measure(users, period.Start, period.End, metric, measure, itemCode, pending: true);
        var (total, elapsed, left, _) = WorkDaysOf(period);
        return new TargetRowDto
        {
            Id = id,
            PeriodType = period.Type,
            PeriodKey = period.Key,
            Metric = metric,
            Measure = measure,
            ItemCode = itemCode,
            ItemName = itemName ?? ItemName(metric, itemCode),
            Value = value,
            Note = note,
            Actual = Round(actual),
            Pending = Round(pending),
            Percent = value > 0 ? Math.Round(actual / value * 100m, 1) : null,
            ExpectedToDate = total > 0 ? Round(value * elapsed / total) : value,
            Forecast = Round(elapsed > 0 && total > 0 ? actual / elapsed * total : actual),
            RequiredPerDay = left > 0 ? Round(Math.Max(0m, value - actual) / left) : null,
            Derived = derived,
            ChildrenSum = childrenSum,
            UpdatedAtMs = updatedAtMs,
            UpdatedBy = updatedBy,
        };
    }

    /// <summary>K11: the same target one level down — a region's teams, a team's members, the company's top level.</summary>
    private decimal? ChildrenSum(string ownerKind, Guid ownerId, SalesTarget target)
    {
        IEnumerable<(string Kind, Guid Id)> children;
        if (ownerKind == TargetOwnerKinds.Team && _directory.Teams.TryGetValue(ownerId, out var team))
        {
            children = team.Kind == SalesTeamKinds.Region
                ? _directory.Teams.Values.Where(t => t.ParentId == team.Id).Select(t => (TargetOwnerKinds.Team, t.Id))
                : _directory.TeamOfUser.Where(p => p.Value == team.Id).Select(p => (TargetOwnerKinds.User, p.Key));
        }
        else if (ownerKind == TargetOwnerKinds.Company)
        {
            var top = _directory.Teams.Values.Where(t => t.ParentId is null).Select(t => (TargetOwnerKinds.Team, t.Id)).ToList();
            children = top.Count > 0 ? top : _targets.Where(t => t.OwnerKind == TargetOwnerKinds.User).Select(t => (TargetOwnerKinds.User, t.OwnerId)).Distinct();
        }
        else return null;

        var set = children.ToHashSet();
        var matches = _targets.Where(t => t.PeriodType == target.PeriodType && t.PeriodKey == target.PeriodKey && t.Metric == target.Metric
                                          && t.Measure == target.Measure && t.ItemCode == target.ItemCode && set.Contains((t.OwnerKind, t.OwnerId)))
            .ToList();
        return matches.Count == 0 ? null : matches.Sum(t => t.Value);
    }

    // ---- measuring ----------------------------------------------------------------------------

    /// <summary>What an owner did in [from, to] for a metric, in its measure.</summary>
    public decimal Measure(IReadOnlySet<Guid>? users, DateOnly from, DateOnly to, string metric, string measure, string itemCode, bool pending)
    {
        if (metric == TargetMetrics.Visit)
            return pending ? 0m : VisitCounts(users, from, to).Completed;
        var facts = FactsOf(users, pending).Where(f => f.Day >= from && f.Day <= to);
        switch (metric)
        {
            case TargetMetrics.Revenue:
                return facts.Sum(f => f.Kind == TargetFactKind.Sale ? f.Amount : f.Kind == TargetFactKind.Return ? -f.Amount : 0m);
            case TargetMetrics.Collection:
                return facts.Where(f => f.Kind == TargetFactKind.Collection).Sum(f => f.Amount);
            case TargetMetrics.DocumentCount:
                return DocumentCount(facts);
            default:
                var quantity = measure == TargetMeasures.Quantity;
                return facts.Where(f => f.Kind != TargetFactKind.Collection && Matches(metric, itemCode, f.StockCode))
                    .Sum(f => (f.Kind == TargetFactKind.Return ? -1 : 1) * (quantity ? f.Quantity : f.Amount));
        }
    }

    private bool Matches(string metric, string itemCode, string? stockCode)
    {
        if (stockCode is null) return false;
        if (metric == TargetMetrics.Product) return string.Equals(stockCode, itemCode, StringComparison.OrdinalIgnoreCase);
        if (!_facts.Products.TryGetValue(stockCode, out var product)) return false;
        var code = metric switch
        {
            TargetMetrics.Category => product.MainGroup,
            TargetMetrics.SubCategory => product.SubGroup is null ? null : $"{product.MainGroup}|{product.SubGroup}",
            TargetMetrics.Brand => product.Brand,
            _ => null,
        };
        return code is not null && string.Equals(code, itemCode, StringComparison.OrdinalIgnoreCase);
    }

    private string? ItemName(string metric, string itemCode) =>
        metric == TargetMetrics.Product && _facts.Products.TryGetValue(itemCode, out var product) ? product.Name
        : itemCode.Length > 0 ? itemCode : null;

    private (int Planned, int Completed) VisitCounts(IReadOnlySet<Guid>? users, DateOnly from, DateOnly to)
    {
        var planned = 0;
        var completed = 0;
        foreach (var (user, visits) in _facts.Visits)
        {
            if (users is not null && !users.Contains(user)) continue;
            foreach (var visit in visits)
            {
                if (visit.Day < from || visit.Day > to) continue;
                if (visit.Planned) planned++;
                if (visit.Completed) completed++;
            }
        }
        return (planned, completed);
    }

    private static decimal Sum(IEnumerable<TargetFact> facts, TargetFactKind kind) => facts.Where(f => f.Kind == kind).Sum(f => f.Amount);

    private static int DocumentCount(IEnumerable<TargetFact> facts) =>
        facts.Where(f => f.Kind == TargetFactKind.Sale && f.DocumentKey is not null).Select(f => f.DocumentKey).Distinct(StringComparer.Ordinal).Count();

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;
}
