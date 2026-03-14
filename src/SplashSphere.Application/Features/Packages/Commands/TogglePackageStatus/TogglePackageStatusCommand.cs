using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.TogglePackageStatus;

public sealed record TogglePackageStatusCommand(string Id) : ICommand;
