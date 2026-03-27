using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.CashAdvances.Commands.DisburseCashAdvance;

public sealed class DisburseCashAdvanceCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<DisburseCashAdvanceCommand, Result>
{
    public async Task<Result> Handle(
        DisburseCashAdvanceCommand request,
        CancellationToken cancellationToken)
    {
        var advance = await context.CashAdvances
            .FirstOrDefaultAsync(ca => ca.Id == request.CashAdvanceId, cancellationToken);

        if (advance is null)
            return Result.Failure(Error.NotFound("CashAdvance", request.CashAdvanceId));

        if (advance.Status != CashAdvanceStatus.Approved)
            return Result.Failure(Error.Validation(
                $"Only Approved advances can be disbursed. Current status: '{advance.Status}'."));

        advance.Status = CashAdvanceStatus.Active;

        return Result.Success();
    }
}
