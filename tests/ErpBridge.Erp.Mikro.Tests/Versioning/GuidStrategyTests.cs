using ErpBridge.Erp.Mikro.Versioning;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.Versioning;

public class GuidStrategyTests
{
    [Fact]
    public void DisplayName_mentions_guid()
    {
        var strategy = new GuidStrategy();

        strategy.DisplayName.Should().Be("V16/Guid");
    }

    [Fact]
    public void SelectScopeIdentitySql_is_empty()
    {
        var strategy = new GuidStrategy();

        strategy.SelectScopeIdentitySql().Should().BeEmpty();
    }

    [Fact]
    public void GenerateNewId_returns_a_unique_guid()
    {
        var strategy = new GuidStrategy();

        var first = strategy.GenerateNewId();
        var second = strategy.GenerateNewId();

        first.Should().BeOfType<Guid>();
        second.Should().BeOfType<Guid>();
        ((Guid)first!).Should().NotBe((Guid)second!);
    }

    [Fact]
    public void ApplyHeaderLinkFields_sets_sth_sip_uid()
    {
        var strategy = new GuidStrategy();
        var row = new Dictionary<string, object?>();
        var parentGuid = Guid.NewGuid();

        strategy.ApplyHeaderLinkFields(row, parentGuid);

        row.Should().ContainKey("sth_sip_uid");
        row["sth_sip_uid"].Should().Be(parentGuid);
    }

    [Fact]
    public void ApplyHeaderLinkFields_rejects_non_guid_parent()
    {
        var strategy = new GuidStrategy();
        var row = new Dictionary<string, object?>();

        Action act = () => strategy.ApplyHeaderLinkFields(row, parentId: 42);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Guid*");
    }
}
