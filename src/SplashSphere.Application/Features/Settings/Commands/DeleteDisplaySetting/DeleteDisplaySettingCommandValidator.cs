using FluentValidation;

namespace SplashSphere.Application.Features.Settings.Commands.DeleteDisplaySetting;

public sealed class DeleteDisplaySettingCommandValidator : AbstractValidator<DeleteDisplaySettingCommand>
{
    public DeleteDisplaySettingCommandValidator()
    {
        // Cannot reset the tenant default — it's permanent.
        RuleFor(x => x.BranchId)
            .NotEmpty()
            .WithMessage("BranchId is required. The tenant default cannot be deleted.");
    }
}
