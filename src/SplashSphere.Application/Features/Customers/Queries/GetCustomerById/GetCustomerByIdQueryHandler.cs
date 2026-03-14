using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto>
{
    public async Task<CustomerDetailDto> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new
            {
                c.Id, c.FirstName, c.LastName,
                c.Email, c.ContactNumber, c.Notes,
                c.IsActive, c.CreatedAt, c.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Customer '{request.Id}' was not found.");

        var cars = await context.Cars
            .AsNoTracking()
            .Where(car => car.CustomerId == request.Id)
            .OrderBy(car => car.PlateNumber)
            .Select(car => new CustomerCarDto(
                car.Id,
                car.PlateNumber,
                car.VehicleType.Name,
                car.Size.Name,
                car.Make != null ? car.Make.Name : null,
                car.Model != null ? car.Model.Name : null,
                car.Color,
                car.Year))
            .ToListAsync(cancellationToken);

        return new CustomerDetailDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.FirstName + " " + customer.LastName,
            customer.Email,
            customer.ContactNumber,
            customer.Notes,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt,
            cars);
    }
}
