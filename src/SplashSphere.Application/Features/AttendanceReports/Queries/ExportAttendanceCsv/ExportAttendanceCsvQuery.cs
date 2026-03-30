using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.AttendanceReports.Queries.ExportAttendanceCsv;

public sealed record ExportAttendanceCsvQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null,
    string? EmployeeId = null) : IQuery<AttendanceCsvResult>;

public sealed record AttendanceCsvResult(
    string FileName,
    byte[] Content);
