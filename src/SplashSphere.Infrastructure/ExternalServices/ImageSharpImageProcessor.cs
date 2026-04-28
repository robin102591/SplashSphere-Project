using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// ImageSharp-backed implementation of <see cref="IImageProcessor"/>.
/// Used by upload handlers to produce normalised PNG variants for the
/// file-storage backend.
/// </summary>
public sealed class ImageSharpImageProcessor : IImageProcessor
{
    public async Task<byte[]> ResizeToPngAsync(
        Stream source,
        int maxSidePx,
        CancellationToken cancellationToken = default)
    {
        Image image;
        try
        {
            image = await Image.LoadAsync(source, cancellationToken);
        }
        catch (UnknownImageFormatException ex)
        {
            throw new InvalidImageFormatException("Uploaded file is not a recognised image.");
        }

        try
        {
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(maxSidePx, maxSidePx),
                Mode = ResizeMode.Max, // preserve aspect ratio; never upscale beyond maxSidePx
            }));

            using var ms = new MemoryStream();
            await image.SaveAsync(ms, new PngEncoder(), cancellationToken);
            return ms.ToArray();
        }
        finally
        {
            image.Dispose();
        }
    }
}
