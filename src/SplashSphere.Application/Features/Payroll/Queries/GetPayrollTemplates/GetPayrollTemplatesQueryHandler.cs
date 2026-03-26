using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollTemplates;

public sealed class GetPayrollTemplatesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPayrollTemplatesQuery, IReadOnlyList<PayrollTemplateDto>>
{
    public async Task<IReadOnlyList<PayrollTemplateDto>> Handle(
        GetPayrollTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        return await context.PayrollAdjustmentTemplates
            .AsNoTracking()
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .Select(t => new PayrollTemplateDto(
                t.Id,
                t.Name,
                t.Type,
                t.DefaultAmount,
                t.IsActive,
                t.SortOrder,
                t.IsSystemDefault))
            .ToListAsync(cancellationToken);
    }
}
