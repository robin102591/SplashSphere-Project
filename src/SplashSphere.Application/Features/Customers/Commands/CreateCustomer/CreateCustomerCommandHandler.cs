using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateCustomerCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await context.Customers
                .AnyAsync(c => c.Email == request.Email, cancellationToken);

            if (emailExists)
                return Result.Failure<string>(Error.Conflict($"A customer with email '{request.Email}' already exists."));
        }

        var customer = new Customer(
            tenantContext.TenantId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.ContactNumber);

        customer.Notes = request.Notes;

        context.Customers.Add(customer);

        return Result.Success(customer.Id);
    }
}
