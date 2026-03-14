using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.UpdatePricingModifier;

public sealed class UpdatePricingModifierCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdatePricingModifierCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePricingModifierCommand request,
        CancellationToken cancellationToken)
    {
        var modifier = await context.PricingModifiers
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (modifier is null)
            return Result.Failure(Error.NotFound("PricingModifier", request.Id));

        if (request.BranchId is not null)
        {
            var branchExists = await context.Branches
                .AnyAsync(b => b.Id == request.BranchId, cancellationToken);

            if (!branchExists)
                return Result.Failure(Error.NotFound("Branch", request.BranchId));
        }

        modifier.Name           = request.Name;
        modifier.Type           = request.Type;
        modifier.Value          = request.Value;
        modifier.BranchId       = request.BranchId;
        modifier.StartTime      = request.StartTime;
        modifier.EndTime        = request.EndTime;
        modifier.ActiveDayOfWeek = request.ActiveDayOfWeek;
        modifier.HolidayDate    = request.HolidayDate;
        modifier.HolidayName    = request.HolidayName;
        modifier.StartDate      = request.StartDate;
        modifier.EndDate        = request.EndDate;

        return Result.Success();
    }
}
