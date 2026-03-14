using FluentValidation;

namespace SplashSphere.Application.Features.ServiceCategories.Commands.UpdateServiceCategory;

public sealed class UpdateServiceCategoryCommandValidator : AbstractValidator<UpdateServiceCategoryCommand>
{
    public UpdateServiceCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
