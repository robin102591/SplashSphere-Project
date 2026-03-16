using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.PricingModifiers.Queries.GetPricingModifiers;

public sealed class GetPricingModifiersQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetPricingModifiersQuery, IReadOnlyList<PricingModifierDto>>
{
    public async Task<IReadOnlyList<PricingModifierDto>> Handle(
        GetPricingModifiersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.PricingModifiers
            .AsNoTracking()
            .Include(m => m.Branch)
            .AsQueryable();

        if (request.BranchId is not null)
            query = query.Where(m => m.BranchId == request.BranchId || m.BranchId == null);

        if (request.Type.HasValue && Enum.IsDefined(typeof(ModifierType), request.Type.Value))
            query = query.Where(m => m.Type == (ModifierType)request.Type.Value);

        if (request.ActiveOnly == true)
            query = query.Where(m => m.IsActive);

        return (await query
            .OrderBy(m => m.Type)
            .ThenBy(m => m.Name)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Type,
                m.Value,
                m.BranchId,
                BranchName    = m.Branch != null ? m.Branch.Name : null,
                m.StartTime,
                m.EndTime,
                m.ActiveDayOfWeek,
                m.HolidayDate,
                m.HolidayName,
                m.StartDate,
                m.EndDate,
                m.IsActive,
                m.CreatedAt,
                m.UpdatedAt,
            })
            .ToListAsync(cancellationToken))
            .Select(m => new PricingModifierDto(
                m.Id,
                m.Name,
                m.Type,
                m.Type.ToString(),
                m.Value,
                m.BranchId,
                m.BranchName,
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
            .ToList();
    }
}
