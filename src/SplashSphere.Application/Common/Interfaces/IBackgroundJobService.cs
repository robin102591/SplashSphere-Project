namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Abstracts Hangfire background job scheduling so the Application layer
/// has no direct dependency on Hangfire types.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Schedules a no-show check for a queue entry after the given delay.
    /// The scheduled job dispatches <c>MarkNoShowCommand</c> via MediatR;
    /// the handler is idempotent — it only marks NoShow if the entry is still in
    /// <see cref="Domain.Enums.QueueStatus.Called"/> state when the job fires.
    /// Returns the Hangfire job ID (can be used for cancellation if needed).
    /// </summary>
    string ScheduleNoShowCheck(string queueEntryId, TimeSpan delay);
}
