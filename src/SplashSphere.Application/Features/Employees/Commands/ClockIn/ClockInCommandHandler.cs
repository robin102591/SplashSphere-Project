using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.ClockIn;

public sealed class ClockInCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<ClockInCommand, Result<string>>
{
    // UTC+8 offset for Asia/Manila — used to derive the local calendar date.
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<Result<string>> Handle(
        ClockInCommand request,
        CancellationToken cancellationToken)
    {
        var employeeExists = await context.Employees
            .AnyAsync(e => e.Id == request.EmployeeId && e.IsActive, cancellationToken);

        if (!employeeExists)
            return Result.Failure<string>(Error.NotFound("Employee", request.EmployeeId));

        var now       = DateTime.UtcNow;
        var localNow  = now + ManilaOffset;
        var today     = DateOnly.FromDateTime(localNow);

        // One attendance row per employee per day — block duplicate clock-in.
        var alreadyClockedIn = await context.Attendances
            .AnyAsync(a => a.EmployeeId == request.EmployeeId && a.Date == today, cancellationToken);

        if (alreadyClockedIn)
            return Result.Failure<string>(Error.Conflict(
                $"Employee '{request.EmployeeId}' has already clocked in today ({today})."));

        var attendance = new Attendance(tenantContext.TenantId, request.EmployeeId, today, now);
        context.Attendances.Add(attendance);

        return Result.Success(attendance.Id);
    }
}
