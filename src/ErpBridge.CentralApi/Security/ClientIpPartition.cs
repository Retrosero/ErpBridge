using System.Net;
using System.Net.Sockets;

namespace ErpBridge.CentralApi.Security;

/// <summary>
/// The rate-limit partition of a caller's address (GOAL_MUSTERI_KATALOGU §6). An IPv4 address is its own
/// partition; an IPv6 caller gets a whole /64 from its provider, so it is partitioned by that prefix — otherwise
/// every new address of the same block would start a fresh bucket.
/// </summary>
public static class ClientIpPartition
{
    public const string Unknown = "unknown";

    public static string Of(IPAddress? address)
    {
        if (address is null) return Unknown;
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        if (address.AddressFamily != AddressFamily.InterNetworkV6) return address.ToString();

        Span<byte> bytes = stackalloc byte[16];
        address.TryWriteBytes(bytes, out _);
        bytes[8..].Clear();
        return new IPAddress(bytes) + "/64";
    }
}
