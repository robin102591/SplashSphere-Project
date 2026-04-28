using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Subscription;

/// <summary>
/// Static catalog of all plan definitions. The single source of truth for
/// features, limits, and pricing per tier.
/// </summary>
public static class PlanCatalog
{
    private static readonly HashSet<string> CoreFeatures =
    [
        FeatureKeys.Pos, FeatureKeys.CommissionTracking, FeatureKeys.WeeklyPayroll,
        FeatureKeys.BasicReports, FeatureKeys.CustomerManagement, FeatureKeys.VehicleManagement,
        FeatureKeys.EmployeeManagement, FeatureKeys.MerchandiseManagement,
        FeatureKeys.SupplyTracking
    ];

    private static readonly HashSet<string> GrowthFeatures =
    [
        ..CoreFeatures,
        FeatureKeys.QueueManagement, FeatureKeys.CustomerLoyalty, FeatureKeys.CashAdvanceTracking,
        FeatureKeys.ExpenseTracking, FeatureKeys.ShiftManagement, FeatureKeys.ProfitLossReports,
        FeatureKeys.SmsNotifications, FeatureKeys.PricingModifiers,
        FeatureKeys.PurchaseOrders, FeatureKeys.EquipmentManagement,
        FeatureKeys.SupplyUsageAutoDeduction, FeatureKeys.CostPerWashReports,
        FeatureKeys.OnlineBooking, FeatureKeys.ConnectDirectoryListing, FeatureKeys.ReferralProgram,
        FeatureKeys.DigitalReceipts
    ];

    private static readonly HashSet<string> EnterpriseFeatures =
    [
        ..GrowthFeatures,
        FeatureKeys.ApiAccess, FeatureKeys.CustomIntegrations,
        FeatureKeys.FranchiseManagement, FeatureKeys.BranchReceiptOverrides
    ];

    public static readonly PlanDefinition Trial = new()
    {
        Tier = PlanTier.Trial,
        Name = "Free Trial",
        MonthlyPrice = 0,
        MaxBranches = 1,
        MaxEmployees = 5,
        SmsPerMonth = 10,
        Features = [..GrowthFeatures] // Trial gets Growth features for 14 days
    };

    public static readonly PlanDefinition Starter = new()
    {
        Tier = PlanTier.Starter,
        Name = "Starter",
        MonthlyPrice = 1499m,
        MaxBranches = 1,
        MaxEmployees = 5,
        SmsPerMonth = 0,
        Features = [..CoreFeatures]
    };

    public static readonly PlanDefinition Growth = new()
    {
        Tier = PlanTier.Growth,
        Name = "Growth",
        MonthlyPrice = 2999m,
        MaxBranches = 3,
        MaxEmployees = 15,
        SmsPerMonth = 50,
        Features = [..GrowthFeatures]
    };

    public static readonly PlanDefinition Enterprise = new()
    {
        Tier = PlanTier.Enterprise,
        Name = "Enterprise",
        MonthlyPrice = 4999m,
        MaxBranches = int.MaxValue,
        MaxEmployees = int.MaxValue,
        SmsPerMonth = 200,
        Features = [..EnterpriseFeatures]
    };

    public static PlanDefinition GetPlan(PlanTier tier) => tier switch
    {
        PlanTier.Trial => Trial,
        PlanTier.Starter => Starter,
        PlanTier.Growth => Growth,
        PlanTier.Enterprise => Enterprise,
        _ => Starter
    };

    /// <summary>
    /// Returns the effective plan for a tenant, accounting for tenant type.
    /// Franchisors on Trial get Enterprise-level features so they can explore
    /// franchise management during their trial period.
    /// </summary>
    public static PlanDefinition GetEffectivePlan(PlanTier tier, TenantType tenantType)
    {
        if (tier == PlanTier.Trial && tenantType == TenantType.Franchisor)
            return new PlanDefinition
            {
                Tier = Trial.Tier,
                Name = Trial.Name,
                MonthlyPrice = Trial.MonthlyPrice,
                MaxBranches = Trial.MaxBranches,
                MaxEmployees = Trial.MaxEmployees,
                SmsPerMonth = Trial.SmsPerMonth,
                Features = [..EnterpriseFeatures]
            };

        return GetPlan(tier);
    }
}
