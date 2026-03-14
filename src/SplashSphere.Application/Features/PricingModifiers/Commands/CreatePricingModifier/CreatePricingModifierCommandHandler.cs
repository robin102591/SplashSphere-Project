using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.CreatePricingModifier;

public sealed class CreatePricingModifierCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreatePricingModifierCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreatePricingModifierCommand request,
        CancellationToken cancellationToken)
    {
        // Validate branch belongs to this tenant when provided.
        if (request.BranchId is not null)
        {
            var branchExists = await context.Branches
                .AnyAsync(b => b.Id == request.BranchId, cancellationToken);

            if (!branchExists)
                return Result.Failure<string>(Error.NotFound("Branch", request.BranchId));
        }

        var modifier = new PricingModifier(
            tenantContext.TenantId,
            request.Name,
            request.Type,
            request.Value)
        {
            BranchId       = request.BranchId,
            StartTime      = request.StartTime,
            EndTime        = request.EndTime,
            ActiveDayOfWeek = request.ActiveDayOfWeek,
            HolidayDate    = request.HolidayDate,
            HolidayName    = request.HolidayName,
            StartDate      = request.StartDate,
            EndDate        = request.EndDate,
        };

        context.PricingModifiers.Add(modifier);

        return Result.Success(modifier.Id);
    }
}
