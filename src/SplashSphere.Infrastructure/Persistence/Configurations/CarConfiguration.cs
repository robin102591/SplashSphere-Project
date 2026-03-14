using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable("Cars");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(c => c.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(c => c.CustomerId)
            .HasMaxLength(36);

        builder.Property(c => c.VehicleTypeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(c => c.SizeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(c => c.MakeId)
            .HasMaxLength(36);

        builder.Property(c => c.ModelId)
            .HasMaxLength(36);

        // Stored uppercase + trimmed by the entity constructor
        builder.Property(c => c.PlateNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Color)
            .HasMaxLength(50);

        builder.Property(c => c.Year);

        builder.Property(c => c.Notes);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // SetNull: unlink the car from the customer if the customer is deleted,
        // preserving the vehicle record and its service history
        builder.HasOne(c => c.Customer)
            .WithMany(cu => cu.Cars)
            .HasForeignKey(c => c.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Restrict: cannot delete a VehicleType or Size while cars reference them
        builder.HasOne(c => c.VehicleType)
            .WithMany()
            .HasForeignKey(c => c.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Size)
            .WithMany()
            .HasForeignKey(c => c.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: if a Make is deleted, clear MakeId but keep the car record
        builder.HasOne(c => c.Make)
            .WithMany()
            .HasForeignKey(c => c.MakeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // SetNull: if a Model is deleted, clear ModelId but keep the car record
        builder.HasOne(c => c.Model)
            .WithMany()
            .HasForeignKey(c => c.ModelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Uniqueness ────────────────────────────────────────────────────────

        // Primary uniqueness: a plate number is unique within a tenant.
        // Two different tenants may service the same physical vehicle independently.
        builder.HasIndex(c => new { c.PlateNumber, c.TenantId })
            .IsUnique();

        // Plain plate number index: backs the fast POS lookup
        // GET /cars/lookup/{plateNumber} which filters by tenant via global query filter
        builder.HasIndex(c => c.PlateNumber);

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.VehicleTypeId);
    }
}
