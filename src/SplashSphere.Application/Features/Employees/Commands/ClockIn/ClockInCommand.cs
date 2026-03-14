using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.ClockIn;

/// <summary>
/// Records clock-in for an employee on the current Asia/Manila calendar date.
/// Creates the Attendance row; TimeIn is set to UtcNow by the handler.
/// Returns the new Attendance ID.
/// </summary>
public sealed record ClockInCommand(string EmployeeId) : ICommand<string>;
