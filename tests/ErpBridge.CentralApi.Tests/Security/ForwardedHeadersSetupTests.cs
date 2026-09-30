using System.Net;
using ErpBridge.CentralApi.Security;
using FluentAssertions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;

namespace ErpBridge.CentralApi.Tests.Security;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §6: the real client address behind Traefik, and the rate-limit partition built from it.
/// </summary>
public sealed class ForwardedHeadersSetupTests
{
    [Fact]
    public void Without_a_proxy_list_the_middleware_is_not_used()
    {
        ForwardedHeadersSetup.FromConfiguration(Config()).Should().BeNull();
        ForwardedHeadersSetup.FromConfiguration(Config(("ForwardedHeaders:KnownNetworks:0", " "))).Should().BeNull("a blank Coolify variable is no proxy");
    }

    [Fact]
    public void Only_the_listed_proxies_are_trusted_one_hop_deep_and_never_for_the_host()
    {
        var options = ForwardedHeadersSetup.FromConfiguration(Config(
            ("ForwardedHeaders:KnownNetworks:0", "10.0.1.0/24"),
            ("ForwardedHeaders:KnownNetworks:1", "fd00::/64"),
            ("ForwardedHeaders:KnownProxies:0", "172.18.0.5")))!;

        options.ForwardedHeaders.Should().Be(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
        options.ForwardLimit.Should().Be(1);
        options.KnownIPNetworks.Should().Equal(System.Net.IPNetwork.Parse("10.0.1.0/24"), System.Net.IPNetwork.Parse("fd00::/64"));
        options.KnownProxies.Should().Equal(IPAddress.Parse("172.18.0.5"));
        options.KnownIPNetworks.Should().NotContain(n => n.Contains(IPAddress.Loopback), "the loopback default is cleared");
    }

    [Fact]
    public void A_comma_separated_value_works_and_a_malformed_one_stops_startup()
    {
        ForwardedHeadersSetup.FromConfiguration(Config(("ForwardedHeaders:KnownNetworks", "10.0.0.0/8, 172.16.0.0/12")))!
            .KnownIPNetworks.Should().HaveCount(2);
        // The web host reports an empty value for a key that only has children; the children still count.
        ForwardedHeadersSetup.FromConfiguration(Config(("ForwardedHeaders:KnownNetworks", ""), ("ForwardedHeaders:KnownNetworks:0", "10.0.1.0/24")))!
            .KnownIPNetworks.Should().ContainSingle();

        var badNetwork = () => ForwardedHeadersSetup.FromConfiguration(Config(("ForwardedHeaders:KnownNetworks:0", "10.0.0.0/33")));
        badNetwork.Should().Throw<InvalidOperationException>().WithMessage("*ForwardedHeaders:KnownNetworks*10.0.0.0/33*");
        var badProxy = () => ForwardedHeadersSetup.FromConfiguration(Config(("ForwardedHeaders:KnownProxies:0", "traefik")));
        badProxy.Should().Throw<InvalidOperationException>().WithMessage("*ForwardedHeaders:KnownProxies*traefik*");
    }

    [Theory]
    [InlineData("203.0.113.7", "203.0.113.7")]
    [InlineData("::ffff:203.0.113.7", "203.0.113.7")]
    [InlineData("2001:db8:1:2:3:4:5:6", "2001:db8:1:2::/64")]
    [InlineData("2001:db8:1:2:ffff::9", "2001:db8:1:2::/64")]
    [InlineData("2001:db8:1:3::1", "2001:db8:1:3::/64")]
    public void An_IPv6_caller_is_partitioned_by_its_64_prefix(string address, string partition)
    {
        ClientIpPartition.Of(IPAddress.Parse(address)).Should().Be(partition);
    }

    [Fact]
    public void An_unknown_address_has_its_own_partition()
    {
        ClientIpPartition.Of(null).Should().Be(ClientIpPartition.Unknown);
    }

    private static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value))).Build();
}
