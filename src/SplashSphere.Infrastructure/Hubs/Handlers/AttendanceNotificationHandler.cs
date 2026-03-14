using MediatR;
using Microsoft.AspNetCore.SignalR;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="AttendanceRecordedEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>AttendanceUpdated</c> → branch group — POS attendance screen updates in real time.</item>
///   <item><c>DashboardMetricsUpdated</c> → tenant group — <c>ClockedInToday</c> KPI changed.</item>
/// </list>
/// </summary>
public sealed class AttendanceNotificationHandler(
    IHubContext<SplashSphereHub> hub)
    : INotificationHandler<DomainEventNotification<AttendanceRecordedEvent>>
{
    public async Task Handle(
        DomainEventNotification<AttendanceRecordedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        await hub.Clients
            .Group(SplashSphereHub.BranchGroup(e.TenantId, e.BranchId))
            .SendAsync("AttendanceUpdated", new AttendanceUpdatedPayload(
                e.AttendanceId,
                e.EmployeeId,
                e.EmployeeFullName,
                e.BranchId,
                e.Date,
                e.IsClockIn,
                e.TimeIn,
                e.TimeOut),
                cancellationToken);

        await hub.Clients
            .Group(SplashSphereHub.TenantGroup(e.TenantId))
            .SendAsync("DashboardMetricsUpdated", new DashboardMetricsUpdatedPayload(
                e.TenantId,
                e.BranchId),
                cancellationToken);
    }
}
