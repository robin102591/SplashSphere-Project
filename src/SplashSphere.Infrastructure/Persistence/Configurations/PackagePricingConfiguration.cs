using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PackagePricingConfiguration : IEntityTypeConfiguration<PackagePricing>
{
    public void Configure(EntityTypeBuilder<PackagePricing> builder)
    {
        builder.ToTable("PackagePricing");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(pp => pp.Id);
        builder.Property(pp => pp.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(pp => pp.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pp => pp.PackageId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(pp => pp.VehicleTypeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(pp => pp.SizeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(pp => pp.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(pp => pp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(pp => pp.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: pricing rows are owned by the package
        builder.HasOne(pp => pp.Package)
            .WithMany(p => p.Pricing)
            .HasForeignKey(pp => pp.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pp => pp.VehicleType)
            .WithMany()
            .HasForeignKey(pp => pp.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pp => pp.Size)
            .WithMany()
            .HasForeignKey(pp => pp.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique constraint: one price per (package, vehicleType, size) cell ─
        builder.HasIndex(pp => new { pp.PackageId, pp.VehicleTypeId, pp.SizeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(pp => pp.TenantId);
        builder.HasIndex(pp => pp.PackageId);
    }
}
