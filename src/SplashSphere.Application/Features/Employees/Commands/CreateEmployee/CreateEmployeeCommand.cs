using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.CreateEmployee;

public sealed record CreateEmployeeCommand(
    string BranchId,
    string FirstName,
    string LastName,
    EmployeeType EmployeeType,
    decimal? DailyRate,
    string? Email,
    string? ContactNumber,
    DateOnly? HiredDate) : ICommand<string>;
