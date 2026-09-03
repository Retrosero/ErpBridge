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

    public static bool IsPublicAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            return false;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] switch
            {
                0 or 10 or 127 => false,
                100 when bytes[1] is >= 64 and <= 127 => false,
                169 when bytes[1] == 254 => false,
                172 when bytes[1] is >= 16 and <= 31 => false,
                192 when bytes[1] == 168 => false,
                >= 224 => false,
                _ => true,
            };
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            // ::/128, fc00::/7 (unique local), fe80::/10 (link local),
            // and ff00::/8 (multicast) are never valid public targets.
            return !address.Equals(IPAddress.IPv6None)
                && !address.IsIPv6LinkLocal
                && !address.IsIPv6Multicast
                && (bytes[0] & 0xFE) != 0xFC;
        }

        return false;
    }
}
