using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Models.Commands.ToggleModelStatus;

public sealed record ToggleModelStatusCommand(string Id) : ICommand;
