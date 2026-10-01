using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.SuspendedSales;
using ErpBridge.CentralApi.Tasks;
using FluentAssertions;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;
using R = ErpBridge.CentralApi.Domain.MobileUserRoles;

namespace ErpBridge.CentralApi.Tests.Permissions;

/// <summary>
/// GOAL_YETKILER S1: the permission catalogue and its resolver. The first test is the promise the whole change rests
/// on: a company that changed nothing gets exactly what the hard-coded role checks gave before.
/// </summary>
public sealed class PermissionResolverTests
{
    private static readonly IReadOnlyDictionary<string, string> NoOverrides = new Dictionary<string, string>();

    public static IEnumerable<object[]> EveryRoleCombination()
    {
        var roles = R.All.ToArray();
        for (var mask = 1; mask < 1 << roles.Length; mask++)
        {
            var set = roles.Where((_, i) => (mask & (1 << i)) != 0).ToArray();
            foreach (var canApprove in new[] { false, true })
                foreach (var canManageRules in new[] { false, true })
                    yield return [set, canApprove, canManageRules];
        }
    }

    [Theory]
    [MemberData(nameof(EveryRoleCombination))]
    public void Without_company_changes_every_role_combination_keeps_todays_rights(string[] roles, bool canApprove, bool canManageRules)
    {
        var user = new MobileUser
        {
            Role = MobileUserRoles.Legacy(roles),
            Roles = roles.Select(r => new MobileUserRole { Role = r }).ToList(),
            CanApprove = canApprove,
            CanManageApprovalRules = canManageRules,
        };

        var p = PermissionResolver.Resolve(user, [], NoOverrides);

        p.Can(K.UsersManage).Should().Be(RolePermissions.CanManageUsers(user));
        p.Can(K.PortalReports).Should().Be(RolePermissions.CanViewReports(user));
        p.Can(K.PortalLedger).Should().Be(RolePermissions.CanViewLedger(user));
        p.Can(K.RoutePlan).Should().Be(RolePermissions.CanPlanRoutes(user));
        p.Can(K.TargetsManage).Should().Be(RolePermissions.CanManageTargets(user));
        p.Can(K.ModuleWarehouseQueue).Should().Be(RolePermissions.CanOperateWarehouse(user));
        p.Can(K.WarehouseManage).Should().Be(RolePermissions.CanManageWarehouse(user));
        p.Can(K.NativeBooksEdit).Should().Be(RolePermissions.CanEditNativeData(user));
        p.Can(K.ProductsEdit).Should().Be(RolePermissions.IsAdmin(user), "product cards from the phone were admin-only in the native processor");
        p.Can(K.TasksManage).Should().Be(TaskService.CanManage(user));
        p.Can(K.SuspendedSalesManageOthers).Should().Be(SuspendedSaleService.CanManage(user));
        p.Can(K.ApprovalsDecide).Should().Be(ApprovalPermissions.CanDecide(user));
        p.Can(K.ApprovalsManageRules).Should().Be(ApprovalPermissions.CanManageRules(user));
        p.Can(K.ModuleSales).Should().Be(roles.Any(r => r is R.Admin or R.Manager or R.Sales), "field screens are for admin, manager and sales");
        p.Can(K.CustomerCatalogManage).Should().Be(roles.Any(r => r is R.Admin or R.Manager), "the web catalog is managed by admin and manager only");
        RolePermissions.CanManageCustomerCatalog(user).Should().Be(p.Can(K.CustomerCatalogManage));
        foreach (var limit in PermissionCatalog.All.Where(d => d.Type == PermissionType.Limit))
            p.Limit(limit.Key).Should().BeNull("no limit exists today");
    }

    [Fact]
    public void An_administrator_has_everything_and_no_override_takes_it_away()
    {
        var overrides = new Dictionary<string, string> { [K.ModuleSales] = PermissionValues.False, [K.LimitSaleAmount] = "100" };
        var templates = new[] { new RoleTemplateValue(R.Admin, K.ModuleSales, PermissionValues.False) };

        var p = PermissionResolver.Resolve([R.Admin, R.Sales], templates, overrides);

        p.IsAdmin.Should().BeTrue();
        p.Can(K.ModuleSales).Should().BeTrue();
        p.Limit(K.LimitSaleAmount).Should().BeNull();
        p.Entry(K.ModuleSales).Source.Should().Be(PermissionSource.Admin);
        p.OverrideCount.Should().Be(0);
    }

