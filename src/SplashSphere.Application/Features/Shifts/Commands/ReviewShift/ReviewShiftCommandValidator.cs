using FluentValidation;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Shifts.Commands.ReviewShift;

public sealed class ReviewShiftCommandValidator : AbstractValidator<ReviewShiftCommand>
{
    public ReviewShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.NewReviewStatus)
            .Must(s => s is ReviewStatus.Approved or ReviewStatus.Flagged)
            .WithMessage("Review status must be Approved or Flagged.");
        RuleFor(x => x.Notes)
            .NotEmpty()
            .WithMessage("Notes are required when flagging a shift.")
            .When(x => x.NewReviewStatus == ReviewStatus.Flagged);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null);
    }
}
