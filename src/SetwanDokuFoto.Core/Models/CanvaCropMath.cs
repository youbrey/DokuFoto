namespace SetwanDokuFoto.Core.Models;

/// <summary>
/// Framework-independent crop calculations shared by the WPF preview and the
/// high-resolution export renderer.
/// </summary>
public static class CanvaCropMath
{
    public static double CalculateCoverScale(
        double frameWidth,
        double frameHeight,
        double imageWidth,
        double imageHeight)
    {
        ValidateDimensions(frameWidth, frameHeight, imageWidth, imageHeight);
        return Math.Max(frameWidth / imageWidth, frameHeight / imageHeight);
    }

    /// <summary>
    /// Returns the additional zoom required for a centered rotated image to
    /// contain every corner of the frame.
    /// </summary>
    public static double CalculateRotationCoverMultiplier(
        double frameWidth,
        double frameHeight,
        double coveredImageWidth,
        double coveredImageHeight,
        double rotationDegrees)
    {
        ValidateDimensions(frameWidth, frameHeight, coveredImageWidth, coveredImageHeight);

        var radians = rotationDegrees * Math.PI / 180d;
        var cos = Math.Abs(Math.Cos(radians));
        var sin = Math.Abs(Math.Sin(radians));

        var requiredImageWidth = frameWidth * cos + frameHeight * sin;
        var requiredImageHeight = frameWidth * sin + frameHeight * cos;

        return Math.Max(
            1,
            Math.Max(requiredImageWidth / coveredImageWidth, requiredImageHeight / coveredImageHeight));
    }

    public static (double Width, double Height) ParseAspectRatio(string? aspectRatio)
    {
        if (string.IsNullOrWhiteSpace(aspectRatio)) return (4, 3);

        var parts = aspectRatio.Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length == 2 &&
            double.TryParse(parts[0], out var width) &&
            double.TryParse(parts[1], out var height) &&
            width > 0 && height > 0)
        {
            return (width, height);
        }

        return (4, 3);
    }

    private static void ValidateDimensions(params double[] dimensions)
    {
        if (dimensions.Any(value => value <= 0 || double.IsNaN(value) || double.IsInfinity(value)))
        {
            throw new ArgumentOutOfRangeException(nameof(dimensions), "All frame and image dimensions must be finite and greater than zero.");
        }
    }
}
