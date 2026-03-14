using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    string Id,
    string FirstName,
    string LastName,
    string? Email,
    string? ContactNumber,
    string? Notes) : ICommand;
