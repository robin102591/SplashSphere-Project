using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Settings.Queries.GetDisplaySetting;

namespace SplashSphere.Application.Features.Display.Queries.GetDisplayConfig;

public sealed class GetDisplayConfigQueryHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IMediator mediator)
    : IRequestHandler<GetDisplayConfigQuery, DisplayConfigDto>
{
    public async Task<DisplayConfigDto> Handle(
        GetDisplayConfigQuery request,
        CancellationToken cancellationToken)
    {
        // Settings flow through the existing branch-fallback resolver.
        var settings = await mediator.Send(
            new GetDisplaySettingQuery(request.BranchId),
            cancellationToken);

        // Tenant branding: pulled directly from the Tenant row (not subject
        // to the tenant query filter) and trimmed to the customer-safe subset.
        var branding = await context.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenantContext.TenantId)
            .Select(t => new DisplayBrandingDto(
                t.Name,
                t.Tagline,
                t.LogoThumbnailUrl,    // 200px is the right size for display headers
                t.FacebookUrl,
                t.InstagramHandle,
                t.GCashNumber,
                null))                  // QR upload not yet wired — slice 4+ of Company Profile
            .FirstOrDefaultAsync(cancellationToken)
            ?? new DisplayBrandingDto("SplashSphere", null, null, null, null, null, null);

        return new DisplayConfigDto(settings, branding);
    }
}
