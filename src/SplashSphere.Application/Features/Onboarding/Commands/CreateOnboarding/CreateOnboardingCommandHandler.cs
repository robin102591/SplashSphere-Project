using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Onboarding.Commands.CreateOnboarding;

public sealed class CreateOnboardingCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IClerkOrganizationService clerkService)
    : IRequestHandler<CreateOnboardingCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        // ── Load user (bypassing global filter — no TenantId yet) ─────────────
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.ClerkUserId == tenantContext.ClerkUserId, cancellationToken);

        if (user is null)
            return Result.Failure<string>(Error.Validation(
                "User record not found. Ensure your account was created before onboarding."));

        if (!string.IsNullOrWhiteSpace(user.TenantId))
            return Result.Failure<string>(Error.Validation(
                "This user has already completed onboarding."));

        // ── Create Clerk Organization ─────────────────────────────────────────
        string orgId;
        try
        {
            orgId = await clerkService.CreateOrganizationAsync(request.BusinessName, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Validation(
                $"Failed to create Clerk Organization: {ex.Message}"));
        }

        // ── Add user as org:admin in Clerk ────────────────────────────────────
        try
        {
            await clerkService.AddMemberAsync(
                orgId,
                tenantContext.ClerkUserId,
                "org:admin",
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Validation(
                $"Failed to add user to Clerk Organization: {ex.Message}"));
        }

        // ── Create Tenant (id = Clerk org id) ─────────────────────────────────
        var tenant = new Tenant(orgId, request.BusinessName, request.BusinessEmail,
            request.ContactNumber, request.Address);
        context.Tenants.Add(tenant);

        // ── Set tenant type ──────────────────────────────────────────────────
        tenant.TenantType = (TenantType)request.BusinessType;

        // ── If Franchisor, create default FranchiseSettings ──────────────────
        if (tenant.TenantType == TenantType.Franchisor)
        {
            context.FranchiseSettings.Add(new FranchiseSettings(orgId));
        }

        // ── Create first Branch ───────────────────────────────────────────────
        var branch = new Branch(orgId, request.BranchName, request.BranchCode,
            request.BranchAddress, request.BranchContactNumber);
        context.Branches.Add(branch);

        // ── Link User to Tenant ───────────────────────────────────────────────
        user.TenantId = orgId;
        user.Role     = "org:admin";

        // ── Create Trial subscription (14-day Growth experience) ─────────────
        var now = DateTime.UtcNow;
        var subscription = new TenantSubscription(orgId, PlanTier.Trial, SubscriptionStatus.Trial)
        {
            TrialStartDate = now,
            TrialEndDate = now.AddDays(14),
            SmsCountResetDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        context.TenantSubscriptions.Add(subscription);

        // ── Pre-seed government deduction templates ─────────────────────────
        var governmentTemplates = new[]
        {
            new PayrollAdjustmentTemplate(orgId, "SSS", AdjustmentType.Deduction, 0m)
                { IsSystemDefault = true, SortOrder = 1 },
            new PayrollAdjustmentTemplate(orgId, "PhilHealth", AdjustmentType.Deduction, 0m)
                { IsSystemDefault = true, SortOrder = 2 },
            new PayrollAdjustmentTemplate(orgId, "Pag-IBIG", AdjustmentType.Deduction, 100m)
                { IsSystemDefault = true, SortOrder = 3 },
            new PayrollAdjustmentTemplate(orgId, "Tax (BIR Withholding)", AdjustmentType.Deduction, 0m)
                { IsSystemDefault = true, SortOrder = 4 },
        };
        context.PayrollAdjustmentTemplates.AddRange(governmentTemplates);

        // ── Pre-seed default expense categories ──────────────────────────────
        var defaultCategories = new[]
        {
            "Water Bill", "Electricity", "Rent", "Soap & Chemicals",
            "Equipment Maintenance", "Employee Meals/Snacks", "Transportation",
            "Supplies (towels, sponges)", "Miscellaneous", "Insurance", "Taxes & Permits"
        };
        foreach (var name in defaultCategories)
            context.ExpenseCategories.Add(new ExpenseCategory(orgId, name));

        // ── Pre-seed vehicle types ──────────────────────────────────────────
        var vehicleTypeNames = new[] { "Sedan", "SUV", "Van", "Truck", "Hatchback", "Pickup", "Motorcycle" };
        foreach (var name in vehicleTypeNames)
            context.VehicleTypes.Add(new VehicleType(orgId, name));

        // ── Pre-seed vehicle sizes ──────────────────────────────────────────
        var sizeNames = new[] { "Small", "Medium", "Large", "XL" };
        foreach (var name in sizeNames)
            context.Sizes.Add(new Size(orgId, name));

        // ── Pre-seed makes + common models ──────────────────────────────────
        var makeModels = new Dictionary<string, string[]>
        {
            ["Toyota"] = ["Vios", "Innova", "Fortuner", "Hilux", "Wigo", "Avanza"],
            ["Honda"] = ["City", "Civic", "CR-V", "BR-V", "Jazz"],
            ["Mitsubishi"] = ["Mirage", "Xpander", "Montero Sport", "Strada"],
            ["Nissan"] = ["Almera", "Navara", "Terra", "Kicks"],
            ["Suzuki"] = ["Ertiga", "Swift", "Celerio", "Jimny", "Dzire"],
            ["Ford"] = ["EcoSport", "Territory", "Ranger", "Everest"],
        };
        foreach (var (makeName, models) in makeModels)
        {
            var make = new Make(orgId, makeName);
            context.Makes.Add(make);
            foreach (var modelName in models)
                context.Models.Add(new Model(orgId, make.Id, modelName));
        }

        // ── Pre-seed service categories ─────────────────────────────────────
        var serviceCategories = new[] { "Basic Services", "Premium Services", "Add-Ons", "Detailing" };
        foreach (var name in serviceCategories)
            context.ServiceCategories.Add(new ServiceCategory(orgId, name));

        // ── Pre-seed merchandise categories ─────────────────────────────────
        var merchCategories = new[]
        {
            "Cleaning Chemicals", "Wax & Polish", "Tire & Trim",
            "Towels & Cloths", "Brushes & Tools", "Packaging & Miscellaneous"
        };
        foreach (var name in merchCategories)
            context.MerchandiseCategories.Add(new MerchandiseCategory(orgId, name));

        // Populate TenantContext so downstream UnitOfWork / query filters work
        // correctly if any subsequent command in this request needs them.
        tenantContext.TenantId = orgId;

        return Result.Success(orgId);
    }
}
