using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.UpdateService;

public sealed record UpdateServiceCommand(
    string Id,
    string CategoryId,
    string Name,
    decimal BasePrice,
    string? Description) : ICommand;
