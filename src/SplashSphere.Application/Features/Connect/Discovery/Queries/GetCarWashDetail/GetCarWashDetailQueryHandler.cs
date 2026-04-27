using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.Discovery.Queries.GetCarWashDetail;

public sealed class GetCarWashDetailQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetCarWashDetailQuery, CarWashDetailDto?>
{
    private static readonly PlanTier[] EligiblePlans =
        [PlanTier.Trial, PlanTier.Growth, PlanTier.Enterprise];

    public async Task<CarWashDetailDto?> Handle(
        GetCarWashDetailQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(t => t.Id == request.TenantId && t.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
        if (tenant is null) return null;

        var sub = await db.TenantSubscriptions
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenant.Id, cancellationToken);
        if (sub is null || !EligiblePlans.Contains(sub.PlanTier)) return null;

        var branchRows = await (
            from branch in db.Branches.IgnoreQueryFilters()
            join setting in db.BookingSettings.IgnoreQueryFilters()
                on new { TenantId = branch.TenantId, BranchId = branch.Id }
                    equals new { setting.TenantId, setting.BranchId }
                into settingJoin
            from setting in settingJoin.DefaultIfEmpty()
            where branch.TenantId == tenant.Id
                && branch.IsActive
                && setting != null
                && setting.ShowInPublicDirectory
            select new
            {
                branch.Id,
                branch.Name,
                branch.Address,
                branch.ContactNumber,
                branch.Latitude,
                branch.Longitude,
                OpenTime = setting.OpenTime,
                CloseTime = setting.CloseTime,
                setting.IsBookingEnabled,
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (branchRows.Count == 0) return null;

        var services = await db.Services
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == tenant.Id && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new CarWashServiceDto(s.Id, s.Name, s.Description, s.BasePrice))
            .ToListAsync(cancellationToken);

        var isJoined = false;
        if (connectUser.IsAuthenticated)
        {
            isJoined = await db.ConnectUserTenantLinks
                .IgnoreQueryFilters()
                .AnyAsync(
                    l => l.ConnectUserId == connectUser.ConnectUserId
                        && l.TenantId == tenant.Id
                        && l.IsActive,
                    cancellationToken);
        }

        var branches = branchRows
            .Select(b => new CarWashBranchDto(
                b.Id,
                b.Name,
                b.Address,
                b.ContactNumber,
                b.Latitude,
                b.Longitude,
                b.OpenTime.ToString("HH:mm"),
                b.CloseTime.ToString("HH:mm"),
                b.IsBookingEnabled))
            .ToList();

        return new CarWashDetailDto(
            tenant.Id,
            tenant.Name,
            tenant.Email,
            tenant.ContactNumber,
            tenant.Address,
            isJoined,
            branches,
            services);
    }
}
