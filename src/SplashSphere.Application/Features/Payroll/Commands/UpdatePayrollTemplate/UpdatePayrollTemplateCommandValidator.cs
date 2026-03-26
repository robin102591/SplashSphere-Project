using FluentValidation;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollTemplate;

public sealed class UpdatePayrollTemplateCommandValidator : AbstractValidator<UpdatePayrollTemplateCommand>
{
    public UpdatePayrollTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.DefaultAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
