using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.ServiceCategories.Commands.ToggleServiceCategoryStatus;

public sealed record ToggleServiceCategoryStatusCommand(string Id) : ICommand;
