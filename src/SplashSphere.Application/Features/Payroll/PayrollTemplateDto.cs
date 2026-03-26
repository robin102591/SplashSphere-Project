using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Payroll;

public sealed record PayrollTemplateDto(
    string Id,
    string Name,
    AdjustmentType Type,
    decimal DefaultAmount,
    bool IsActive,
    int SortOrder);
