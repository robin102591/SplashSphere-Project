---
name: prisma-reference
description: Original Prisma schema mapping reference for EF Core entity creation
---

# Prisma-to-EF Core Mapping Reference

This project does not use Prisma — it uses EF Core directly. This file serves as a
reference for the Prisma-to-EF Core type mapping patterns used when the schema was
originally designed from a Prisma-style spec.

## Type Mappings

| Prisma | C# | EF Core Configuration |
|---|---|---|
| `String @id @default(uuid())` | `string` | `.HasDefaultValueSql("gen_random_uuid()::text")` |
| `String @id @default(ulid())` | `string` | Generate in app: `Ulid.NewUlid().ToString()` |
| `Boolean @default(true)` | `bool` | `.HasDefaultValue(true)` |
| `DateTime @default(now())` | `DateTime` | `.HasDefaultValueSql("now()")` |
| `DateTime @updatedAt` | `DateTime` | Set via `AuditableEntityInterceptor` |
| `DateTime @db.Date` | `DateOnly` | `.HasColumnType("date")` |
| `Decimal @db.Decimal(10, 2)` | `decimal` | `.HasPrecision(10, 2)` |
| `@@unique([a, b])` | — | `.HasIndex(e => new { e.A, e.B }).IsUnique()` |
| `@@index([a])` | — | `.HasIndex(e => e.A)` |
| `onDelete: Cascade` | — | `.OnDelete(DeleteBehavior.Cascade)` |
| `onDelete: SetNull` | — | `.OnDelete(DeleteBehavior.SetNull)` |
| `onDelete: Restrict` | — | `.OnDelete(DeleteBehavior.Restrict)` |

## Enum Mapping
Prisma enums map to C# enums. EF Core stores them as integers by default.
For string storage: `.HasConversion<string>()`.

## Relation Patterns
- One-to-many: Parent has `ICollection<Child>`, child has `ParentId` FK + `Parent` nav.
- One-to-one: `.HasOne().WithOne().HasForeignKey<Child>(c => c.ParentId)`.
- Many-to-many: Explicit join entity preferred over EF Core implicit.

## Nullable FK Pattern
```csharp
builder.HasOne(e => e.Customer)
    .WithMany()
    .HasForeignKey(e => e.CustomerId)
    .OnDelete(DeleteBehavior.SetNull);
// Requires: public string? CustomerId { get; set; }
```
