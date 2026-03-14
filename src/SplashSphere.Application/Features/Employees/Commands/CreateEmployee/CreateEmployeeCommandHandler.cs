using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.CreateEmployee;

public sealed class CreateEmployeeCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateEmployeeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == request.BranchId, cancellationToken);

        if (!branchExists)
            return Result.Failure<string>(Error.Validation("Branch ID is invalid."));

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await context.Employees
                .AnyAsync(e => e.Email == request.Email, cancellationToken);

            if (emailExists)
                return Result.Failure<string>(Error.Conflict($"An employee with email '{request.Email}' already exists."));
        }

        var employee = new Employee(
            tenantContext.TenantId,
            request.BranchId,
            request.FirstName,
            request.LastName,
            request.EmployeeType,
            request.DailyRate,
            request.Email,
            request.ContactNumber,
            request.HiredDate);

        context.Employees.Add(employee);

        return Result.Success(employee.Id);
    }
}
