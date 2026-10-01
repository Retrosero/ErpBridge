using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace ErpBridge.Portal.Api;

/// <summary>An image variant ready to send: its bytes and media type.</summary>
public sealed record ShrunkImage(byte[] Content, string ContentType);

/// <summary>
/// Makes the catalog's image variants from a picked file (GOAL_MUSTERI_KATALOGU §5.1: the client sizes them, the server
/// only checks bytes and type). A seam: the browser does the work in production, which a test cannot run.
/// </summary>
public interface IImageShrinker
{
    /// <summary>The picture with its long edge at most <paramref name="maxSide"/> pixels and at most <paramref name="maxBytes"/>;
    /// null when the browser cannot read it or no size fits.</summary>
    Task<ShrunkImage?> ShrinkAsync(IBrowserFile file, int maxSide, long maxBytes, CancellationToken ct = default) =>
        ShrinkAsync(file, maxSide, maxSide, maxBytes, ct);

    /// <summary>
    /// The picture fitted inside <paramref name="maxWidth"/> × <paramref name="maxHeight"/> pixels, its proportions kept,
    /// at most <paramref name="maxBytes"/>: a wide box for a banner (1920 × 720), a square one for a product picture.
    /// </summary>
    Task<ShrunkImage?> ShrinkAsync(IBrowserFile file, int maxWidth, int maxHeight, long maxBytes, CancellationToken ct = default);
}

/// <summary>
/// Resizes in the browser (<see cref="BrowserFileExtensions.RequestImageFileAsync"/>, a canvas): the picture is drawn anew,
/// so no EXIF or location data travels. A PNG stays PNG to keep its transparent background (JPEG would paint it black)
/// unless only JPEG fits; a smaller size is tried when the first is still too heavy.
/// </summary>
public sealed class BrowserImageShrinker : IImageShrinker
{
    public async Task<ShrunkImage?> ShrinkAsync(IBrowserFile file, int maxWidth, int maxHeight, long maxBytes, CancellationToken ct = default)
    {
        string[] formats = file.ContentType == "image/png" ? ["image/png", "image/jpeg"] : ["image/jpeg"];
        foreach (var format in formats)
        {
            foreach (var (width, height) in new[] { (maxWidth, maxHeight), (maxWidth * 3 / 4, maxHeight * 3 / 4), (maxWidth / 2, maxHeight / 2) })
            {
                IBrowserFile resized;
                try
                {
                    resized = await file.RequestImageFileAsync(format, width, height);
                }
                catch (JSException)
                {
                    // The browser could not decode the picture (HEIC on a desktop, a damaged file).
                    return null;
                }
                if (resized.Size > maxBytes) continue;
                await using var stream = resized.OpenReadStream(maxBytes, ct);
                using var buffer = new MemoryStream();
                await stream.CopyToAsync(buffer, ct);
                return new ShrunkImage(buffer.ToArray(), format);
            }
        }
        return null;
    }
}
