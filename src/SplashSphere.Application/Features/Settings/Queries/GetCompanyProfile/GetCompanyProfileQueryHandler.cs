using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Settings.Queries.GetCompanyProfile;

public sealed class GetCompanyProfileQueryHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<GetCompanyProfileQuery, CompanyProfileDto?>
{
    public async Task<CompanyProfileDto?> Handle(
        GetCompanyProfileQuery request,
        CancellationToken cancellationToken)
    {
        // Tenant rows are not subject to the global query filter, so look up
        // by the current tenant ID explicitly.
        return await context.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenantContext.TenantId)
            .Select(t => new CompanyProfileDto(
                t.Name,
                t.Tagline,
                t.Email,
                t.ContactNumber,
                t.Website,
                t.Address,
                t.StreetAddress,
                t.Barangay,
                t.City,
                t.Province,
                t.ZipCode,
                t.TaxId,
                t.BusinessPermitNo,
                t.IsVatRegistered,
                t.FacebookUrl,
                t.InstagramHandle,
                t.GCashNumber))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
