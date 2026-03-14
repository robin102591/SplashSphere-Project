using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(string Id) : IQuery<CustomerDetailDto>;
