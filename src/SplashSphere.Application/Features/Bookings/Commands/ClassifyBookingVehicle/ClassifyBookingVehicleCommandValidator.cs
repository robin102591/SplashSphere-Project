using FluentValidation;

namespace SplashSphere.Application.Features.Bookings.Commands.ClassifyBookingVehicle;

public sealed class ClassifyBookingVehicleCommandValidator : AbstractValidator<ClassifyBookingVehicleCommand>
{
    public ClassifyBookingVehicleCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
        RuleFor(x => x.VehicleTypeId).NotEmpty();
        RuleFor(x => x.SizeId).NotEmpty();
    }
}
