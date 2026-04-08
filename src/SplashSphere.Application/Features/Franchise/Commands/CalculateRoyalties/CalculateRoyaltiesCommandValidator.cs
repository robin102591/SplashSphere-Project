using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.CalculateRoyalties;

public sealed class CalculateRoyaltiesCommandValidator : AbstractValidator<CalculateRoyaltiesCommand>
{
    public CalculateRoyaltiesCommandValidator()
    {
        RuleFor(x => x.FranchiseeTenantId).NotEmpty();

        RuleFor(x => x.PeriodStart)
            .LessThan(x => x.PeriodEnd)
            .WithMessage("Period start must be before period end.");
    }
}
