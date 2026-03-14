using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(c =>
                c.FirstName.Contains(search) ||
                c.LastName.Contains(search)  ||
                (c.Email != null && c.Email.Contains(search)) ||
                (c.ContactNumber != null && c.ContactNumber.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerDto(
                c.Id,
                c.FirstName,
                c.LastName,
                c.FirstName + " " + c.LastName,
                c.Email,
                c.ContactNumber,
                c.Notes,
                c.IsActive,
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<CustomerDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
