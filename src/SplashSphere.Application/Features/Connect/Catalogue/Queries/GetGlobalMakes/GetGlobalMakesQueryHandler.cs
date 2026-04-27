using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Catalogue.Queries.GetGlobalMakes;

public sealed class GetGlobalMakesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetGlobalMakesQuery, IReadOnlyList<GlobalMakeDto>>
{
    public async Task<IReadOnlyList<GlobalMakeDto>> Handle(
        GetGlobalMakesQuery request,
        CancellationToken cancellationToken)
    {
        return await db.GlobalMakes
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Name)
            .Select(m => new GlobalMakeDto(m.Id, m.Name, m.DisplayOrder))
            .ToListAsync(cancellationToken);
    }
}
