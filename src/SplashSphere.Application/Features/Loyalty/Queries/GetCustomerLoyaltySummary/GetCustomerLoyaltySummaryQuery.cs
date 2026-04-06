using MediatR;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetCustomerLoyaltySummary;

public sealed record GetCustomerLoyaltySummaryQuery(string CustomerId) : IRequest<CustomerLoyaltySummaryDto?>;
