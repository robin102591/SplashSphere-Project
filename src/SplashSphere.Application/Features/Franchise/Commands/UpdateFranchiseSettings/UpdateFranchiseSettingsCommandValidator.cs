using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.UpdateFranchiseSettings;

public sealed class UpdateFranchiseSettingsCommandValidator : AbstractValidator<UpdateFranchiseSettingsCommand>
{
    public UpdateFranchiseSettingsCommandValidator()
    {
        RuleFor(x => x.RoyaltyRate)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Royalty rate must be between 0 and 1.");

        RuleFor(x => x.MarketingFeeRate)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Marketing fee rate must be between 0 and 1.");

        RuleFor(x => x.TechnologyFeeRate)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Technology fee rate must be between 0 and 1.");

        RuleFor(x => x.MaxBranchesPerFranchisee)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Max branches per franchisee must be at least 1.");
    }
}
