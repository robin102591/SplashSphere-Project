namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Abstracts image decoding + resizing so the Application layer doesn't
/// take a direct dependency on ImageSharp (or any other image library).
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Decodes the input stream and produces a single resized PNG copy.
    /// Aspect ratio is preserved; the image is fit inside an
    /// <paramref name="maxSidePx"/> × <paramref name="maxSidePx"/> box.
    /// Throws <see cref="InvalidImageFormatException"/> if the stream
    /// is not a recognised image.
    /// </summary>
    /// <param name="source">The original upload (any common image format).</param>
    /// <param name="maxSidePx">Bounding-box size in pixels.</param>
    /// <returns>A new PNG-encoded byte array. Caller owns the result.</returns>
    Task<byte[]> ResizeToPngAsync(Stream source, int maxSidePx, CancellationToken cancellationToken = default);
}

/// <summary>Thrown when <see cref="IImageProcessor"/> can't decode the input.</summary>
public sealed class InvalidImageFormatException(string message) : Exception(message);
