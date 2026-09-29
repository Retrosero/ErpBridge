using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Targets;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Targets;

/// <summary>Measuring targets (GOAL_HEDEF_RUT K6–K11) without a database.</summary>
public sealed class TargetBoardTests
{
    // Thursday; October 2026 has 27 working days Monday–Saturday.
    private static readonly DateOnly Today = new(2026, 10, 15);
    private static readonly TargetPeriod October = TargetPeriod.Of(TargetPeriodTypes.Monthly, Today);
    private static readonly TargetPeriod Day = TargetPeriod.Of(TargetPeriodTypes.Daily, Today);

    private static readonly Guid Ali = Guid.NewGuid();
    private static readonly Guid Veli = Guid.NewGuid();
    private static readonly Guid Ayse = Guid.NewGuid();
    private static readonly Guid Region = Guid.NewGuid();
    private static readonly Guid TeamA = Guid.NewGuid();
    private static readonly Guid TeamB = Guid.NewGuid();

    private static TeamDirectory Directory() => new()
    {
        Teams = new Dictionary<Guid, SalesTeam>
        {
            [Region] = new() { Id = Region, Name = "Marmara", Kind = SalesTeamKinds.Region },
            [TeamA] = new() { Id = TeamA, Name = "A", Kind = SalesTeamKinds.Team, ParentId = Region },
            [TeamB] = new() { Id = TeamB, Name = "B", Kind = SalesTeamKinds.Team },
        },
        TeamOfUser = new Dictionary<Guid, Guid> { [Ali] = TeamA, [Veli] = TeamA, [Ayse] = TeamB },
        ManagersOf = new Dictionary<Guid, IReadOnlyList<Guid>>(),
    };

    private static PortalStockCatalog.Product Product(string code, string group, string brand) =>
        new(code, code + " adı", "ADET", group, "ALT", brand, null, [], new Dictionary<int, (decimal, decimal)>(), new Dictionary<int, decimal>(), null, 20m);

    private static TargetFactSet Facts(IEnumerable<TargetFact> facts, IEnumerable<TargetFact>? pending = null,
        Dictionary<Guid, IReadOnlyList<(DateOnly, bool, bool)>>? visits = null, Dictionary<string, Guid>? salespeople = null) => new()
    {
        Source = TargetSources.ServerDocuments,
        Facts = facts.ToList(),
        Pending = (pending ?? []).ToList(),
        UserOfSalesperson = salespeople ?? [],
        UsersWithoutSalesperson = new HashSet<Guid>(),
        Visits = visits ?? [],
        Products = new Dictionary<string, PortalStockCatalog.Product>(StringComparer.OrdinalIgnoreCase)
        {
            ["CAY"] = Product("CAY", "ICECEK", "RIZE"),
            ["SEKER"] = Product("SEKER", "GIDA", "TAT"),
        },
    };

    private static TargetFact Sale(Guid user, DateOnly day, decimal amount, string? stock = null, decimal quantity = 0m, string? doc = null) =>
        new(day, TargetFactKind.Sale, user, null, stock, amount, quantity, doc ?? Guid.NewGuid().ToString());

    private static SalesTarget Target(string owner, Guid ownerId, TargetPeriod period, string metric, decimal value, string item = "", string measure = TargetMeasures.Amount) => new()
    {
        OwnerKind = owner, OwnerId = ownerId, PeriodType = period.Type, PeriodKey = period.Key, Metric = metric, Measure = measure, ItemCode = item, Value = value,
    };

    private static TargetBoard Board(TargetFactSet facts, params SalesTarget[] targets) =>
        new(facts, Directory(), targets, new Dictionary<Guid, string>(), TargetSettings.DefaultWorkDays, Today);

    [Fact]
    public void A_day_without_its_own_target_gets_its_share_of_what_the_month_has_left()
    {
        var board = Board(Facts([Sale(Ali, new DateOnly(2026, 10, 3), 10_000m), Sale(Ali, Today, 500m)]),
            Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Revenue, 30_000m));

        var row = board.Build(TargetOwnerKinds.User, Ali, "Ali", Day).Targets.Single(t => t.Metric == TargetMetrics.Revenue);

