using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.ExportPayrollCsv;

public sealed record ExportPayrollCsvQuery(string PeriodId) : IQuery<PayrollCsvResult?>;

public sealed record PayrollCsvResult(
    string FileName,
    byte[] Content);
