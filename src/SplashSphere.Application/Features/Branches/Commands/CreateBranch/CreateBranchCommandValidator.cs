using FluentValidation;

namespace SplashSphere.Application.Features.Branches.Commands.CreateBranch;

public sealed class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(10)
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("Code must contain only letters and digits.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ContactNumber)
            .NotEmpty()
            .MaximumLength(50);
    }
}
