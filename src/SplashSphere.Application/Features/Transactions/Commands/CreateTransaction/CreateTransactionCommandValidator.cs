using FluentValidation;

namespace SplashSphere.Application.Features.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.CarId)
            .NotEmpty().WithMessage("Car ID is required.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative.");

        // At least one service or package must be present
        RuleFor(x => x)
            .Must(x => x.Services.Count > 0 || x.Packages.Count > 0)
            .WithMessage("At least one service or package is required.")
            .OverridePropertyName("Services");

        // Services
        RuleForEach(x => x.Services).ChildRules(svc =>
        {
            svc.RuleFor(s => s.ServiceId)
                .NotEmpty().WithMessage("Service ID is required.");

            svc.RuleFor(s => s.EmployeeIds)
                .NotEmpty().WithMessage("At least one employee must be assigned to each service.");

            svc.RuleForEach(s => s.EmployeeIds)
                .NotEmpty().WithMessage("Employee ID cannot be empty.");
        });

        // No duplicate ServiceIds within the same transaction
        RuleFor(x => x.Services)
            .Must(services => services.Select(s => s.ServiceId).Distinct().Count() == services.Count)
            .WithMessage("Duplicate service IDs are not allowed in a single transaction.")
            .When(x => x.Services.Count > 0);

        // Packages
        RuleForEach(x => x.Packages).ChildRules(pkg =>
        {
            pkg.RuleFor(p => p.PackageId)
                .NotEmpty().WithMessage("Package ID is required.");

            pkg.RuleFor(p => p.EmployeeIds)
                .NotEmpty().WithMessage("At least one employee must be assigned to each package.");

            pkg.RuleForEach(p => p.EmployeeIds)
                .NotEmpty().WithMessage("Employee ID cannot be empty.");
        });

        // No duplicate PackageIds
        RuleFor(x => x.Packages)
            .Must(packages => packages.Select(p => p.PackageId).Distinct().Count() == packages.Count)
            .WithMessage("Duplicate package IDs are not allowed in a single transaction.")
            .When(x => x.Packages.Count > 0);

        // Merchandise
        RuleForEach(x => x.Merchandise).ChildRules(merch =>
        {
            merch.RuleFor(m => m.MerchandiseId)
                .NotEmpty().WithMessage("Merchandise ID is required.");

            merch.RuleFor(m => m.Quantity)
                .GreaterThanOrEqualTo(1).WithMessage("Merchandise quantity must be at least 1.");
        });

        // No duplicate MerchandiseIds
        RuleFor(x => x.Merchandise)
            .Must(items => items.Select(m => m.MerchandiseId).Distinct().Count() == items.Count)
            .WithMessage("Duplicate merchandise IDs are not allowed in a single transaction.")
            .When(x => x.Merchandise.Count > 0);
    }
}
