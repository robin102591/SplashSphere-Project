using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Commands.ToggleMerchandiseStatus;

public sealed record ToggleMerchandiseStatusCommand(string Id) : ICommand;
