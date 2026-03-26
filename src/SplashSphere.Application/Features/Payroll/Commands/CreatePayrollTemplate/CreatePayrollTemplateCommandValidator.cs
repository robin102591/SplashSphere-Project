using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.CreatePayrollTemplate;

public sealed class CreatePayrollTemplateCommandValidator : AbstractValidator<CreatePayrollTemplateCommand>
{
    public CreatePayrollTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.DefaultAmount)
            .GreaterThanOrEqualTo(0);
    }
}
