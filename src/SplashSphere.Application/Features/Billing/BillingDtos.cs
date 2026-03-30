using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Billing;

// ── Current Plan ────────────────────────────────────────────────────────────

public sealed record TenantPlanDto(
    string Tier,
    string Status,
    string PlanName,
    decimal MonthlyPrice,
    IReadOnlyList<string> Features,
    PlanLimitsDto Limits,
    TrialInfoDto? Trial,
    BillingInfoDto? Billing);

public sealed record PlanLimitsDto(
    int MaxBranches,
    int CurrentBranches,
    int MaxEmployees,
    int CurrentEmployees,
    int SmsPerMonth,
    int SmsUsedThisMonth);

public sealed record TrialInfoDto(
    DateTime StartDate,
    DateTime EndDate,
    int DaysRemaining,
    bool Expired);

public sealed record BillingInfoDto(
    DateTime? NextBillingDate,
    DateTime? LastPaymentDate,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd);

// ── Billing History ─────────────────────────────────────────────────────────

public sealed record BillingRecordDto(
    string Id,
    decimal Amount,
    string Currency,
    BillingType Type,
    BillingStatus Status,
    string? PaymentMethod,
    string? InvoiceNumber,
    DateTime BillingDate,
    DateTime? PaidDate,
    string? Notes);

// ── Checkout ────────────────────────────────────────────────────────────────

public sealed record CheckoutResultDto(
    string CheckoutUrl,
    string SessionId);
