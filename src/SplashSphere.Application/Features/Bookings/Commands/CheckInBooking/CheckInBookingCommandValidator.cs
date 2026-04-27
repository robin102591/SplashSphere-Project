using FluentValidation;

namespace SplashSphere.Application.Features.Bookings.Commands.CheckInBooking;

public sealed class CheckInBookingCommandValidator : AbstractValidator<CheckInBookingCommand>
{
    public CheckInBookingCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
    }
}
