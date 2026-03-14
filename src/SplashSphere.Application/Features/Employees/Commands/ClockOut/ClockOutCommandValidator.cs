using FluentValidation;

namespace SplashSphere.Application.Features.Employees.Commands.ClockOut;

public sealed class ClockOutCommandValidator : AbstractValidator<ClockOutCommand>
{
    public ClockOutCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}
