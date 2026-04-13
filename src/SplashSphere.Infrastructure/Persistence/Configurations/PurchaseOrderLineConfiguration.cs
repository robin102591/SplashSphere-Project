using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("PurchaseOrderLines");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(l => l.PurchaseOrderId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(l => l.SupplyItemId)
            .HasMaxLength(36);

        builder.Property(l => l.MerchandiseId)
            .HasMaxLength(36);

        builder.Property(l => l.ItemName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(l => l.Quantity)
            .IsRequired()
            .HasPrecision(14, 4);

        builder.Property(l => l.ReceivedQuantity)
            .IsRequired()
            .HasPrecision(14, 4);

        builder.Property(l => l.UnitCost)
            .IsRequired()
            .HasPrecision(14, 4);

        builder.Property(l => l.TotalCost)
            .IsRequired()
            .HasPrecision(14, 4);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(l => l.PurchaseOrder)
            .WithMany(po => po.Lines)
            .HasForeignKey(l => l.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.SupplyItem)
            .WithMany()
            .HasForeignKey(l => l.SupplyItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Merchandise)
            .WithMany()
            .HasForeignKey(l => l.MerchandiseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(l => l.PurchaseOrderId);
    }
}
