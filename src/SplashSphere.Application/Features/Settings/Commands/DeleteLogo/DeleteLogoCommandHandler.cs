using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Settings.Commands.DeleteLogo;

public sealed class DeleteLogoCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IFileStorageService storage)
    : IRequestHandler<DeleteLogoCommand, Result>
{
    private static readonly string[] VariantSuffixes = ["logo", "thumbnail", "icon"];

    public async Task<Result> Handle(
        DeleteLogoCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant", tenantContext.TenantId));

        // Best-effort delete from R2. We don't fail the request if R2 is
        // unreachable — clearing the local URLs is the user-visible action;
        // orphan blobs in R2 are a cleanup concern, not a correctness one.
        foreach (var suffix in VariantSuffixes)
        {
            var key = $"tenants/{tenantContext.TenantId}/{suffix}.png";
            try { await storage.DeleteAsync(key, cancellationToken); }
            catch { /* swallowed by design — see comment above */ }
        }

        tenant.LogoUrl          = null;
        tenant.LogoThumbnailUrl = null;
        tenant.LogoIconUrl      = null;

        return Result.Success();
    }
}
