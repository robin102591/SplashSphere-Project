using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Catalogue.Queries.GetGlobalModels;

public sealed class GetGlobalModelsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetGlobalModelsQuery, IReadOnlyList<GlobalModelDto>>
{
    public async Task<IReadOnlyList<GlobalModelDto>> Handle(
        GetGlobalModelsQuery request,
        CancellationToken cancellationToken)
    {
        return await db.GlobalModels
            .AsNoTracking()
            .Where(m => m.GlobalMakeId == request.MakeId && m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Name)
            .Select(m => new GlobalModelDto(m.Id, m.GlobalMakeId, m.Name, m.DisplayOrder))
            .ToListAsync(cancellationToken);
    }
}
