using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Sizes.Commands.ToggleSizeStatus;

public sealed record ToggleSizeStatusCommand(string Id) : ICommand;
