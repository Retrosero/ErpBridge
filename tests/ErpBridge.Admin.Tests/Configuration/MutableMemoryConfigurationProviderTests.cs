using ErpBridge.Agent.UI.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace ErpBridge.Admin.Tests.Configuration;

public class MutableMemoryConfigurationProviderTests
{
    [Fact]
    public void Setting_value_notifies_configuration_consumers()
    {
        var provider = new MutableMemoryConfigurationProvider();
        var reloaded = false;
        using var registration = ChangeToken.OnChange(provider.GetReloadToken, () => reloaded = true);

        provider["CentralApi:Jwt"] = "new-token";

        reloaded.Should().BeTrue();
    }
}
