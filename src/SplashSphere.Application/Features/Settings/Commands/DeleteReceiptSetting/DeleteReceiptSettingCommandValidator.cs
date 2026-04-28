using FluentValidation;

namespace SplashSphere.Application.Features.Settings.Commands.DeleteReceiptSetting;

public sealed class DeleteReceiptSettingCommandValidator : AbstractValidator<DeleteReceiptSettingCommand>
{
    public DeleteReceiptSettingCommandValidator()
    {
        RuleFor(c => c.BranchId)
            .NotEmpty()
            .WithMessage("BranchId is required. The tenant default cannot be deleted.");
    }
}
