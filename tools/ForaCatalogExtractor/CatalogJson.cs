using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpBridge.Tools.ForaCatalog;

/// <summary>
/// Serialises the catalogue the same way on every machine. The output is committed and
/// compared byte for byte by the golden test, so encoding, escaping, indentation and
/// line endings are all pinned here rather than left to platform defaults.
/// </summary>
public static class CatalogJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        // Turkish text and the label templates stay readable instead of turning into \uXXXX.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>UTF-8 without a byte order mark; Git and the golden test both expect none.</summary>
    public static readonly Encoding FileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static string Serialize<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        // Normalise regardless of the runtime's indentation newline, then end with a single LF.
        return json.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n') + "\n";
    }

    public static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options)
        ?? throw new InvalidOperationException($"Could not read {typeof(T).Name} from JSON.");

    public static async Task WriteAsync<T>(string path, T value, CancellationToken ct = default)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, Serialize(value), FileEncoding, ct);
    }
}
