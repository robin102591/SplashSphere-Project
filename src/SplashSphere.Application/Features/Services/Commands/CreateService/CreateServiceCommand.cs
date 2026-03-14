using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.CreateService;

public sealed record CreateServiceCommand(
    string CategoryId,
    string Name,
    decimal BasePrice,
    string? Description) : ICommand<string>;
