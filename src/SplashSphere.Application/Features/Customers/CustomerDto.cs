namespace SplashSphere.Application.Features.Customers;

public sealed record CustomerDto(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? ContactNumber,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CustomerDetailDto(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? ContactNumber,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<CustomerCarDto> Cars);

/// <summary>Car summary embedded in a customer detail response.</summary>
public sealed record CustomerCarDto(
    string Id,
    string PlateNumber,
    string VehicleTypeName,
    string SizeName,
    string? MakeName,
    string? ModelName,
    string? Color,
    int? Year);
