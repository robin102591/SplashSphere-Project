using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.ToggleEmployeeStatus;

public sealed class ToggleEmployeeStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleEmployeeStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleEmployeeStatusCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (employee is null)
            return Result.Failure(Error.NotFound("Employee", request.Id));

        employee.IsActive = !employee.IsActive;

        return Result.Success();
    }
}
