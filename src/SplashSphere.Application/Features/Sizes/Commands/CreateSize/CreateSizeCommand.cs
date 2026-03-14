using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Sizes.Commands.CreateSize;

public sealed record CreateSizeCommand(string Name) : ICommand<string>;
