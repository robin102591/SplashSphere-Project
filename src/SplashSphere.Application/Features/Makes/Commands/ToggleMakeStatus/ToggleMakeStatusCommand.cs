using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Makes.Commands.ToggleMakeStatus;

public sealed record ToggleMakeStatusCommand(string Id) : ICommand;