        // 20.000 left before today, over the 15 working days from the 15th to the 31st (two Sundays out).
        row.Derived.Should().BeTrue();
        row.Value.Should().Be(1333.33m);
        row.Actual.Should().Be(500m, "today's sale counts for today, not for the share");
    }

    [Fact]
    public void A_day_with_its_own_target_keeps_it()
    {
        var board = Board(Facts([]),
            Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Revenue, 30_000m),
            Target(TargetOwnerKinds.User, Ali, Day, TargetMetrics.Revenue, 2_000m));

        board.Build(TargetOwnerKinds.User, Ali, "Ali", Day).Targets.Should().ContainSingle(t => t.Metric == TargetMetrics.Revenue)
            .Which.Should().Match<Contracts.TargetRowDto>(t => !t.Derived && t.Value == 2_000m);
    }

    [Fact]
    public void Pace_forecast_and_the_need_per_day()
    {
        // 13 working days elapsed by Thursday the 15th (1–15 minus the 4th and 11th), 15 left with today.
        var board = Board(Facts([Sale(Ali, new DateOnly(2026, 10, 2), 13_000m)]),
            Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Revenue, 27_000m));

        var row = board.Build(TargetOwnerKinds.User, Ali, "Ali", October).Targets.Single();

        row.Percent.Should().Be(48.1m);
        row.ExpectedToDate.Should().Be(13_000m);
        row.Forecast.Should().Be(27_000m);
        row.RequiredPerDay.Should().Be(933.33m, "14.000 left over 15 working days, today included");
    }

    [Fact]
    public void Returns_count_against_revenue_and_item_targets_in_amount_or_quantity()
    {
        var facts = Facts(
        [
            Sale(Ali, Today, 100m, "CAY", 10m),
            Sale(Ali, Today, 40m, "SEKER", 4m),
            new TargetFact(Today, TargetFactKind.Return, Ali, null, "CAY", 20m, 2m, "R1"),
            new TargetFact(Today, TargetFactKind.Collection, Ali, null, null, 75m, 0m, "T1"),
        ]);
        var board = Board(facts,
            Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Brand, 500m, "RIZE"),
            Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Category, 50m, "ICECEK", TargetMeasures.Quantity),
            Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Product, 10m, "seker", TargetMeasures.Quantity),
            Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Collection, 1_000m));

        var owner = board.Build(TargetOwnerKinds.User, Ali, "Ali", October);

        owner.Summary.Revenue.Should().Be(120m);
        owner.Summary.Returns.Should().Be(20m);
        owner.Summary.Collection.Should().Be(75m);
        owner.Summary.DocumentCount.Should().Be(2);
        owner.Targets.Single(t => t.Metric == TargetMetrics.Brand).Actual.Should().Be(80m);
        owner.Targets.Single(t => t.Metric == TargetMetrics.Category).Actual.Should().Be(8m);
        owner.Targets.Single(t => t.Metric == TargetMetrics.Product).Actual.Should().Be(4m, "codes match without regard to case");
    }

    [Fact]
    public void A_team_is_its_members_a_region_its_teams_and_the_company_everything()
    {
        var unattributed = new TargetFact(Today, TargetFactKind.Sale, null, "OFIS", null, 1_000m, 0m, "O1");
        var board = Board(Facts([Sale(Ali, Today, 100m), Sale(Veli, Today, 200m), Sale(Ayse, Today, 400m), unattributed]),
            Target(TargetOwnerKinds.Team, TeamA, October, TargetMetrics.Revenue, 900m),
            Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Revenue, 500m),
            Target(TargetOwnerKinds.User, Veli, October, TargetMetrics.Revenue, 300m));

        board.Build(TargetOwnerKinds.Team, TeamA, "A", October).Summary.Revenue.Should().Be(300m);
        board.Build(TargetOwnerKinds.Team, Region, "Marmara", October).Summary.Revenue.Should().Be(300m);
        board.Build(TargetOwnerKinds.Team, TeamB, "B", October).Summary.Revenue.Should().Be(400m);
        board.Build(TargetOwnerKinds.Company, Guid.Empty, "Firma", October).Summary.Revenue.Should().Be(1_700m, "office sales nobody owns still count for the company");
        board.Build(TargetOwnerKinds.Team, TeamA, "A", October).Targets.Single().ChildrenSum.Should().Be(800m, "members' targets add up to 800 of the team's 900");
    }

    [Fact]
    public void Planned_route_stops_are_the_visit_target_when_none_is_entered()
    {
        var visits = new Dictionary<Guid, IReadOnlyList<(DateOnly, bool, bool)>>
        {
            [Ali] = [(Today, true, true), (Today, true, false), (Today.AddDays(1), true, false), (Today, false, true)],
        };
        var board = Board(Facts([], visits: visits));

        var dayRow = board.Build(TargetOwnerKinds.User, Ali, "Ali", Day).Targets.Single(t => t.Metric == TargetMetrics.Visit);
        var monthRow = board.Build(TargetOwnerKinds.User, Ali, "Ali", October).Targets.Single(t => t.Metric == TargetMetrics.Visit);

        dayRow.Derived.Should().BeTrue();
        dayRow.Value.Should().Be(2m);
        dayRow.Actual.Should().Be(2m, "a visit outside the plan still happened");
        monthRow.Value.Should().Be(3m);
    }

    [Fact]
    public void Documents_on_their_way_to_the_erp_show_apart()
    {
        var pending = Sale(Ali, Today, 250m);
        var board = Board(Facts([Sale(Ali, Today, 100m)], [pending]), Target(TargetOwnerKinds.User, Ali, October, TargetMetrics.Revenue, 1_000m));

        var owner = board.Build(TargetOwnerKinds.User, Ali, "Ali", October);

        owner.Summary.Revenue.Should().Be(100m);
        owner.Summary.PendingRevenue.Should().Be(250m);
        owner.Targets.Single().Pending.Should().Be(250m);
    }

    [Fact]
    public void Mikro_rows_belong_to_the_user_whose_salesperson_code_they_carry()
    {
        var facts = Facts([new TargetFact(Today, TargetFactKind.Sale, null, "PL01", null, 300m, 0m, "r1")], salespeople: new() { ["PL01"] = Ali });

        Board(facts).Build(TargetOwnerKinds.User, Ali, "Ali", October).Summary.Revenue.Should().Be(300m);
        Board(facts).Build(TargetOwnerKinds.User, Veli, "Veli", October).Summary.Revenue.Should().Be(0m);
    }
}
