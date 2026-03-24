using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Employees.Commands.InviteEmployee;

public sealed record InviteEmployeeCommand(string EmployeeId) : ICommand;
