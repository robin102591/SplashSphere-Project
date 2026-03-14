using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Branches.Queries.GetBranches;

public sealed class GetBranchesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBranchesQuery, PagedResult<BranchDto>>
{
    public async Task<PagedResult<BranchDto>> Handle(
        GetBranchesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Branches.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(b =>
                b.Name.Contains(search) ||
                b.Code.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BranchDto(
                b.Id,
                b.Name,
                b.Code,
                b.Address,
                b.ContactNumber,
                b.IsActive,
                b.CreatedAt,
                b.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<BranchDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
