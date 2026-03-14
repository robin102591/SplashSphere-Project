using FluentValidation;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Employees.Commands.CreateEmployee;

public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.EmployeeType)
            .IsInEnum();

        // Daily-rate employees must supply a positive rate.
        RuleFor(x => x.DailyRate)
            .NotNull().WithMessage("DailyRate is required for Daily-type employees.")
            .GreaterThan(0).WithMessage("DailyRate must be greater than zero.")
            .When(x => x.EmployeeType == EmployeeType.Daily);

        // Commission employees must not supply a daily rate.
        RuleFor(x => x.DailyRate)
            .Null().WithMessage("DailyRate must be null for Commission-type employees.")
            .When(x => x.EmployeeType == EmployeeType.Commission);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(200);

        RuleFor(x => x.ContactNumber)
            .MaximumLength(50);
    }
}
