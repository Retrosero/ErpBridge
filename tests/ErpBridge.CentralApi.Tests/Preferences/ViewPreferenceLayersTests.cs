using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Preferences;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Preferences;

/// <summary>
/// Görünüm katmanları (KB kural 35): bir kişinin rollerinin varsayılanları ve kilitleri düşük öncelikten yükseğe birleşir,
/// kişinin kendi kilitleri en son gelir; damga sürümlerden üretilir.
/// </summary>
public sealed class ViewPreferenceLayersTests
{
    private static TenantRoleViewPreference Template(string role, string json, string locks = "{}", long version = 1) =>
        new() { Role = role, Json = json, LocksJson = locks, Version = version };

    [Fact]
    public void A_higher_role_wins_where_two_roles_disagree()
    {
        var merged = ViewPreferenceLayers.Merge(
            [MobileUserRoles.Sales, MobileUserRoles.Manager],
            [
                Template(MobileUserRoles.Sales, """{"sales.viewMode":"Grid","home.showRouteCard":false}"""),
                Template(MobileUserRoles.Manager, """{"sales.viewMode":"List"}""")
            ],
            userLocksJson: null,
            userLocksVersion: 0);

        merged.Data["sales.viewMode"]!.GetValue<string>().Should().Be("List");
        merged.Data["home.showRouteCard"]!.GetValue<bool>().Should().BeFalse("only the salesperson role sets it");
    }

    [Fact]
    public void Templates_of_roles_the_user_does_not_hold_are_ignored()
    {
        var merged = ViewPreferenceLayers.Merge(
            [MobileUserRoles.Sales],
            [Template(MobileUserRoles.Manager, """{"sales.viewMode":"List"}""", """{"home.visibleModules#reports":false}""")],
            null, 0);

        merged.Data.Count.Should().Be(0);
        merged.Locks.Count.Should().Be(0);
    }

    [Fact]
    public void The_persons_own_lock_wins_over_a_role_lock()
    {
        var merged = ViewPreferenceLayers.Merge(
            [MobileUserRoles.Sales],
            [Template(MobileUserRoles.Sales, "{}", """{"sales.viewMode":"Grid","home.showTargetCard":true}""")],
            """{"sales.viewMode":"Detail"}""", 3);

        merged.Locks["sales.viewMode"]!.GetValue<string>().Should().Be("Detail");
        merged.Locks["home.showTargetCard"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void The_stamp_names_every_role_with_its_template_version_and_the_persons_locks()
    {
        var merged = ViewPreferenceLayers.Merge(
            [MobileUserRoles.Sales, MobileUserRoles.Manager],
            [Template(MobileUserRoles.Sales, "{}", version: 4)],
            "{}", 2);

        merged.Stamp.Should().Be("MANAGER:0,SALES:4|u:2");
    }

    [Fact]
    public void A_broken_document_adds_nothing()
    {
        var merged = ViewPreferenceLayers.Merge([MobileUserRoles.Sales], [Template(MobileUserRoles.Sales, "{bozuk", "[1]")], "null", 0);

        merged.Data.Count.Should().Be(0);
        merged.Locks.Count.Should().Be(0);
    }
}