    [Fact]
    public void A_company_template_changes_the_role_and_a_personal_override_beats_it()
    {
        var templates = new[] { new RoleTemplateValue(R.Sales, K.ModuleReports, PermissionValues.False) };

        var plain = PermissionResolver.Resolve([R.Sales], templates, NoOverrides);
        plain.Can(K.ModuleReports).Should().BeFalse();
        plain.Entry(K.ModuleReports).Source.Should().Be(PermissionSource.Role);

        var allowed = PermissionResolver.Resolve([R.Sales], templates, new Dictionary<string, string> { [K.ModuleReports] = PermissionValues.True });
        allowed.Can(K.ModuleReports).Should().BeTrue();
        var entry = allowed.Entry(K.ModuleReports);
        entry.Source.Should().Be(PermissionSource.Override);
        entry.RoleValue.Should().Be(PermissionValues.False, "the editor shows what the role alone gives");
        allowed.OverrideCount.Should().Be(1);
    }

    [Fact]
    public void Roles_only_add_a_grant_from_any_role_wins_and_names_its_role()
    {
        var templates = new[] { new RoleTemplateValue(R.Sales, K.ModuleReports, PermissionValues.False) };

        var p = PermissionResolver.Resolve([R.Sales, R.Manager], templates, NoOverrides);

        p.Can(K.ModuleReports).Should().BeTrue();
        p.Entry(K.ModuleReports).RoleSources.Should().Equal(R.Manager);
    }

    [Fact]
    public void Limits_take_the_highest_role_and_unlimited_beats_a_number()
    {
        var templates = new[]
        {
            new RoleTemplateValue(R.Sales, K.LimitSaleLineDiscountPct, "10"),
            new RoleTemplateValue(R.Warehouse, K.LimitSaleLineDiscountPct, "15"),
        };

        PermissionResolver.Resolve([R.Sales, R.Warehouse], templates, NoOverrides).Limit(K.LimitSaleLineDiscountPct).Should().Be(15m);
        PermissionResolver.Resolve([R.Sales, R.Manager], templates, NoOverrides).Limit(K.LimitSaleLineDiscountPct).Should().BeNull();

        var personal = PermissionResolver.Resolve([R.Sales], templates, new Dictionary<string, string> { [K.LimitSaleLineDiscountPct] = "5" });
        personal.Limit(K.LimitSaleLineDiscountPct).Should().Be(5m);
        personal.Entry(K.LimitSaleLineDiscountPct).RoleValue.Should().Be("10");
    }

    [Fact]
    public void The_session_maps_hold_every_key_of_their_type()
    {
        var p = PermissionResolver.Resolve([R.Sales], [], NoOverrides);

        p.Flags().Keys.Should().BeEquivalentTo(PermissionCatalog.All.Where(d => d.Type == PermissionType.Bool).Select(d => d.Key));
        p.Limits().Keys.Should().BeEquivalentTo(PermissionCatalog.All.Where(d => d.Type == PermissionType.Limit).Select(d => d.Key));
        p.Entries.Select(e => e.Key).Should().Equal(PermissionCatalog.All.Select(d => d.Key));
    }

    [Fact]
    public void The_catalogue_is_well_formed()
    {
        PermissionCatalog.All.Select(d => d.Key).Should().OnlyHaveUniqueItems();
        foreach (var definition in PermissionCatalog.All)
        {
            definition.Defaults.Keys.Should().BeEquivalentTo(R.All, $"{definition.Key} needs a default for every role");
            definition.Label.Should().NotBeNullOrWhiteSpace();
            definition.Description.Should().NotBeNullOrWhiteSpace();
            if (definition.Type == PermissionType.Bool)
                definition.Defaults[R.Admin].Should().Be(PermissionValues.True, $"{definition.Key}: admin has everything");
        }
    }

    [Theory]
    [InlineData(K.ModuleSales, "allow", "1")]
    [InlineData(K.ModuleSales, "0", "0")]
    [InlineData(K.ModuleSales, "maybe", null)]
    [InlineData(K.LimitSaleLineDiscountPct, "12.5", "12.5")]
    [InlineData(K.LimitSaleLineDiscountPct, "120", null)]
    [InlineData(K.LimitSaleLineDiscountPct, "-1", null)]
    [InlineData(K.LimitSaleAmount, "unlimited", "")]
    [InlineData(K.LimitSaleAmount, "25000", "25000")]
    public void Values_are_normalised_or_refused(string key, string input, string? expected)
    {
        PermissionValues.Normalize(PermissionCatalog.Find(key)!, input).Should().Be(expected);
    }
}
