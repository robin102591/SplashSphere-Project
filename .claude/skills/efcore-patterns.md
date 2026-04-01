---
name: efcore-patterns
description: EF Core 9 configuration patterns for SplashSphere
---

# EF Core 9 Patterns

## Configuration Pattern
```csharp
public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    private readonly string _tenantId;
    public ServiceConfiguration(ITenantContext tenantContext) => _tenantId = tenantContext.TenantId;

    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.BasePrice).HasPrecision(10, 2);
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.IsActive);
        builder.HasQueryFilter(e => e.TenantId == _tenantId);
    }
}
```

## Tenant Filter
Every tenant-scoped entity MUST have:
```csharp
builder.HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
```

## Prisma → EF Core Quick Reference
| Prisma | EF Core |
|---|---|
| @id @default(uuid()) | .HasDefaultValueSql("gen_random_uuid()") |
| @unique | .HasIndex().IsUnique() |
| @@unique([a,b]) | .HasIndex(e => new { e.A, e.B }).IsUnique() |
| Decimal @db.Decimal(10,2) | .HasPrecision(10, 2) |
| @default(now()) | .HasDefaultValueSql("now()") |
| @updatedAt | AuditableEntityInterceptor |
| onDelete: Cascade | .OnDelete(DeleteBehavior.Cascade) |

## AuditableEntityInterceptor
Automatically sets CreatedAt on Added and UpdatedAt on Added/Modified entities.
All entities implement `IAuditableEntity { DateTime CreatedAt; DateTime UpdatedAt; }`.

## ID Strategy
- Most entities: `gen_random_uuid()::text` (server-generated UUID string)
- Transaction-like entities: `Ulid.NewUlid().ToString()` (app-generated, time-sortable)
