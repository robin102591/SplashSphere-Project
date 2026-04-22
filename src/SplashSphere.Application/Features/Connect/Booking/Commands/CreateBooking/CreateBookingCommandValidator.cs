using FluentValidation;

namespace SplashSphere.Application.Features.Connect.Booking.Commands.CreateBooking;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.ConnectVehicleId).NotEmpty();

        RuleFor(x => x.SlotStartUtc)
            .Must(d => d.Kind == DateTimeKind.Utc || d.Kind == DateTimeKind.Unspecified)
            .WithMessage("SlotStartUtc must be a UTC timestamp.");

        RuleFor(x => x.ServiceIds)
            .NotNull()
            .Must(ids => ids.Count > 0)
            .WithMessage("At least one service must be selected.")
            .Must(ids => ids.Distinct(StringComparer.Ordinal).Count() == ids.Count)
            .WithMessage("Service IDs must be unique.");
    }
}
