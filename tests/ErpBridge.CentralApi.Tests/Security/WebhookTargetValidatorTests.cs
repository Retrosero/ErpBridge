using System.Net;
using ErpBridge.CentralApi.Webhooks;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Security;

public sealed class WebhookTargetValidatorTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("10.0.0.10")]
    [InlineData("172.16.0.10")]
    [InlineData("192.168.1.10")]
    [InlineData("169.254.169.254")]
    [InlineData("::1")]
    [InlineData("fc00::1")]
    public void Private_or_special_addresses_are_not_public(string rawAddress)
    {
        WebhookTargetValidator.IsPublicAddress(IPAddress.Parse(rawAddress)).Should().BeFalse();
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("2606:4700:4700::1111")]
    public void Public_unicast_addresses_are_allowed(string rawAddress)
    {
        WebhookTargetValidator.IsPublicAddress(IPAddress.Parse(rawAddress)).Should().BeTrue();
    }
}
