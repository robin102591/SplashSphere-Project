using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Commands.ClockOut;

public sealed class ClockOutCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ClockOutCommand, Result>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<Result> Handle(
        ClockOutCommand request,
        CancellationToken cancellationToken)
    {
        var now      = DateTime.UtcNow;
        var localNow = now + ManilaOffset;
        var today    = DateOnly.FromDateTime(localNow);

        // Find the open attendance record for today (clocked in, not yet clocked out).
        var attendance = await context.Attendances
            .FirstOrDefaultAsync(
                a => a.EmployeeId == request.EmployeeId
                  && a.Date == today
                  && a.TimeOut == null,
                cancellationToken);

        if (attendance is null)
            return Result.Failure(Error.NotFound(
                "Attendance",
                $"No open clock-in found for employee '{request.EmployeeId}' today ({today})."));

        // Guard: TimeOut must be after TimeIn (should always pass with wall-clock time,
        // but protects against sub-second edge cases or manual backdating).
        if (now <= attendance.TimeIn)
            return Result.Failure(Error.Validation(
                "Clock-out time must be after clock-in time."));

        attendance.TimeOut = now;

        return Result.Success();
    }
}
