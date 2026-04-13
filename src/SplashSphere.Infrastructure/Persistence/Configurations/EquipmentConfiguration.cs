using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.BranchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Brand)
            .HasMaxLength(256);

        builder.Property(e => e.Model)
            .HasMaxLength(256);

        builder.Property(e => e.SerialNumber)
            .HasMaxLength(256);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.PurchaseDate);

        builder.Property(e => e.PurchaseCost)
            .HasPrecision(14, 2);

        builder.Property(e => e.WarrantyExpiry);

        builder.Property(e => e.Location)
            .HasMaxLength(256);

        builder.Property(e => e.Notes);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(e => e.Tenant)
            .WithMany(t => t.Equipment)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Branch)
            .WithMany(b => b.Equipment)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(e => new { e.TenantId, e.BranchId, e.Status });
        builder.HasIndex(e => e.TenantId);
    }
}
