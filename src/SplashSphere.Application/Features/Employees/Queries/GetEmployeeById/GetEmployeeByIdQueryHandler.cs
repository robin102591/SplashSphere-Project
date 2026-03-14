using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Employees.Queries.GetEmployeeById;

public sealed class GetEmployeeByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Employees
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
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
                e.CreatedAt,
                e.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Employee '{request.Id}' was not found.");
    }
}
