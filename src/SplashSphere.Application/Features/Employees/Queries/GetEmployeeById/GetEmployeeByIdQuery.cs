using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Employees.Queries.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(string Id) : IQuery<EmployeeDto>;
