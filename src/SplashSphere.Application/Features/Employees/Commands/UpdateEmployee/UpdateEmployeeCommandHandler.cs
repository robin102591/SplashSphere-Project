using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.UpdateEmployee;

public sealed class UpdateEmployeeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee is null)
            return Result.Failure(Error.NotFound("Employee", request.Id));

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != employee.Email)
        {
            var emailConflict = await context.Employees
                .AnyAsync(e => e.Email == request.Email && e.Id != request.Id, cancellationToken);

            if (emailConflict)
                return Result.Failure(Error.Conflict($"An employee with email '{request.Email}' already exists."));
        }

        // DailyRate must remain null for Commission-type employees.
        if (employee.EmployeeType == EmployeeType.Commission && request.DailyRate.HasValue)
            return Result.Failure(Error.Validation("DailyRate must be null for Commission-type employees."));

        employee.FirstName     = request.FirstName;
        employee.LastName      = request.LastName;
        employee.DailyRate     = request.DailyRate;
        employee.Email         = request.Email;
        employee.ContactNumber = request.ContactNumber;
        employee.HiredDate     = request.HiredDate;

        return Result.Success();
    }
}
