using ErpBridge.Erp.Mikro.Versioning;
using FluentAssertions;

namespace ErpBridge.Erp.Mikro.Tests.Versioning;

public class RecnoStrategyTests
{
    [Fact]
    public void DisplayName_mentions_recno()
    {
        var strategy = new RecnoStrategy();

        strategy.DisplayName.Should().Be("V15/RECno");
    }

    [Fact]
    public void SelectScopeIdentitySql_uses_scope_identity()
    {
        var strategy = new RecnoStrategy();

        var sql = strategy.SelectScopeIdentitySql();

        sql.Should().Contain("SCOPE_IDENTITY");
        sql.Should().Contain("INT");
    }

    [Fact]
    public void GenerateNewId_returns_zero()
    {
        var strategy = new RecnoStrategy();

        var id = strategy.GenerateNewId();

        id.Should().Be(0);
    }

    [Fact]
    public void ApplyHeaderLinkFields_sets_sth_sip_RECid_RECno()
    {
        var strategy = new RecnoStrategy();
        var row = new Dictionary<string, object?>();

        strategy.ApplyHeaderLinkFields(row, parentId: 42);

        row.Should().ContainKey("sth_sip_RECid_RECno");
        row["sth_sip_RECid_RECno"].Should().Be(42);
    }

    [Fact]
    public void ApplyHeaderLinkFields_preserves_existing_values()
    {
        var strategy = new RecnoStrategy();
        var row = new Dictionary<string, object?> { ["other"] = "keep" };

        strategy.ApplyHeaderLinkFields(row, parentId: 7);

        row["other"].Should().Be("keep");
        row["sth_sip_RECid_RECno"].Should().Be(7);
    }
}
