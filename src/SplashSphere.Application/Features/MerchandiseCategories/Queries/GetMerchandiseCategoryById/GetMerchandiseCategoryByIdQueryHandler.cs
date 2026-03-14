using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.MerchandiseCategories.Queries.GetMerchandiseCategoryById;

public sealed class GetMerchandiseCategoryByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMerchandiseCategoryByIdQuery, MerchandiseCategoryDto>
{
    public async Task<MerchandiseCategoryDto> Handle(
        GetMerchandiseCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var c = await context.MerchandiseCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Merchandise category '{request.Id}' was not found.");

        return new MerchandiseCategoryDto(c.Id, c.Name, c.Description, c.IsActive, c.CreatedAt, c.UpdatedAt);
    }
}
