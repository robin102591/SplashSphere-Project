using FluentValidation;

namespace SplashSphere.Application.Features.BookingSettings.Commands.UpsertBookingSetting;

public sealed class UpsertBookingSettingCommandValidator : AbstractValidator<UpsertBookingSettingCommand>
{
    public UpsertBookingSettingCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();

        RuleFor(x => x)
            .Must(x => x.OpenTime < x.CloseTime)
            .WithMessage("OpenTime must be earlier than CloseTime.");

        RuleFor(x => x.SlotIntervalMinutes)
            .InclusiveBetween(10, 120)
            .WithMessage("SlotIntervalMinutes must be between 10 and 120.");

        RuleFor(x => x.MaxBookingsPerSlot)
            .GreaterThan(0)
            .WithMessage("MaxBookingsPerSlot must be greater than zero.");

        RuleFor(x => x.AdvanceBookingDays)
            .InclusiveBetween(1, 60)
            .WithMessage("AdvanceBookingDays must be between 1 and 60.");

        RuleFor(x => x.MinLeadTimeMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinLeadTimeMinutes cannot be negative.");

        RuleFor(x => x.NoShowGraceMinutes)
            .InclusiveBetween(0, 120)
            .WithMessage("NoShowGraceMinutes must be between 0 and 120.");
    }
}
