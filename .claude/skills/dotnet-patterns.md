---
name: dotnet-patterns
description: .NET 9 and Clean Architecture patterns specific to SplashSphere
---

# .NET Patterns

## CQRS Handler Pattern
- Commands return Result<T> (success/failure, not exceptions)
- Queries return DTOs, never entities
- Validators: FluentValidation AbstractValidator<TCommand>
- All handlers are sealed classes
- Inject IApplicationDbContext for data access

## Endpoint Pattern
```csharp
public static class ServiceEndpoints
{
    public static void MapServiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/services").RequireAuthorization();
        group.MapGet("/", GetServices);
        group.MapPost("/", CreateService);
        // ...
    }
}
```

## Domain Event Pattern
- Record types: public sealed record TransactionCompletedEvent(...) : IDomainEvent;
- Published via MediatR notifications
- Handlers in Application/Features/{Feature}/EventHandlers/

## Key Packages
MediatR 12.x, FluentValidation 11.x, Npgsql.EntityFrameworkCore.PostgreSQL 9.x,
Microsoft.AspNetCore.SignalR, Hangfire 1.8.x, Clerk.BackendAPI

## Coding Standards
- File-scoped namespaces, primary constructors, sealed classes
- Records for DTOs and commands
- CancellationToken everywhere
- No AutoMapper — use Mapster or manual mapping in handlers
