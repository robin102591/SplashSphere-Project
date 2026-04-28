using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Settings.Commands.UploadLogo;

public sealed class UploadLogoCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IFileStorageService storage,
    IImageProcessor imageProcessor)
    : IRequestHandler<UploadLogoCommand, Result<UploadLogoResult>>
{
    private static readonly (string Suffix, int Px)[] Variants =
    [
        ("logo",      500),  // Main — Connect detail page, admin sidebar.
        ("thumbnail", 200),  // Receipts.
        ("icon",       80),  // Connect directory rows, dense UI surfaces.
    ];

    public async Task<Result<UploadLogoResult>> Handle(
        UploadLogoCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<UploadLogoResult>(Error.NotFound("Tenant", tenantContext.TenantId));

        // Buffer the upload once. The processor decodes per call; for a 2MB
        // ceiling and only 3 variants the duplicated decode work is negligible
        // and keeps the abstraction simple (no shared decoded state needed).
        byte[] sourceBytes;
        using (var ms = new MemoryStream())
        {
            await request.Content.CopyToAsync(ms, cancellationToken);
            sourceBytes = ms.ToArray();
        }

        var urls = new Dictionary<string, string>();
        try
        {
            foreach (var (suffix, px) in Variants)
            {
                using var perVariantSource = new MemoryStream(sourceBytes, writable: false);
                var pngBytes = await imageProcessor.ResizeToPngAsync(perVariantSource, px, cancellationToken);
                using var pngStream = new MemoryStream(pngBytes, writable: false);

                var key = $"tenants/{tenantContext.TenantId}/{suffix}.png";
                var url = await storage.UploadAsync(key, pngStream, "image/png", cancellationToken);
                urls[suffix] = url;
            }
        }
        catch (InvalidImageFormatException ex)
        {
            return Result.Failure<UploadLogoResult>(Error.Validation(ex.Message));
        }

        // Cache-busting version suffix forces clients (browsers, CDN cache) to
        // refetch after re-uploads — R2 keys are stable, so without this the
        // old image would keep showing.
        var version = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        tenant.LogoUrl          = $"{urls["logo"]}?v={version}";
        tenant.LogoThumbnailUrl = $"{urls["thumbnail"]}?v={version}";
        tenant.LogoIconUrl      = $"{urls["icon"]}?v={version}";

        return Result.Success(new UploadLogoResult(
            tenant.LogoUrl,
            tenant.LogoThumbnailUrl,
            tenant.LogoIconUrl));
    }
}
