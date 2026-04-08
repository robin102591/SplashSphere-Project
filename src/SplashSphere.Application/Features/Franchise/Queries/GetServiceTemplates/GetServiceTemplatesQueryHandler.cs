using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Queries.GetServiceTemplates;

public sealed class GetServiceTemplatesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetServiceTemplatesQuery, IReadOnlyList<FranchiseServiceTemplateDto>>
{
    public async Task<IReadOnlyList<FranchiseServiceTemplateDto>> Handle(
        GetServiceTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var templates = await db.FranchiseServiceTemplates
            .AsNoTracking()
            .OrderBy(t => t.ServiceName)
            .Select(t => new FranchiseServiceTemplateDto(
                t.Id,
                t.ServiceName,
                t.Description,
                t.CategoryName,
                t.BasePrice,
                t.DurationMinutes,
                t.IsRequired,
                t.IsActive))
            .ToListAsync(cancellationToken);

        return templates;
    }
}
