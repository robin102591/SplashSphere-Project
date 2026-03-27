using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.CashAdvances.Commands.CancelCashAdvance;

public sealed class CancelCashAdvanceCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<CancelCashAdvanceCommand, Result>
{
    public async Task<Result> Handle(
        CancelCashAdvanceCommand request,
        CancellationToken cancellationToken)
    {
        var advance = await context.CashAdvances
            .FirstOrDefaultAsync(ca => ca.Id == request.CashAdvanceId, cancellationToken);

        if (advance is null)
            return Result.Failure(Error.NotFound("CashAdvance", request.CashAdvanceId));

        if (advance.Status is not (CashAdvanceStatus.Pending or CashAdvanceStatus.Approved))
            return Result.Failure(Error.Validation(
                $"Only Pending or Approved advances can be cancelled. Current status: '{advance.Status}'."));

        advance.Status = CashAdvanceStatus.Cancelled;

        return Result.Success();
    }
}
