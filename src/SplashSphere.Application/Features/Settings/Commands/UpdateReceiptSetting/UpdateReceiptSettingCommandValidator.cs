using FluentValidation;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateReceiptSetting;

public sealed class UpdateReceiptSettingCommandValidator : AbstractValidator<UpdateReceiptSettingCommand>
{
    public UpdateReceiptSettingCommandValidator()
    {
        RuleFor(c => c.ThankYouMessage)
            .NotEmpty().WithMessage("Thank-you message is required.")
            .MaximumLength(256);

        RuleFor(c => c.CustomHeaderText).MaximumLength(256);
        RuleFor(c => c.PromoText).MaximumLength(512);
        RuleFor(c => c.CustomFooterText).MaximumLength(512);

        RuleFor(c => c.LogoSize).IsInEnum();
        RuleFor(c => c.LogoPosition).IsInEnum();
        RuleFor(c => c.ReceiptWidth).IsInEnum();
        RuleFor(c => c.FontSize).IsInEnum();
    }
}
