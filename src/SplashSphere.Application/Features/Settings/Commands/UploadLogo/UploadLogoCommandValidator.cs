using FluentValidation;

namespace SplashSphere.Application.Features.Settings.Commands.UploadLogo;

public sealed class UploadLogoCommandValidator : AbstractValidator<UploadLogoCommand>
{
    private const long MaxBytes = 2 * 1024 * 1024; // 2 MB

    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/jpg",
        "image/webp",
    };

    public UploadLogoCommandValidator()
    {
        RuleFor(c => c.Content)
            .NotNull().WithMessage("File is required.");

        RuleFor(c => c.ContentLength)
            .LessThanOrEqualTo(MaxBytes)
            .WithMessage($"Logo must be {MaxBytes / 1024 / 1024}MB or smaller.");

        RuleFor(c => c.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Logo must be PNG, JPEG, or WebP.");
    }
}
