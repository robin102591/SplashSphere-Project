using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ServiceCommissionConfiguration : IEntityTypeConfiguration<ServiceCommission>
{
    public void Configure(EntityTypeBuilder<ServiceCommission> builder)
    {
        builder.ToTable("ServiceCommissions");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(sc => sc.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sc => sc.ServiceId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(sc => sc.VehicleTypeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(sc => sc.SizeId)
            .IsRequired()
            .HasMaxLength(36);

        // Store enum as its integer value
        builder.Property(sc => sc.Type)
            .IsRequired()
            .HasConversion<int>();

        // Nullable: null for Percentage type; set for FixedAmount and Hybrid
        builder.Property(sc => sc.FixedAmount)
            .HasPrecision(10, 2);

        // Nullable: null for FixedAmount type; set for Percentage and Hybrid
        builder.Property(sc => sc.PercentageRate)
            .HasPrecision(5, 2);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(sc => sc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sc => sc.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: commission rows are owned by the service
        builder.HasOne(sc => sc.Service)
            .WithMany(s => s.Commissions)
            .HasForeignKey(sc => sc.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete a VehicleType or Size that has commission rows
        builder.HasOne(sc => sc.VehicleType)
            .WithMany()
            .HasForeignKey(sc => sc.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sc => sc.Size)
            .WithMany()
            .HasForeignKey(sc => sc.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique constraint: one commission rule per (service, vehicleType, size) cell ─
        builder.HasIndex(sc => new { sc.ServiceId, sc.VehicleTypeId, sc.SizeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(sc => sc.TenantId);
        builder.HasIndex(sc => sc.ServiceId);
    }
}
