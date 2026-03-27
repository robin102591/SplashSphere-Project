using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.CashAdvances;

public sealed record CashAdvanceDto(
    string Id,
    string EmployeeId,
    string EmployeeName,
    decimal Amount,
    decimal RemainingBalance,
    CashAdvanceStatus Status,
    string? Reason,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    decimal DeductionPerPeriod,
    DateTime CreatedAt,
    DateTime UpdatedAt);
