using System.Net;
using System.Net.Sockets;

namespace ErpBridge.CentralApi.Webhooks;

/// <summary>
/// Rejects webhook targets that could reach the API host, its local network,
/// or cloud metadata services. The outbound HTTP client resolves and connects
/// through <see cref="ConnectPublicAsync"/> so it cannot validate one DNS
/// result and then connect to a rebinding result.
/// </summary>
public static class WebhookTargetValidator
{
    public static bool TryParsePublicHttpsUri(string rawUrl, out Uri? uri, out string? error)
    {
        uri = null;
        error = null;
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var parsed)
            || parsed.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(parsed.UserInfo)
            || parsed.Port != 443)
        {
            error = "url must be an absolute https URL without user info or a custom port.";
            return false;
        }

        if (string.Equals(parsed.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || parsed.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
            || parsed.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            error = "url host must be publicly routable.";
            return false;
        }

        if (IPAddress.TryParse(parsed.Host, out var address) && !IsPublicAddress(address))
        {
            error = "url host must be publicly routable.";
            return false;
        }

        uri = parsed;
        return true;
    }

    public static async Task<string?> ValidateResolvedTargetAsync(Uri uri, CancellationToken ct)
    {
        if (IPAddress.TryParse(uri.Host, out var literal))
            return IsPublicAddress(literal) ? null : "Webhook target resolved to a non-public address.";

        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(uri.DnsSafeHost, ct);
        }
        catch (SocketException)
        {
            return "Webhook target hostname could not be resolved.";
        }

        return addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address))
            ? "Webhook target resolved to a non-public address."
            : null;
    }

    /// <summary>
    /// Resolver used by the webhook HTTP handler. It permits only addresses
    /// validated as public and connects the socket to that exact address,
    /// preventing a second DNS lookup from changing the destination.
    /// </summary>
    public static async ValueTask<Stream> ConnectPublicAsync(
        SocketsHttpConnectionContext context,
        CancellationToken ct)
    {
        var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, ct);
        if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
            throw new HttpRequestException("Webhook target resolved to a non-public address.");

        var selected = addresses.First();
        var socket = new Socket(selected.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await socket.ConnectAsync(new IPEndPoint(selected, context.DnsEndPoint.Port), ct);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    /// <summary>
    /// True only for a globally routable unicast address. Rejected: loopback, unspecified, private, carrier-grade NAT,
    /// link-local, IETF protocol assignments (192.0.0.0/24), benchmarking (198.18.0.0/15), documentation, multicast,
    /// reserved and broadcast IPv4; and in IPv6 unique-local, site-local, link-local, multicast, the IPv4-compatible
    /// block, and any IPv6 form that carries an IPv4 address inside (IPv4-mapped <c>::ffff:a.b.c.d</c>, NAT64
    /// <c>64:ff9b::/96</c>, 6to4 <c>2002::/16</c>) whose IPv4 address is not itself public. Local-use NAT64
    /// (<c>64:ff9b:1::/48</c>) and Teredo (<c>2001::/32</c>) are refused outright. Shared by the webhook dispatcher
    /// and the XML picture fetcher (<c>Storage/SafeHttpFetcher</c>).
    /// </summary>
    public static bool IsPublicAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any)
            || address.Equals(IPAddress.Broadcast) || address.Equals(IPAddress.IPv6None))
            return false;

        if (address.AddressFamily == AddressFamily.InterNetwork)
            return IsPublicV4(address.GetAddressBytes());

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv4MappedToIPv6) return IsPublicV4(address.MapToIPv4().GetAddressBytes());
            if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast) return false;
            var bytes = address.GetAddressBytes();
            // fc00::/7 unique local.
            if ((bytes[0] & 0xFE) == 0xFC) return false;
            // ::/96 IPv4-compatible (deprecated; :: and ::1 included).
            if (bytes.AsSpan(0, 12).IndexOfAnyExcept((byte)0) < 0) return false;
            // 64:ff9b::/96 NAT64 well-known prefix: the last four bytes are the IPv4 target.
            if (bytes.AsSpan(0, 12).SequenceEqual(Nat64Prefix)) return IsPublicV4(bytes[12..16]);
            // 64:ff9b:1::/48 local-use NAT64: translated inside some network, never a public target.
            if (bytes[0] == 0x00 && bytes[1] == 0x64 && bytes[2] == 0xff && bytes[3] == 0x9b && bytes[4] == 0x00 && bytes[5] == 0x01) return false;
            // 2001::/32 Teredo tunnels to an address of its own choosing.
            if (bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x00 && bytes[3] == 0x00) return false;
            // 2002::/16 6to4: bytes 2-5 are the IPv4 relay.
            if (bytes[0] == 0x20 && bytes[1] == 0x02) return IsPublicV4(bytes[2..6]);
            return true;
        }

        return false;
    }

    private static readonly byte[] Nat64Prefix = [0x00, 0x64, 0xff, 0x9b, 0, 0, 0, 0, 0, 0, 0, 0];

    private static bool IsPublicV4(byte[] bytes) => bytes[0] switch
    {
        0 or 10 or 127 => false,
        100 when bytes[1] is >= 64 and <= 127 => false,
        169 when bytes[1] == 254 => false,
        172 when bytes[1] is >= 16 and <= 31 => false,
        192 when bytes[1] == 168 => false,
        // 192.0.0.0/24 IETF protocol assignments, 192.0.2.0/24 TEST-NET-1.
        192 when bytes[1] == 0 && bytes[2] is 0 or 2 => false,
        // 198.18.0.0/15 benchmarking, 198.51.100.0/24 TEST-NET-2.
        198 when bytes[1] is 18 or 19 => false,
        198 when bytes[1] == 51 && bytes[2] == 100 => false,
        // 203.0.113.0/24 TEST-NET-3.
        203 when bytes[1] == 0 && bytes[2] == 113 => false,
        // 224/4 multicast, 240/4 reserved, 255.255.255.255 broadcast.
        >= 224 => false,
        _ => true,
    };
}
