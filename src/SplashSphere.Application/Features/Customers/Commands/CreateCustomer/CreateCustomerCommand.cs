using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string? Email,
    string? ContactNumber,
    string? Notes) : ICommand<string>;
