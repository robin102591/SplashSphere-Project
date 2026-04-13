using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class SupplyItemConfiguration : IEntityTypeConfiguration<SupplyItem>
{
    public void Configure(EntityTypeBuilder<SupplyItem> builder)
    {
        builder.ToTable("SupplyItems");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(si => si.Id);
        builder.Property(si => si.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(si => si.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(si => si.BranchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(si => si.CategoryId)
            .HasMaxLength(36);

        builder.Property(si => si.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(si => si.Description);

        builder.Property(si => si.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(si => si.CurrentStock)
            .IsRequired()
            .HasPrecision(14, 4);

        builder.Property(si => si.ReorderLevel)
            .HasPrecision(14, 4);

        builder.Property(si => si.AverageUnitCost)
            .IsRequired()
            .HasPrecision(14, 4);

        builder.Property(si => si.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Computed / ignored ────────────────────────────────────────────────
        // IsLowStock is a pure in-memory computed property — not a stored column
        builder.Ignore(si => si.IsLowStock);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(si => si.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(si => si.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(si => si.Tenant)
            .WithMany(t => t.SupplyItems)
            .HasForeignKey(si => si.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.Branch)
            .WithMany(b => b.SupplyItems)
            .HasForeignKey(si => si.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: uncategorise items when a category is deleted
        builder.HasOne(si => si.Category)
            .WithMany(sc => sc.Items)
            .HasForeignKey(si => si.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(si => si.TenantId);
        builder.HasIndex(si => new { si.TenantId, si.BranchId });
        builder.HasIndex(si => si.CategoryId);
    }
}
