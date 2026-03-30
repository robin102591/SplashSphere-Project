namespace SplashSphere.Domain.Subscription;

/// <summary>
/// Every gatable feature in SplashSphere has a unique string key.
/// Plans define which keys are enabled. The <see cref="PlanCatalog"/>
/// maps each <see cref="Enums.PlanTier"/> to a set of these keys.
/// </summary>
public static class FeatureKeys
{
    // ── Core (all plans) ─────────────────────────────────────────────────────
    public const string Pos = "pos";
    public const string CommissionTracking = "commission_tracking";
    public const string WeeklyPayroll = "weekly_payroll";
    public const string BasicReports = "basic_reports";
    public const string CustomerManagement = "customer_management";
    public const string VehicleManagement = "vehicle_management";
    public const string EmployeeManagement = "employee_management";
    public const string MerchandiseManagement = "merchandise_management";

    // ── Growth (Growth + Enterprise) ─────────────────────────────────────────
    public const string QueueManagement = "queue_management";
    public const string CustomerLoyalty = "customer_loyalty";
    public const string CashAdvanceTracking = "cash_advance_tracking";
    public const string ExpenseTracking = "expense_tracking";
    public const string ShiftManagement = "shift_management";
    public const string ProfitLossReports = "profit_loss_reports";
    public const string SmsNotifications = "sms_notifications";
    public const string PricingModifiers = "pricing_modifiers";

    // ── Enterprise only ──────────────────────────────────────────────────────
    public const string ApiAccess = "api_access";
    public const string CustomIntegrations = "custom_integrations";
}
