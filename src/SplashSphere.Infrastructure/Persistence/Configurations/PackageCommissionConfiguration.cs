using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PackageCommissionConfiguration : IEntityTypeConfiguration<PackageCommission>
{
    public void Configure(EntityTypeBuilder<PackageCommission> builder)
    {
        builder.ToTable("PackageCommissions");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(pc => pc.Id);
        builder.Property(pc => pc.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(pc => pc.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pc => pc.PackageId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(pc => pc.VehicleTypeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(pc => pc.SizeId)
            .IsRequired()
            .HasMaxLength(36);

        // Package commissions are always percentage-based — no FixedAmount field
        builder.Property(pc => pc.PercentageRate)
            .IsRequired()
            .HasPrecision(5, 2);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(pc => pc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(pc => pc.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: commission rows are owned by the package
        builder.HasOne(pc => pc.Package)
            .WithMany(p => p.Commissions)
            .HasForeignKey(pc => pc.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.VehicleType)
            .WithMany()
            .HasForeignKey(pc => pc.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pc => pc.Size)
            .WithMany()
            .HasForeignKey(pc => pc.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Unique constraint: one commission rule per (package, vehicleType, size) cell ─
        builder.HasIndex(pc => new { pc.PackageId, pc.VehicleTypeId, pc.SizeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(pc => pc.TenantId);
        builder.HasIndex(pc => pc.PackageId);
    }
}
