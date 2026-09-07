using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SyncAdapter.Core.Models;

namespace SyncAdapter.Infrastructure.Serialization;

/// <summary>
/// OfflineEvrakV2 envelope'unun HTTP gövdesine yazılmak üzere
/// JSON serileştirme + GZip sıkıştırma yapan yardımcı.
/// FORA Win binary BinaryWriter yerine JSON+GZip kullanıyor — debug edilebilirlik
/// kazanıyoruz, performans kaybı ihmal edilebilir düzeyde (tipik evrak &lt; 50 KB).
/// </summary>
public static class OfflineEvrakSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public static byte[] Sıkıştır(OfflineEvrakV2 envelope)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(envelope, Options);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(json, 0, json.Length);
        }
        return output.ToArray();
    }

    public static OfflineEvrakV2 Aç(byte[] sıkıştırılmış)
    {
        using var input = new MemoryStream(sıkıştırılmış);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<OfflineEvrakV2>(json, Options)
            ?? throw new InvalidDataException("Boş envelope deserialize edildi.");
    }
}
