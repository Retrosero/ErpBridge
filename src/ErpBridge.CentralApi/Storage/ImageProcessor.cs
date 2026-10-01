using SkiaSharp;

namespace ErpBridge.CentralApi.Storage;

/// <summary>A bounding box a picture is shrunk into (never enlarged); the aspect ratio is kept.</summary>
public readonly record struct ImageBox(int MaxWidth, int MaxHeight)
{
    /// <summary>Detail picture: long edge 1280 px (T5).</summary>
    public static readonly ImageBox Large = new(1280, 1280);

    /// <summary>List thumbnail: long edge 400 px.</summary>
    public static readonly ImageBox Small = new(400, 400);

    /// <summary>Catalog banner, large: within 1920 × 720.</summary>
    public static readonly ImageBox BannerLarge = new(1920, 720);

    /// <summary>Catalog banner, small: within 800 × 300.</summary>
    public static readonly ImageBox BannerSmall = new(800, 300);
}

/// <summary>A WebP picture made by <see cref="ImageProcessor"/>.</summary>
public sealed record ProcessedImage(byte[] Data, int Width, int Height)
{
    public string ContentType => ImageBytes.Webp;
}

/// <summary>The input is not a picture the server can read (broken, unsupported, too many pixels).</summary>
public sealed class ImageProcessingException : Exception
{
    public ImageProcessingException(string message) : base(message) { }
}

/// <summary>
/// Server-side picture work (GOAL_DEPOLAMA_R2 T5, SkiaSharp — MIT): decodes JPEG, PNG and WebP, turns the picture
/// upright by its EXIF orientation, shrinks it into each requested <see cref="ImageBox"/> and encodes WebP. The output
/// carries no metadata at all (Skia writes none), so EXIF/GPS never survives. Pure and thread-safe: no state.
/// </summary>
public static class ImageProcessor
{
    public const int DefaultQuality = 80;

    /// <summary>Decoding refuses more pixels than this (≈ 8000 × 6000): a small file must not unpack into gigabytes.</summary>
    public const long MaxPixels = 50_000_000;

    /// <summary>One size.</summary>
    public static ProcessedImage ToWebp(byte[] input, ImageBox box, int quality = DefaultQuality) => ToWebp(input, [box], quality)[0];

    /// <summary>
    /// Every size from one decode, in the order of <paramref name="boxes"/>. Throws <see cref="ImageProcessingException"/>
    /// for anything that is not a readable JPEG, PNG or WebP.
    /// </summary>
    public static IReadOnlyList<ProcessedImage> ToWebp(byte[] input, IReadOnlyList<ImageBox> boxes, int quality = DefaultQuality)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (boxes.Count == 0) throw new ArgumentException("At least one size is needed.", nameof(boxes));
        if (boxes.Any(b => b.MaxWidth < 1 || b.MaxHeight < 1)) throw new ArgumentOutOfRangeException(nameof(boxes), "A box needs a positive width and height.");
        quality = Math.Clamp(quality, 1, 100);

        using var data = SKData.CreateCopy(input);
        using var codec = SKCodec.Create(data) ?? throw new ImageProcessingException("The file is not a readable picture.");
        if (codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))
            throw new ImageProcessingException("Only JPEG, PNG and WebP pictures are accepted.");
        var size = codec.Info;
        if (size.Width < 1 || size.Height < 1 || (long)size.Width * size.Height > MaxPixels)
            throw new ImageProcessingException("The picture's size is not acceptable.");

        var origin = codec.EncodedOrigin;
        var turned = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var (uprightWidth, uprightHeight) = turned ? (size.Height, size.Width) : (size.Width, size.Height);
        var targets = boxes.Select(b => Fit(uprightWidth, uprightHeight, b)).ToList();

        using var source = Decode(codec, size, turned, targets);
        using var image = SKImage.FromBitmap(source);
        var results = new List<ProcessedImage>(targets.Count);
        foreach (var (width, height) in targets)
            results.Add(Render(image, origin, width, height, quality));
        return results;
    }

    /// <summary>The size inside <paramref name="box"/> with the same proportions, never larger than the picture.</summary>
    public static (int Width, int Height) Fit(int width, int height, ImageBox box)
    {
        var scale = Math.Min(1d, Math.Min((double)box.MaxWidth / width, (double)box.MaxHeight / height));
        return (Math.Max(1, (int)Math.Round(width * scale)), Math.Max(1, (int)Math.Round(height * scale)));
    }

    /// <summary>
    /// Decodes once. A JPEG can decode straight at a fraction of its size; that is used when the fraction is still at
    /// least as large as the biggest output, so a 4000 px photo is not unpacked in full to make a 1280 px picture.
    /// </summary>
    private static SKBitmap Decode(SKCodec codec, SKImageInfo size, bool turned, List<(int Width, int Height)> targets)
    {
        var needWidth = targets.Max(t => turned ? t.Height : t.Width);
        var needHeight = targets.Max(t => turned ? t.Width : t.Height);
        var decodeSize = new SKSizeI(size.Width, size.Height);
        var scaled = codec.GetScaledDimensions(Math.Max((float)needWidth / size.Width, (float)needHeight / size.Height));
        if (scaled.Width >= needWidth && scaled.Height >= needHeight && scaled.Width <= size.Width && scaled.Height <= size.Height)
            decodeSize = scaled;

        var info = new SKImageInfo(decodeSize.Width, decodeSize.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        var bitmap = new SKBitmap(info);
        var result = codec.GetPixels(info, bitmap.GetPixels());
        if (result != SKCodecResult.Success)
        {
            bitmap.Dispose();
            throw new ImageProcessingException("The picture could not be decoded.");
        }
        return bitmap;
    }

    /// <summary>Draws the decoded picture upright and scaled onto a canvas of the output size and encodes WebP.</summary>
    private static ProcessedImage Render(SKImage source, SKEncodedOrigin origin, int width, int height, int quality)
    {
        var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info) ?? throw new ImageProcessingException("The picture could not be drawn.");
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        var turned = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        float uprightWidth = turned ? source.Height : source.Width;
        float uprightHeight = turned ? source.Width : source.Height;
        canvas.Scale(width / uprightWidth, height / uprightHeight);
        canvas.Concat(Orientation(origin, source.Width, source.Height));
        using var paint = new SKPaint();
        canvas.DrawImage(source, 0, 0, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint);
        canvas.Flush();

        using var snapshot = surface.Snapshot();
        using var pixels = snapshot.PeekPixels() ?? throw new ImageProcessingException("The picture could not be read back.");
        using var encoded = pixels.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality))
            ?? throw new ImageProcessingException("The picture could not be encoded.");
        return new ProcessedImage(encoded.ToArray(), width, height);
    }

    /// <summary>
    /// The matrix that turns the stored pixels upright, per EXIF orientation (source size <paramref name="w"/> ×
    /// <paramref name="h"/>): x' = ScaleX·x + SkewX·y + TransX, y' = SkewY·x + ScaleY·y + TransY.
    /// </summary>
    private static SKMatrix Orientation(SKEncodedOrigin origin, float w, float h) => origin switch
    {
        SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
        SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
        SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
        SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
        SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
        SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
        SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
        _ => SKMatrix.Identity,
    };
}
