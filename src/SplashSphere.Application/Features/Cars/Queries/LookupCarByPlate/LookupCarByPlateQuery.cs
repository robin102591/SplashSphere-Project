using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Cars.Queries.LookupCarByPlate;

/// <summary>
/// Fast POS plate lookup: GET /cars/lookup/{plateNumber}.
/// Input is normalised to uppercase + trimmed before querying.
/// Returns null when no match is found (not an error — cashier will create the car).
/// </summary>
public sealed record LookupCarByPlateQuery(string PlateNumber) : IQuery<CarDto?>;
