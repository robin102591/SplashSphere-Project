using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.PricingModifiers.Queries.GetPricingModifierById;

public sealed class GetPricingModifierByIdQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetPricingModifierByIdQuery, PricingModifierDto?>
{
    public async Task<PricingModifierDto?> Handle(
        GetPricingModifierByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.PricingModifiers
            .AsNoTracking()
            .Include(m => m.Branch)
            .Where(m => m.Id == request.Id)
            .Select(m => new PricingModifierDto(
                m.Id,
                m.Name,
                m.Type,
                m.Type.ToString(),
                m.Value,
                m.BranchId,
                m.Branch != null ? m.Branch.Name : null,
                m.StartTime,
                m.EndTime,
                m.ActiveDayOfWeek,
                m.HolidayDate,
                m.HolidayName,
                m.StartDate,
                m.EndDate,
                m.IsActive,
                m.CreatedAt,
                m.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
