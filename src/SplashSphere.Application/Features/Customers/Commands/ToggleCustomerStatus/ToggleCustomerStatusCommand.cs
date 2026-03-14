using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Customers.Commands.ToggleCustomerStatus;

public sealed record ToggleCustomerStatusCommand(string Id) : ICommand;
