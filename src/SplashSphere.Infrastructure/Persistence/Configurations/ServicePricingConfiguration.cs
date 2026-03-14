using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ServicePricingConfiguration : IEntityTypeConfiguration<ServicePricing>
{
    public void Configure(EntityTypeBuilder<ServicePricing> builder)
    {
        builder.ToTable("ServicePricing");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        // TenantId is a filter discriminator — no FK navigation to Tenant
        builder.Property(sp => sp.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sp => sp.ServiceId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(sp => sp.VehicleTypeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(sp => sp.SizeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(sp => sp.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(sp => sp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sp => sp.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: pricing rows are owned by the service
        builder.HasOne(sp => sp.Service)
            .WithMany(s => s.Pricing)
            .HasForeignKey(sp => sp.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete a VehicleType or Size that has pricing rows
        builder.HasOne(sp => sp.VehicleType)
            .WithMany()
            .HasForeignKey(sp => sp.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.Size)
            .WithMany()
            .HasForeignKey(sp => sp.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique constraint: one price per (service, vehicleType, size) cell ─
        builder.HasIndex(sp => new { sp.ServiceId, sp.VehicleTypeId, sp.SizeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(sp => sp.TenantId);
        builder.HasIndex(sp => sp.ServiceId);
    }
}
