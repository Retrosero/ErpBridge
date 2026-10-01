using System.Buffers.Binary;
using System.Text;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// What every picture upload checks before it is kept (GOAL_DEPOLAMA_R2 T3), in one place for the catalog, the task
/// pictures and the central file store: the type from the first bytes (a renamed file or a script is refused) and the
/// metadata dropped without an image library (JPEG EXIF/XMP, PNG text/EXIF, WebP EXIF/XMP: location, camera, author).
/// Moved here unchanged from <c>CatalogImages</c> and <c>TaskService</c>.
/// </summary>
public static class ImageBytes
{
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string Webp = "image/webp";

    public static readonly IReadOnlySet<string> ContentTypes = new HashSet<string>(StringComparer.Ordinal) { Jpeg, Png, Webp };

    /// <summary>A header that matches the declared type: a renamed file or a script is refused.</summary>
    public static bool LooksLike(string type, byte[] data) => type switch
    {
        Jpeg => data.Length > 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
        Png => data.Length > 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47,
        Webp => data.Length > 12 && data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F'
            && data[8] == (byte)'W' && data[9] == (byte)'E' && data[10] == (byte)'B' && data[11] == (byte)'P',
        _ => false,
    };

    /// <summary>The picture's type from its first bytes (JPEG, PNG, WebP), or null for anything else.</summary>
    public static string? Sniff(byte[] data)
    {
        foreach (var type in ContentTypes)
            if (LooksLike(type, data)) return type;
        return null;
    }

    /// <summary>The <c>Content-Type</c> header's media type, lower case, without parameters.</summary>
    public static string MediaType(string? contentType) => (contentType ?? string.Empty).Split(';')[0].Trim().ToLowerInvariant();

    /// <summary>The file name extension of a supported type.</summary>
    public static string Extension(string type) => type switch
    {
        Jpeg => "jpg",
        Png => "png",
        Webp => "webp",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Not a supported picture type."),
    };

    /// <summary>
    /// A JPEG without its APP1 segments (EXIF and XMP: location, camera, time). Segments are walked up to the image
    /// data; anything the walk cannot read is kept as it is, so a picture is never broken by the clean-up.
    /// </summary>
    public static byte[] StripJpegMetadata(byte[] data)
    {
        if (data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8) return data;
        using var output = new MemoryStream(data.Length);
        output.Write(data, 0, 2);
        var i = 2;
        while (i + 4 <= data.Length && data[i] == 0xFF)
        {
            var marker = data[i + 1];
            if (marker == 0xFF)
            {
                i++; // fill byte before a marker
                continue;
            }
            // Start of scan or end of image: the rest is image data.
            if (marker is 0xDA or 0xD9) break;
            // Markers without a length.
            if (marker is 0x01 or >= 0xD0 and <= 0xD7)
            {
                output.Write(data, i, 2);
                i += 2;
                continue;
            }
            var length = (data[i + 2] << 8) | data[i + 3];
            if (length < 2 || i + 2 + length > data.Length) break;
            if (marker != 0xE1) output.Write(data, i, 2 + length);
            i += 2 + length;
        }
        output.Write(data, i, data.Length - i);
        return output.ToArray();
    }

    /// <summary>
    /// The picture without what it says about where and how it was taken: JPEG APP1 (EXIF, XMP), PNG text and EXIF
    /// chunks, WebP EXIF and XMP chunks. A JPEG's orientation goes with its EXIF; the phone and the panel turn the picture
    /// upright and encode it again before sending, so nothing is left to turn.
    /// </summary>
    public static byte[] StripMetadata(string contentType, byte[] data) => contentType switch
    {
        Jpeg => StripJpegMetadata(data),
        Png => StripPngMetadata(data),
        Webp => StripWebpMetadata(data),
        _ => data,
    };

    /// <summary>PNG chunks dropped: EXIF and the three text kinds (author, comments, software, XMP).</summary>
    private static readonly HashSet<string> PngMetadataChunks = new(StringComparer.Ordinal) { "eXIf", "tEXt", "iTXt", "zTXt" };

    /// <summary>
    /// A PNG without its <c>eXIf</c>, <c>tEXt</c>, <c>iTXt</c> and <c>zTXt</c> chunks (each chunk: length, type, data,
    /// CRC; the CRC covers only its own chunk, so the rest stay valid). A file the walk cannot read is returned as it is.
    /// </summary>
    public static byte[] StripPngMetadata(byte[] data)
    {
        const int signature = 8;
        if (data.Length < signature || data[0] != 0x89 || data[1] != 0x50 || data[2] != 0x4E || data[3] != 0x47) return data;
        using var output = new MemoryStream(data.Length);
        output.Write(data, 0, signature);
        var i = signature;
        var dropped = false;
        while (i < data.Length)
        {
            if (i + 12 > data.Length) return data;
            var length = ((long)data[i] << 24) | ((long)data[i + 1] << 16) | ((long)data[i + 2] << 8) | data[i + 3];
            var total = 12 + length;
            if (i + total > data.Length) return data;
            var type = Encoding.ASCII.GetString(data, i + 4, 4);
            if (PngMetadataChunks.Contains(type)) dropped = true;
            else output.Write(data, i, (int)total);
            i += (int)total;
            if (type == "IEND") break;
        }
        if (!dropped) return data;
        output.Write(data, i, data.Length - i);
        return output.ToArray();
    }

    /// <summary>
    /// A WebP without its <c>EXIF</c> and <c>XMP </c> chunks; the extended header's (<c>VP8X</c>) flags for them are
    /// cleared and the RIFF size is written again. A file the walk cannot read is returned as it is.
    /// </summary>
    public static byte[] StripWebpMetadata(byte[] data)
    {
        const int header = 12;
        if (data.Length < header || !LooksLike(Webp, data)) return data;
        using var output = new MemoryStream(data.Length);
        output.Write(data, 0, header);
        var i = header;
        var dropped = false;
        while (i < data.Length)
        {
            if (i + 8 > data.Length) return data;
            long size = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i + 4, 4));
            var total = 8 + size + (size & 1);
            if (i + 8 + size > data.Length) return data;
            total = Math.Min(total, data.Length - i);
            var fourCc = Encoding.ASCII.GetString(data, i, 4);
            if (fourCc is "EXIF" or "XMP ")
            {
                dropped = true;
            }
            else
            {
                var start = (int)output.Position;
                output.Write(data, i, (int)total);
                // VP8X: the first payload byte's flags say which chunks follow (EXIF 0x08, XMP 0x04).
                if (fourCc == "VP8X" && size >= 1)
                {
                    var buffer = output.GetBuffer();
                    buffer[start + 8] = (byte)(buffer[start + 8] & ~0x0C);
                }
            }
            i += (int)total;
        }
        if (!dropped) return data;
        var result = output.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4, 4), (uint)(result.Length - 8));
        return result;
    }
}
