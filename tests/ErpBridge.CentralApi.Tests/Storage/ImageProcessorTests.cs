using System.Text;
using ErpBridge.CentralApi.Storage;
using FluentAssertions;
using SkiaSharp;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S2: <see cref="ImageProcessor"/> shrinks into the requested boxes, keeps the proportions, turns the
/// picture upright by its EXIF orientation, writes WebP without any metadata and refuses broken input cleanly.
/// </summary>
public sealed class ImageProcessorTests
{
    [Fact]
    public void A_4000_px_jpeg_becomes_1280_and_400_px_webp_with_its_proportions()
    {
        var photo = Encode(Picture(4000, 3000), SKEncodedImageFormat.Jpeg);

        var sizes = ImageProcessor.ToWebp(photo, [ImageBox.Large, ImageBox.Small]);

        sizes.Select(s => (s.Width, s.Height)).Should().Equal((1280, 960), (400, 300));
        foreach (var size in sizes)
        {
            size.ContentType.Should().Be("image/webp");
            ImageBytes.LooksLike("image/webp", size.Data).Should().BeTrue();
            using var decoded = SKBitmap.Decode(size.Data);
            (decoded.Width, decoded.Height).Should().Be((size.Width, size.Height));
        }
        sizes[0].Data.Length.Should().BeLessThan(photo.Length);
    }

    [Fact]
    public void A_portrait_picture_is_bounded_by_its_long_edge_and_a_small_one_is_never_enlarged()
    {
        var portrait = ImageProcessor.ToWebp(Encode(Picture(1500, 3000), SKEncodedImageFormat.Png), ImageBox.Large);
        (portrait.Width, portrait.Height).Should().Be((640, 1280));

        var small = ImageProcessor.ToWebp(Encode(Picture(300, 200), SKEncodedImageFormat.Webp), ImageBox.Large);
        (small.Width, small.Height).Should().Be((300, 200));
    }

    [Fact]
    public void A_banner_is_fitted_inside_its_width_and_height()
    {
        var wide = Encode(Picture(3840, 1080), SKEncodedImageFormat.Jpeg);

        var banners = ImageProcessor.ToWebp(wide, [ImageBox.BannerLarge, ImageBox.BannerSmall]);

        banners.Select(b => (b.Width, b.Height)).Should().Equal((1920, 540), (800, 225));
        ImageProcessor.Fit(1000, 1000, ImageBox.BannerLarge).Should().Be((720, 720), "a square banner picture is limited by the height");
    }

    [Fact]
    public void The_exif_orientation_turns_the_picture_upright_and_no_metadata_survives()
    {
        // Left half red, right half blue, stored sideways with EXIF orientation 6 (turn 90° clockwise to view).
        var stored = Encode(Halves(300, 100), SKEncodedImageFormat.Jpeg);
        var withExif = WithExif(stored, orientation: 6, comment: "GPS-SECRET-41.0082N");
        using (var codec = SKCodec.Create(SKData.CreateCopy(withExif)))
            codec.EncodedOrigin.Should().Be(SKEncodedOrigin.RightTop, "the test picture carries the orientation");

        var upright = ImageProcessor.ToWebp(withExif, ImageBox.Large);

        (upright.Width, upright.Height).Should().Be((100, 300));
        using var decoded = SKBitmap.Decode(upright.Data);
        IsRed(decoded.GetPixel(50, 30)).Should().BeTrue("the left half is on top after a clockwise turn");
        IsBlue(decoded.GetPixel(50, 270)).Should().BeTrue();
        var text = Encoding.ASCII.GetString(upright.Data);
        text.Should().NotContain("GPS-SECRET").And.NotContain("Exif").And.NotContain("EXIF").And.NotContain("XMP");
    }

    [Theory]
    [InlineData("not a picture at all")]
    [InlineData("")]
    public void Garbage_is_refused_with_a_clear_error(string content)
    {
        var act = () => ImageProcessor.ToWebp(Encoding.UTF8.GetBytes(content), ImageBox.Small);

        act.Should().Throw<ImageProcessingException>();
    }

    [Fact]
    public void A_truncated_picture_is_refused_with_a_clear_error()
    {
        var photo = Encode(Picture(800, 600), SKEncodedImageFormat.Png);

        var act = () => ImageProcessor.ToWebp(photo[..(photo.Length / 3)], ImageBox.Small);

        act.Should().Throw<ImageProcessingException>();
    }

    [Fact]
    public void A_gif_is_not_accepted()
    {
        // GIF89a, 1×1, one black pixel.
        byte[] gif = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 1, 0, 1, 0, 0x80, 0, 0, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0x21, 0xF9, 4, 1, 0, 0, 0, 0,
            0x2C, 0, 0, 0, 0, 1, 0, 1, 0, 0, 2, 2, 0x44, 1, 0, 0x3B];

        var act = () => ImageProcessor.ToWebp(gif, ImageBox.Small);

        act.Should().Throw<ImageProcessingException>();
    }

    // ---- helpers ------------------------------------------------------------------------------

    private static SKBitmap Picture(int width, int height)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(width, height), [SKColors.Orange, SKColors.Teal], SKShaderTileMode.Clamp) };
        canvas.DrawRect(0, 0, width, height, paint);
        return bitmap;
    }

    private static SKBitmap Halves(int width, int height)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Blue);
        using var red = new SKPaint { Color = SKColors.Red };
        canvas.DrawRect(0, 0, width / 2f, height, red);
        return bitmap;
    }

    private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format)
    {
        using (bitmap)
        using (var data = bitmap.Encode(format, 90))
            return data.ToArray();
    }

    /// <summary>The JPEG with an APP1 EXIF block (big-endian TIFF, IFD0: orientation, image description) after SOI.</summary>
    private static byte[] WithExif(byte[] jpeg, ushort orientation, string comment)
    {
        var text = Encoding.ASCII.GetBytes(comment + "\0");
        var tiff = new List<byte> { (byte)'M', (byte)'M', 0, 42, 0, 0, 0, 8, 0, 2 };
        // 0x0112 Orientation, SHORT, 1, value.
        tiff.AddRange([0x01, 0x12, 0, 3, 0, 0, 0, 1, (byte)(orientation >> 8), (byte)orientation, 0, 0]);
        // 0x010E ImageDescription, ASCII, n, offset after the IFD (8 + 2 + 2*12 + 4 = 38).
        tiff.AddRange([0x01, 0x0E, 0, 2, 0, 0, 0, (byte)text.Length, 0, 0, 0, 38]);
        tiff.AddRange([0, 0, 0, 0]);
        tiff.AddRange(text);
        var app1 = new List<byte>();
        app1.AddRange(Encoding.ASCII.GetBytes("Exif\0\0"));
        app1.AddRange(tiff);
        var length = app1.Count + 2;
        return [.. jpeg[..2], 0xFF, 0xE1, (byte)(length >> 8), (byte)length, .. app1, .. jpeg[2..]];
    }

    private static bool IsRed(SKColor c) => c.Red > 200 && c.Green < 60 && c.Blue < 60;

    private static bool IsBlue(SKColor c) => c.Blue > 200 && c.Red < 60 && c.Green < 60;
}
