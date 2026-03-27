using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.CashAdvances.Queries.GetCashAdvanceById;

public sealed class GetCashAdvanceByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCashAdvanceByIdQuery, CashAdvanceDto?>
{
    public async Task<CashAdvanceDto?> Handle(
        GetCashAdvanceByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.CashAdvances
            .AsNoTracking()
            .Include(ca => ca.Employee)
            .Include(ca => ca.ApprovedBy)
            .Where(ca => ca.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
