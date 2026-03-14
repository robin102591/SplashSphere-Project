using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.ToggleServiceStatus;

public sealed record ToggleServiceStatusCommand(string Id) : ICommand;
