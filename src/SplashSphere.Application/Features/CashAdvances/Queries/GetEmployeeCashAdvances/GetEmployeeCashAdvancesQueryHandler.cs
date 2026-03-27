using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.CashAdvances.Queries.GetEmployeeCashAdvances;

public sealed class GetEmployeeCashAdvancesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetEmployeeCashAdvancesQuery, IReadOnlyList<CashAdvanceDto>>
{
    public async Task<IReadOnlyList<CashAdvanceDto>> Handle(
        GetEmployeeCashAdvancesQuery request,
        CancellationToken cancellationToken)
    {
        return await context.CashAdvances
            .AsNoTracking()
            .Include(ca => ca.Employee)
            .Include(ca => ca.ApprovedBy)
            .Where(ca => ca.EmployeeId == request.EmployeeId)
            .OrderByDescending(ca => ca.CreatedAt)
            .Select(ca => new CashAdvanceDto(
                ca.Id,
                ca.EmployeeId,
                ca.Employee.FirstName + " " + ca.Employee.LastName,
                ca.Amount,
                ca.RemainingBalance,
                ca.Status,
                ca.Reason,
                ca.ApprovedBy != null ? ca.ApprovedBy.FirstName + " " + ca.ApprovedBy.LastName : null,
                ca.ApprovedAt,
                ca.DeductionPerPeriod,
                ca.CreatedAt,
                ca.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
