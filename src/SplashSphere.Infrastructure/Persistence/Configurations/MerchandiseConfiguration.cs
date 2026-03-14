using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class MerchandiseConfiguration : IEntityTypeConfiguration<Merchandise>
{
    public void Configure(EntityTypeBuilder<Merchandise> builder)
    {
        builder.ToTable("Merchandise");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(m => m.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(m => m.CategoryId)
            .HasMaxLength(36);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(256);

        // Stored uppercase + trimmed by the entity constructor
        builder.Property(m => m.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Description);

        builder.Property(m => m.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(m => m.CostPrice)
            .HasPrecision(10, 2);

        builder.Property(m => m.StockQuantity)
            .IsRequired();

        builder.Property(m => m.LowStockThreshold)
            .IsRequired();

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Computed / ignored ────────────────────────────────────────────────
        // IsLowStock is a pure in-memory computed property — not a stored column
        builder.Ignore(m => m.IsLowStock);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(m => m.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(m => m.Tenant)
            .WithMany()
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // SetNull: uncategorise items when a category is deleted rather than
        // cascade-deleting merchandise that may have transaction history
        builder.HasOne(m => m.Category)
            .WithMany(mc => mc.Items)
            .HasForeignKey(m => m.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Uniqueness ────────────────────────────────────────────────────────

        // SKU is unique per tenant — two tenants may use identical SKU strings
        builder.HasIndex(m => new { m.Sku, m.TenantId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(m => m.TenantId);
        builder.HasIndex(m => m.CategoryId);

        // Supports the low-stock alert job: WHERE StockQuantity <= LowStockThreshold
        builder.HasIndex(m => new { m.TenantId, m.StockQuantity });
    }
}
