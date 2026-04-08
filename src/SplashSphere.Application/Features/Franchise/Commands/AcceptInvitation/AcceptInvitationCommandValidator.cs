using FluentValidation;

namespace SplashSphere.Application.Features.Franchise.Commands.AcceptInvitation;

public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.ContactNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(512);
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BranchAddress).NotEmpty().MaximumLength(512);
        RuleFor(x => x.BranchContactNumber).NotEmpty().MaximumLength(50);
    }
}
