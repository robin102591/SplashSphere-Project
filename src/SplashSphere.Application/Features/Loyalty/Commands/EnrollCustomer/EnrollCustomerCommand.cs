using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Loyalty.Commands.EnrollCustomer;

public sealed record EnrollCustomerCommand(string CustomerId) : ICommand<string>;
