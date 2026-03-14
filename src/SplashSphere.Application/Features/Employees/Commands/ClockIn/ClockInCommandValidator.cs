using FluentValidation;

namespace SplashSphere.Application.Features.Employees.Commands.ClockIn;

public sealed class ClockInCommandValidator : AbstractValidator<ClockInCommand>
{
    public ClockInCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}
