using SetwanDokuFoto.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace SetwanDokuFoto.DocxEngine;

/// <summary>
/// Renders the original source image directly at export resolution. It never
/// uses a screenshot of the WPF canvas and never overwrites the source file.
/// </summary>
internal static class CropImageRenderer
{
    public static byte[] RenderJpeg(CollageCell cell, int outputWidth, int outputHeight)
    {
        if (cell.Photo is null) throw new ArgumentException("The collage cell has no photo.", nameof(cell));
        if (outputWidth <= 0 || outputHeight <= 0) throw new ArgumentOutOfRangeException(nameof(outputWidth));

        using var image = LoadOriginal(cell.Photo);
        image.Mutate(context => context.AutoOrient());

        var transform = cell.Photo.Transform;
        var coverScale = CanvaCropMath.CalculateCoverScale(
            outputWidth, outputHeight, image.Width, image.Height);
        var coveredWidth = image.Width * coverScale;
        var coveredHeight = image.Height * coverScale;
        var rotationCover = CanvaCropMath.CalculateRotationCoverMultiplier(
            outputWidth, outputHeight, coveredWidth, coveredHeight, transform.Rotation);
        var effectiveScale = Math.Max(transform.Scale, rotationCover);

        var resizedWidth = Math.Max(outputWidth, (int)Math.Ceiling(coveredWidth * effectiveScale));
        var resizedHeight = Math.Max(outputHeight, (int)Math.Ceiling(coveredHeight * effectiveScale));

        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Size = new Size(resizedWidth, resizedHeight),
            Mode = ResizeMode.Stretch,
            Sampler = KnownResamplers.Lanczos3
        }));

        if (Math.Abs(transform.Rotation) > .01)
            image.Mutate(context => context.Rotate((float)transform.Rotation, KnownResamplers.Bicubic));

        var desiredLeft = (image.Width - outputWidth) / 2d - transform.OffsetX * outputWidth;
        var desiredTop = (image.Height - outputHeight) / 2d - transform.OffsetY * outputHeight;
        var left = (int)Math.Round(Math.Clamp(desiredLeft, 0, Math.Max(0, image.Width - outputWidth)));
        var top = (int)Math.Round(Math.Clamp(desiredTop, 0, Math.Max(0, image.Height - outputHeight)));

        using var cropped = image.Clone(context => context.Crop(new Rectangle(left, top, outputWidth, outputHeight)));
        if (transform.FlipHorizontal) cropped.Mutate(context => context.Flip(FlipMode.Horizontal));
        if (transform.FlipVertical) cropped.Mutate(context => context.Flip(FlipMode.Vertical));

        using var output = new MemoryStream();
        cropped.Save(output, new JpegEncoder { Quality = 92 });
        return output.ToArray();
    }

    private static Image LoadOriginal(PhotoItem photo)
    {
        if (photo.ImageBytes is { Length: > 0 } bytes) return Image.Load(bytes);
        if (!string.IsNullOrWhiteSpace(photo.FilePath) && File.Exists(photo.FilePath))
            return Image.Load(photo.FilePath);

        throw new FileNotFoundException("File foto sumber tidak ditemukan.", photo.FilePath);
    }
}
