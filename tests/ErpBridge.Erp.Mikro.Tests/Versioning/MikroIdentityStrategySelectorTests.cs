using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Versioning;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Versioning;

public class MikroIdentityStrategySelectorTests
{
    private static MikroIdentityStrategySelector NewSelector() => new(NullLogger<MikroIdentityStrategySelector>.Instance);

    [Fact]
    public void V16_resolves_to_guid_strategy()
    {
        var selector = NewSelector();
        var info = new ErpVersionInfo(MikroVersion.V16, "16.0.1.7", "MIKRO16", DateTime.UtcNow);

        var strategy = selector.GetFor("MIKRO16", info);

        strategy.Should().BeOfType<GuidStrategy>();
    }

    [Fact]
    public void V15_resolves_to_recno_strategy()
    {
        var selector = NewSelector();
        var info = new ErpVersionInfo(MikroVersion.V15, "15.0.2000.0", "MIKRO15", DateTime.UtcNow);

        var strategy = selector.GetFor("MIKRO15", info);

        strategy.Should().BeOfType<RecnoStrategy>();
    }

    [Fact]
    public void Unknown_resolves_to_recno_strategy_by_default()
    {
        var selector = NewSelector();
        var info = new ErpVersionInfo(MikroVersion.Unknown, null, "MIKRO?", DateTime.UtcNow);

        var strategy = selector.GetFor("MIKRO?", info);

        strategy.Should().BeOfType<RecnoStrategy>();
    }

    [Fact]
    public void Caches_by_database_name()
    {
        var selector = NewSelector();
        var info = new ErpVersionInfo(MikroVersion.V16, "16.x", "MIKRO_A", DateTime.UtcNow);

        var first = selector.GetFor("MIKRO_A", info);
        var second = selector.GetFor("MIKRO_A", info);

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void Invalidate_forces_re_evaluation()
    {
        var selector = NewSelector();
        var v15Info = new ErpVersionInfo(MikroVersion.V15, "15.x", "MIKRO_X", DateTime.UtcNow);
        var v16Info = new ErpVersionInfo(MikroVersion.V16, "16.x", "MIKRO_X", DateTime.UtcNow);

        selector.GetFor("MIKRO_X", v15Info).Should().BeOfType<RecnoStrategy>();
        selector.Invalidate("MIKRO_X");
        selector.GetFor("MIKRO_X", v16Info).Should().BeOfType<GuidStrategy>();
    }

    [Fact]
    public void GetFor_rejects_blank_database_name()
    {
        var selector = NewSelector();
        var info = new ErpVersionInfo(MikroVersion.V15, "15.x", "MIKRO", DateTime.UtcNow);

        Action act = () => selector.GetFor(" ", info);

        act.Should().Throw<ArgumentException>();
    }
}
