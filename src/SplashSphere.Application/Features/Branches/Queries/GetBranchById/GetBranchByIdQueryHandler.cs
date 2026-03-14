using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Branches.Queries.GetBranchById;

public sealed class GetBranchByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBranchByIdQuery, BranchDto>
{
    public async Task<BranchDto> Handle(
        GetBranchByIdQuery request,
        CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (branch is null)
            throw new NotFoundException($"Branch '{request.Id}' was not found.");

        return new BranchDto(
            branch.Id,
            branch.Name,
            branch.Code,
            branch.Address,
            branch.ContactNumber,
            branch.IsActive,
            branch.CreatedAt,
            branch.UpdatedAt);
    }
}
