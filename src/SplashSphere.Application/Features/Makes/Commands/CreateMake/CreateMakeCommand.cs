using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Makes.Commands.CreateMake;

public sealed record CreateMakeCommand(string Name) : ICommand<string>;
