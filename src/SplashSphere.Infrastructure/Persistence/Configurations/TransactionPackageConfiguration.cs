using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class TransactionPackageConfiguration : IEntityTypeConfiguration<TransactionPackage>
{
    public void Configure(EntityTypeBuilder<TransactionPackage> builder)
    {
        builder.ToTable("TransactionPackages");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(tp => tp.Id);
        builder.Property(tp => tp.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(tp => tp.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(tp => tp.TransactionId)
            .IsRequired()
            .HasMaxLength(26); // ULID

        builder.Property(tp => tp.PackageId)
            .IsRequired()
            .HasMaxLength(36);

        // Snapshots of vehicle dimension at transaction time
        builder.Property(tp => tp.VehicleTypeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(tp => tp.SizeId)
            .IsRequired()
            .HasMaxLength(36);

        // Resolved package price after matrix lookup
        builder.Property(tp => tp.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        // Commission pool before employee split (always percentage-based for packages)
        builder.Property(tp => tp.TotalCommission)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(tp => tp.Notes);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(tp => tp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(tp => tp.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: line items are deleted when the parent transaction is deleted
        builder.HasOne(tp => tp.Transaction)
            .WithMany(t => t.Packages)
            .HasForeignKey(tp => tp.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete a package with historical transaction records
        builder.HasOne(tp => tp.Package)
            .WithMany(p => p.TransactionPackages)
            .HasForeignKey(tp => tp.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: vehicle dimension snapshots reference master data
        builder.HasOne(tp => tp.VehicleType)
            .WithMany()
            .HasForeignKey(tp => tp.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tp => tp.Size)
            .WithMany()
            .HasForeignKey(tp => tp.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(tp => tp.TenantId);
        builder.HasIndex(tp => tp.TransactionId);
    }
}
