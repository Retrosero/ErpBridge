using ErpBridge.Agent.Service.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ErpBridge.Agent.Service.Tests.Configuration;

/// <summary>Ajan hızı A1/A2: the new knobs reach the pump and the loop from appsettings' <c>AgentService</c> section.</summary>
public class AgentServiceOptionsTests
{
    [Fact]
    public void Defaults_turn_the_long_poll_and_the_write_kick_on()
    {
        var options = new AgentServiceOptions();

        options.ToJobPumpOptions().LongPollWaitSeconds.Should().Be(25);
        options.ToSyncLoopOptions().KickMinGapSeconds.Should().Be(5);
    }

    [Fact]
    public void The_section_values_are_projected()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AgentService:JobLongPollWaitSeconds"] = "0",
            ["AgentService:SyncKickMinGapSeconds"] = "12",
        }).Build();
        var options = new AgentServiceOptions();
        configuration.GetSection(AgentServiceOptions.SectionName).Bind(options);

        options.ToJobPumpOptions().LongPollWaitSeconds.Should().Be(0);
        options.ToSyncLoopOptions().KickMinGapSeconds.Should().Be(12);
    }
}
