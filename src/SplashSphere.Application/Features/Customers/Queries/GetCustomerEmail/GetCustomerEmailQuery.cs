using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Customers.Queries.GetCustomerEmail;

/// <summary>
/// Returns the email address for a customer, or null if the customer has
/// none on file. Internal helper used by handlers (digital-receipt email,
/// future notification flows) — not exposed via the API surface.
/// </summary>
public sealed record GetCustomerEmailQuery(string CustomerId) : IQuery<string?>;
