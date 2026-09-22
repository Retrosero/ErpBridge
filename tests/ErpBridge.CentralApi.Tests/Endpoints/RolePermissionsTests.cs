using ErpBridge.CentralApi.Domain;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// <see cref="RolePermissions.CanEditNativeData"/> (GOAL_PANEL_ERPSIZ E7a): writing product/customer
/// cards, sales/purchase/return documents, collections and ledger corrections from the portal for an
/// ERP-less tenant is admin-only by user decision (2026-09-21) — no manager/accounting carve-out.
/// </summary>
public sealed class RolePermissionsTests
{
    [Theory]
    [InlineData(MobileUserRoles.Admin, true)]
    [InlineData(MobileUserRoles.Manager, false)]
    [InlineData(MobileUserRoles.Accounting, false)]
    [InlineData(MobileUserRoles.Warehouse, false)]
    [InlineData(MobileUserRoles.Sales, false)]
    public void Only_admin_may_edit_native_data(string role, bool expected)
    {
        var user = new MobileUser { Role = role, Roles = [new MobileUserRole { Role = role }] };

        RolePermissions.CanEditNativeData(user).Should().Be(expected);
    }

    [Fact]
    public void Admin_combined_with_other_roles_still_may_edit_native_data()
    {
        var user = new MobileUser
        {
            Role = MobileUserRoles.Admin,
            Roles = [new MobileUserRole { Role = MobileUserRoles.Admin }, new MobileUserRole { Role = MobileUserRoles.Warehouse }],
        };

        RolePermissions.CanEditNativeData(user).Should().BeTrue();
    }
}
