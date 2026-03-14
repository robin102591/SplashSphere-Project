using Hangfire;
using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Queue.Commands.MarkNoShow;

namespace SplashSphere.Infrastructure.Services;

/// <summary>
/// Hangfire-backed implementation. Schedules a <see cref="MarkNoShowCommand"/>
/// to run after the specified delay. The command handler is idempotent —
/// it checks the entry's current status before acting.
/// </summary>
public sealed class BackgroundJobService : IBackgroundJobService
{
    public string ScheduleNoShowCheck(string queueEntryId, TimeSpan delay)
    {
        return BackgroundJob.Schedule<IMediator>(
            m => m.Send(new MarkNoShowCommand(queueEntryId), CancellationToken.None),
            delay);
    }
}
