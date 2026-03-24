using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Queries.GetEmployees;

public sealed class GetEmployeesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    public async Task<PagedResult<EmployeeDto>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(e => e.BranchId == request.BranchId);

        if (request.EmployeeType.HasValue)
            query = query.Where(e => e.EmployeeType == request.EmployeeType.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(e =>
                e.FirstName.Contains(search) ||
                e.LastName.Contains(search)  ||
                (e.Email != null && e.Email.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.Branch.Name)
            .ThenBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EmployeeDto(
                e.Id,
                e.BranchId,
                e.Branch.Name,
                e.FirstName,
                e.LastName,
                e.FirstName + " " + e.LastName,
                e.Email,
                e.ContactNumber,
                e.EmployeeType,
                e.DailyRate,
                e.HiredDate,
                e.IsActive,
                e.UserId,
                e.InvitedAt,
                e.CreatedAt,
                e.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<EmployeeDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
