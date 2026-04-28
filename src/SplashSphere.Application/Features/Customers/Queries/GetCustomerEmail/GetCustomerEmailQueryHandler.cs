using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Customers.Queries.GetCustomerEmail;

public sealed class GetCustomerEmailQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomerEmailQuery, string?>
{
    public async Task<string?> Handle(
        GetCustomerEmailQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Customers
            .AsNoTracking()
            .Where(c => c.Id == request.CustomerId)
            .Select(c => c.Email)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
