using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.ClockOut;

/// <summary>
/// Records clock-out for an employee. Finds the open attendance row for today
/// (TimeOut == null) and sets TimeOut to UtcNow.
/// Enforces TimeOut > TimeIn before saving.
/// </summary>
public sealed record ClockOutCommand(string EmployeeId) : ICommand;
