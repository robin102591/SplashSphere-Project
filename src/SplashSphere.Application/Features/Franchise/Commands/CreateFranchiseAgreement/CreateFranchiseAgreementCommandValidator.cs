using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.CreateFranchiseAgreement;

public sealed class CreateFranchiseAgreementCommandValidator : AbstractValidator<CreateFranchiseAgreementCommand>
{
    public CreateFranchiseAgreementCommandValidator()
    {
        RuleFor(x => x.FranchiseeTenantId).NotEmpty();

        RuleFor(x => x.AgreementNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.TerritoryName).NotEmpty();

        RuleFor(x => x.InitialFranchiseFee)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial franchise fee must be zero or greater.");

        RuleFor(x => x.CustomRoyaltyRate)
            .InclusiveBetween(0m, 1m)
            .When(x => x.CustomRoyaltyRate.HasValue)
            .WithMessage("Custom royalty rate must be between 0 and 1.");

        RuleFor(x => x.CustomMarketingFeeRate)
            .InclusiveBetween(0m, 1m)
            .When(x => x.CustomMarketingFeeRate.HasValue)
            .WithMessage("Custom marketing fee rate must be between 0 and 1.");
    }
}
