using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.InviteEmployee;

public sealed class InviteEmployeeCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IClerkOrganizationService clerkService)
    : IRequestHandler<InviteEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        InviteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            return Result.Failure(Error.NotFound("Employee", request.EmployeeId));

        if (string.IsNullOrWhiteSpace(employee.Email))
            return Result.Failure(Error.Validation(
                "Employee must have an email address to be invited."));

        if (!employee.IsActive)
            return Result.Failure(Error.Validation(
                "Cannot invite an inactive employee."));

        if (employee.UserId is not null)
            return Result.Failure(Error.Conflict(
                "Employee already has a linked user account."));

        await clerkService.InviteMemberAsync(
            tenantContext.TenantId,
            employee.Email,
            tenantContext.ClerkUserId,
            cancellationToken: cancellationToken);

        employee.InvitedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
