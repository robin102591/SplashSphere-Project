using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.ToggleEmployeeStatus;

public sealed record ToggleEmployeeStatusCommand(string Id) : ICommand;
