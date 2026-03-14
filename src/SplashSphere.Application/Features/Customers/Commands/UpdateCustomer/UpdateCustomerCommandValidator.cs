using FluentValidation;

namespace SplashSphere.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(200);

        RuleFor(x => x.ContactNumber)
            .MaximumLength(50);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}
