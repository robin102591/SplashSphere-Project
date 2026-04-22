using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.Discovery.Queries.SearchCarWashes;

public sealed class SearchCarWashesQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<SearchCarWashesQuery, IReadOnlyList<CarWashListItemDto>>
{
    private static readonly PlanTier[] EligiblePlans =
        [PlanTier.Trial, PlanTier.Growth, PlanTier.Enterprise];

    public async Task<IReadOnlyList<CarWashListItemDto>> Handle(
        SearchCarWashesQuery request,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, 200);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        var userId = connectUser.IsAuthenticated ? connectUser.ConnectUserId : null;

        // Cross-tenant read — Connect customers have no tenant scope.
        var query =
            from branch in db.Branches.IgnoreQueryFilters()
            join tenant in db.Tenants.IgnoreQueryFilters()
                on branch.TenantId equals tenant.Id
            join sub in db.TenantSubscriptions.IgnoreQueryFilters()
                on tenant.Id equals sub.TenantId
            join setting in db.BookingSettings.IgnoreQueryFilters()
                on new { TenantId = tenant.Id, BranchId = branch.Id }
                    equals new { setting.TenantId, setting.BranchId }
                into settingJoin
            from setting in settingJoin.DefaultIfEmpty()
            where branch.IsActive
                && tenant.IsActive
                && EligiblePlans.Contains(sub.PlanTier)
                && setting != null
                && setting.ShowInPublicDirectory
            select new
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                BranchId = branch.Id,
                BranchName = branch.Name,
                branch.Address,
                branch.ContactNumber,
                branch.Latitude,
                branch.Longitude,
                OpenTime = setting.OpenTime,
                CloseTime = setting.CloseTime,
                IsBookingEnabled = setting.IsBookingEnabled,
            };

        if (search is not null)
        {
            var needle = search.ToLower();
            query = query.Where(x =>
                x.TenantName.ToLower().Contains(needle) ||
                x.BranchName.ToLower().Contains(needle) ||
                x.Address.ToLower().Contains(needle));
        }

        var rows = await query.Take(take * 2).ToListAsync(cancellationToken);

        HashSet<string> joinedTenantIds = userId is null
            ? []
            : (await db.ConnectUserTenantLinks
                .IgnoreQueryFilters()
                .Where(l => l.ConnectUserId == userId && l.IsActive)
                .Select(l => l.TenantId)
                .ToListAsync(cancellationToken)).ToHashSet();

        var items = rows.Select(r => new CarWashListItemDto(
            r.TenantId,
            r.TenantName,
            r.BranchId,
            r.BranchName,
            r.Address,
            r.ContactNumber,
            r.Latitude,
            r.Longitude,
            DistanceKm: Haversine(request.Latitude, request.Longitude, r.Latitude, r.Longitude),
            OpenTime: r.OpenTime.ToString("HH:mm"),
            CloseTime: r.CloseTime.ToString("HH:mm"),
            r.IsBookingEnabled,
            IsJoined: joinedTenantIds.Contains(r.TenantId)));

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            // Nearest first; entries without coords fall to the bottom.
            items = items.OrderBy(i => i.DistanceKm ?? double.MaxValue)
                         .ThenBy(i => i.TenantName);
        }
        else
        {
            items = items.OrderBy(i => i.TenantName).ThenBy(i => i.BranchName);
        }

        return items.Take(take).ToList();
    }

    /// <summary>Great-circle distance in km between two WGS84 points, or null if any coord is missing.</summary>
    private static double? Haversine(decimal? lat1, decimal? lon1, decimal? lat2, decimal? lon2)
    {
        if (lat1 is null || lon1 is null || lat2 is null || lon2 is null) return null;

        const double earthRadiusKm = 6371.0;
        var dLat = DegreesToRadians((double)(lat2.Value - lat1.Value));
        var dLon = DegreesToRadians((double)(lon2.Value - lon1.Value));
        var lat1Rad = DegreesToRadians((double)lat1.Value);
        var lat2Rad = DegreesToRadians((double)lat2.Value);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1Rad) * Math.Cos(lat2Rad);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double d) => d * Math.PI / 180.0;
}
