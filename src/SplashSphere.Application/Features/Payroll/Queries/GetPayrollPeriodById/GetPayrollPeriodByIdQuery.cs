using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriodById;

/// <summary>Returns a payroll period with all of its employee entries.</summary>
public sealed record GetPayrollPeriodByIdQuery(string PeriodId) : IQuery<PayrollPeriodDetailDto?>;
