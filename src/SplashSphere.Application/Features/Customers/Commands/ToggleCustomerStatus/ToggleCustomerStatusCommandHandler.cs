using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Customers.Commands.ToggleCustomerStatus;

public sealed class ToggleCustomerStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleCustomerStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleCustomerStatusCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
            return Result.Failure(Error.NotFound("Customer", request.Id));

        customer.IsActive = !customer.IsActive;

        return Result.Success();
    }
}
