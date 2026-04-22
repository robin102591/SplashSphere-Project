using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Discovery.Queries.GetMyCarWashes;

public sealed class GetMyCarWashesQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetMyCarWashesQuery, IReadOnlyList<MyCarWashDto>>
{
    public async Task<IReadOnlyList<MyCarWashDto>> Handle(
        GetMyCarWashesQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return [];

        var userId = connectUser.ConnectUserId;

        return await (
            from link in db.ConnectUserTenantLinks.IgnoreQueryFilters()
            join tenant in db.Tenants.IgnoreQueryFilters()
                on link.TenantId equals tenant.Id
            where link.ConnectUserId == userId && link.IsActive && tenant.IsActive
            orderby link.LinkedAt descending
            select new MyCarWashDto(tenant.Id, tenant.Name, tenant.Address, link.LinkedAt))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
