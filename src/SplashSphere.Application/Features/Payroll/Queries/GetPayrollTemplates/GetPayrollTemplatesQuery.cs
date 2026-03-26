using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollTemplates;

public sealed record GetPayrollTemplatesQuery : IQuery<IReadOnlyList<PayrollTemplateDto>>;
