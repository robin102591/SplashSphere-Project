using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.CashAdvances.Commands.ApproveCashAdvance;

public sealed class ApproveCashAdvanceCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<ApproveCashAdvanceCommand, Result>
{
    public async Task<Result> Handle(
        ApproveCashAdvanceCommand request,
        CancellationToken cancellationToken)
    {
        var advance = await context.CashAdvances
            .FirstOrDefaultAsync(ca => ca.Id == request.CashAdvanceId, cancellationToken);

        if (advance is null)
            return Result.Failure(Error.NotFound("CashAdvance", request.CashAdvanceId));

        if (advance.Status != CashAdvanceStatus.Pending)
            return Result.Failure(Error.Validation(
                $"Only Pending advances can be approved. Current status: '{advance.Status}'."));

        // Resolve the User record from the current Clerk user
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.ClerkUserId == tenantContext.ClerkUserId, cancellationToken);

        advance.Status = CashAdvanceStatus.Approved;
        advance.ApprovedById = user?.Id;
        advance.ApprovedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
