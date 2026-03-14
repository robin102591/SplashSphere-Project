using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.UpdateEmployee;

/// <summary>
/// Updates employee details. EmployeeType is immutable after creation —
/// changing compensation type would invalidate historical payroll records.
/// DailyRate may be adjusted for Daily-type employees.
/// </summary>
public sealed record UpdateEmployeeCommand(
    string Id,
    string FirstName,
    string LastName,
    decimal? DailyRate,
    string? Email,
    string? ContactNumber,
    DateOnly? HiredDate) : ICommand;
