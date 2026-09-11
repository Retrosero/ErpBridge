using System.Text;
using System.Text.Json;

namespace ErpBridge.CentralApi.Sync;

/// <summary>
/// The token a device hands back to resume its feed.
///
/// <para>Opaque on purpose. A bare integer would harden the sequence into the
/// public contract, and every later change to how a position is expressed would
/// need a client release to go with it. Wrapping it leaves room to add fields —
/// and to reject a token from a format the server no longer understands, which
/// is a clean resync rather than a silent misread.</para>
/// </summary>
public static class SyncCursor
{
    /// <summary>
    /// Format version stamped into every token. Raising it invalidates every
    /// cursor in the field, which asks each device for one full resync — so raise
    /// it only when an old position genuinely cannot be honoured any more.
    /// </summary>
    public const int FormatVersion = 1;

    /// <summary>Position a device starts from when it holds nothing yet.</summary>
    public const long Start = 0;

    /// <summary>Encodes a sequence position as a token.</summary>
    public static string Encode(long sequence)
    {
        var json = JsonSerializer.Serialize(new Token(FormatVersion, sequence));
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>
    /// Reads a token handed back by a device. An absent or empty token is a fresh
    /// install and decodes to <see cref="Start"/>; anything unparseable, or from a
    /// version this build does not know, is refused so the caller can answer with
    /// a resync instead of guessing a position.
    /// </summary>
    public static bool TryDecode(string? token, out long sequence)
    {
        sequence = Start;
        if (string.IsNullOrWhiteSpace(token)) return true;

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parsed = JsonSerializer.Deserialize<Token>(json);
            if (parsed is null || parsed.V != FormatVersion || parsed.S < 0) return false;
            sequence = parsed.S;
            return true;
        }
        catch (Exception ex) when (ex is FormatException or JsonException or DecoderFallbackException)
        {
            return false;
        }
    }

    private sealed record Token(int V, long S);
}
