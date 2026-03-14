using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Sizes.Commands.UpdateSize;

public sealed record UpdateSizeCommand(string Id, string Name) : ICommand;
