using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace ErpBridge.CentralApi.Security;

/// <summary>
/// The real client address behind Traefik (GOAL_MUSTERI_KATALOGU §6). Without it every caller shares the proxy's
/// address: the anonymous and global rate limits become one bucket for everybody. Only <c>X-Forwarded-For</c> and
/// <c>X-Forwarded-Proto</c> are read, one hop deep, and only from the proxies the operator names;
/// <c>X-Forwarded-Host</c> is never trusted (the catalog host check would be bypassed).
/// </summary>
public static class ForwardedHeadersSetup
{
    public const string KnownNetworksKey = "ForwardedHeaders:KnownNetworks";
    public const string KnownProxiesKey = "ForwardedHeaders:KnownProxies";

    /// <summary>
    /// Options for <c>UseForwardedHeaders</c>, or null when no proxy is configured — the middleware is then not
    /// added and the connection address is used as before. A malformed entry throws, so a typo stops startup
    /// instead of silently trusting nobody.
    /// </summary>
    public static ForwardedHeadersOptions? FromConfiguration(IConfiguration cfg)
    {
        var networks = Values(cfg, KnownNetworksKey);
        var proxies = Values(cfg, KnownProxiesKey);
        if (networks.Count == 0 && proxies.Count == 0) return null;

        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1,
        };
        // The defaults trust loopback; only what the operator lists is trusted.
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
        foreach (var network in networks)
        {
            options.KnownIPNetworks.Add(System.Net.IPNetwork.TryParse(network, out var parsed)
                ? parsed
                : throw new InvalidOperationException($"{KnownNetworksKey} entry '{network}' is not a network in CIDR form (e.g. 10.0.1.0/24)."));
        }
        foreach (var proxy in proxies)
        {
            options.KnownProxies.Add(IPAddress.TryParse(proxy, out var parsed)
                ? parsed
                : throw new InvalidOperationException($"{KnownProxiesKey} entry '{proxy}' is not an IP address."));
        }
        return options;
    }

    /// <summary>Array entries (<c>…__0</c>) or one comma-separated value; blank entries are ignored.</summary>
    private static List<string> Values(IConfiguration cfg, string key)
    {
        // Children first: some providers report an empty value for a key that only has children.
        var section = cfg.GetSection(key);
        var children = section.GetChildren().Select(c => c.Value ?? string.Empty).ToList();
        IEnumerable<string> values = children.Count > 0 ? children : (section.Value ?? string.Empty).Split(',');
        return values.Select(v => v.Trim()).Where(v => v.Length > 0).ToList();
    }
}
