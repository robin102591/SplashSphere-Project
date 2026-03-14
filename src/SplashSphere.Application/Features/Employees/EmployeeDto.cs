using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Employees;

public sealed record EmployeeDto(
    string Id,
    string BranchId,
    string BranchName,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? ContactNumber,
    EmployeeType EmployeeType,
    decimal? DailyRate,
    DateOnly? HiredDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
