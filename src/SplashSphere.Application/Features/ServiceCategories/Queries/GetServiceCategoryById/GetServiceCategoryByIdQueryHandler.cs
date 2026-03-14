using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.ServiceCategories.Queries.GetServiceCategoryById;

public sealed class GetServiceCategoryByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetServiceCategoryByIdQuery, ServiceCategoryDto>
{
    public async Task<ServiceCategoryDto> Handle(
        GetServiceCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var c = await context.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Service category '{request.Id}' was not found.");

        return new ServiceCategoryDto(c.Id, c.Name, c.Description, c.IsActive, c.CreatedAt, c.UpdatedAt);
    }
}
