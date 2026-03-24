using FluentValidation;

namespace SplashSphere.Application.Features.Employees.Commands.InviteEmployee;

public sealed class InviteEmployeeCommandValidator : AbstractValidator<InviteEmployeeCommand>
{
    public InviteEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}
