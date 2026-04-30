using FluentValidation;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateCompanyProfile;

public sealed class UpdateCompanyProfileCommandValidator : AbstractValidator<UpdateCompanyProfileCommand>
{
    public UpdateCompanyProfileCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Business name is required.")
            .MaximumLength(256);

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid address.")
            .MaximumLength(256);

        RuleFor(c => c.ContactNumber)
            .NotEmpty().WithMessage("Contact number is required.")
            .MaximumLength(50);

        RuleFor(c => c.Website)
            .MaximumLength(256)
            .Must(BeAValidUrlOrNull).WithMessage("Website must be a valid URL.");

        RuleFor(c => c.Tagline).MaximumLength(200);
        RuleFor(c => c.StreetAddress).MaximumLength(256);
        RuleFor(c => c.Barangay).MaximumLength(128);
        RuleFor(c => c.City).MaximumLength(128);
        RuleFor(c => c.Province).MaximumLength(128);
        RuleFor(c => c.ZipCode).MaximumLength(20);
        RuleFor(c => c.TaxId).MaximumLength(50);
        RuleFor(c => c.BusinessPermitNo).MaximumLength(100);

        RuleFor(c => c.FacebookUrl)
            .MaximumLength(256)
            .Must(BeAValidUrlOrNull).WithMessage("Facebook URL must be a valid URL.");

        RuleFor(c => c.InstagramHandle).MaximumLength(64);
        RuleFor(c => c.GCashNumber).MaximumLength(50);

        // #RRGGBB or null. We store and validate as 7 chars including the hash.
        RuleFor(c => c.PrimaryColorHex)
            .MaximumLength(7)
            .Must(BeAValidHexColorOrNull).WithMessage("Primary color must be a #RRGGBB hex value.");
    }

    private static bool BeAValidUrlOrNull(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)) return true;
        return Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool BeAValidHexColorOrNull(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)) return true;
        return System.Text.RegularExpressions.Regex.IsMatch(candidate, @"^#[0-9A-Fa-f]{6}$");
    }
}
