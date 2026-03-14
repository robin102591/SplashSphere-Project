using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Queries.GetAttendance;

public sealed class GetAttendanceQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAttendanceQuery, PagedResult<AttendanceDto>>
{
    public async Task<PagedResult<AttendanceDto>> Handle(
        GetAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Attendances.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(a => a.Employee.BranchId == request.BranchId);

        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
            query = query.Where(a => a.EmployeeId == request.EmployeeId);

        if (request.From.HasValue)
            query = query.Where(a => a.Date >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.Date <= request.To.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee.LastName)
            .ThenBy(a => a.Employee.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AttendanceDto(
                a.Id,
                a.EmployeeId,
                a.Employee.FirstName + " " + a.Employee.LastName,
                a.Employee.Branch.Name,
                a.Date,
                a.TimeIn,
                a.TimeOut,
                a.Notes))
            .ToListAsync(cancellationToken);

        return PagedResult<AttendanceDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
