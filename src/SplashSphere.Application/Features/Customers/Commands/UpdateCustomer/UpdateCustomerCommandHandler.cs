using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCustomerCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
            return Result.Failure(Error.NotFound("Customer", request.Id));

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != customer.Email)
        {
            var emailConflict = await context.Customers
                .AnyAsync(c => c.Email == request.Email && c.Id != request.Id, cancellationToken);

            if (emailConflict)
                return Result.Failure(Error.Conflict($"A customer with email '{request.Email}' already exists."));
        }

        customer.FirstName     = request.FirstName;
        customer.LastName      = request.LastName;
        customer.Email         = request.Email;
        customer.ContactNumber = request.ContactNumber;
        customer.Notes         = request.Notes;

        return Result.Success();
    }
}
