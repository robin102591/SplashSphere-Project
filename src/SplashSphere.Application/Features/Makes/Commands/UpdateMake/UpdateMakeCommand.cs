using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Makes.Commands.UpdateMake;

public sealed record UpdateMakeCommand(string Id, string Name) : ICommand;
