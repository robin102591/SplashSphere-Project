using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.CashAdvances.Commands.CreateCashAdvance;

public sealed class CreateCashAdvanceCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateCashAdvanceCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateCashAdvanceCommand request,
        CancellationToken cancellationToken)
    {
        var employeeExists = await context.Employees
            .AnyAsync(e => e.Id == request.EmployeeId && e.IsActive, cancellationToken);

        if (!employeeExists)
            return Result.Failure<string>(Error.NotFound("Employee", request.EmployeeId));

        var advance = new CashAdvance(
            tenantContext.TenantId,
            request.EmployeeId,
            request.Amount,
            request.DeductionPerPeriod,
            request.Reason);

        context.CashAdvances.Add(advance);

        return Result.Success(advance.Id);
    }
}
