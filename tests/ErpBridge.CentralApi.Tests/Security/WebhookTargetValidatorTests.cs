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
    [InlineData("0.0.0.0")]
    [InlineData("::")]
    [InlineData("255.255.255.255")]
    [InlineData("100.64.0.1")]
    [InlineData("192.0.0.8")]
    [InlineData("198.18.0.1")]
    [InlineData("198.19.255.254")]
    [InlineData("192.0.2.1")]
    [InlineData("203.0.113.9")]
    [InlineData("::ffff:10.0.0.1")]
    [InlineData("::ffff:127.0.0.1")]
    [InlineData("::ffff:169.254.169.254")]
    [InlineData("::10.0.0.1")]
    [InlineData("64:ff9b::a00:1")]
    [InlineData("64:ff9b::7f00:1")]
    [InlineData("64:ff9b:1::808:808")]
    [InlineData("2002:a00:1::1")]
    [InlineData("2001:0:4136:e378:8000:63bf:3fff:fdd2")]
    [InlineData("fe80::1")]
    [InlineData("fec0::1")]
    [InlineData("ff02::1")]
    public void Private_or_special_addresses_are_not_public(string rawAddress)
    {
        WebhookTargetValidator.IsPublicAddress(IPAddress.Parse(rawAddress)).Should().BeFalse();
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("2606:4700:4700::1111")]
    [InlineData("::ffff:8.8.8.8")]
    [InlineData("64:ff9b::808:808")]
    [InlineData("2002:808:808::1")]
    public void Public_unicast_addresses_are_allowed(string rawAddress)
    {
        WebhookTargetValidator.IsPublicAddress(IPAddress.Parse(rawAddress)).Should().BeTrue();
    }
}
