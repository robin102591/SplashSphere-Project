using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(po => po.Id);
        builder.Property(po => po.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(po => po.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(po => po.BranchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(po => po.SupplierId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(po => po.PoNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(po => po.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(po => po.TotalAmount)
            .IsRequired()
            .HasPrecision(14, 2);

        builder.Property(po => po.Notes);

        builder.Property(po => po.OrderDate);

        builder.Property(po => po.ExpectedDeliveryDate);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(po => po.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(po => po.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(po => po.Tenant)
            .WithMany(t => t.PurchaseOrders)
            .HasForeignKey(po => po.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(po => po.Branch)
            .WithMany()
            .HasForeignKey(po => po.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Uniqueness ────────────────────────────────────────────────────────
        // PO number is unique per tenant
        builder.HasIndex(po => new { po.TenantId, po.PoNumber })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(po => po.TenantId);
        builder.HasIndex(po => new { po.TenantId, po.Status });
    }
}
