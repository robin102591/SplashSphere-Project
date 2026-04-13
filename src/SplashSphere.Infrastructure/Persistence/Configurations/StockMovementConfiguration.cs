using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(sm => sm.Id);
        builder.Property(sm => sm.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(sm => sm.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sm => sm.BranchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(sm => sm.SupplyItemId)
            .HasMaxLength(36);

        builder.Property(sm => sm.MerchandiseId)
            .HasMaxLength(36);

        builder.Property(sm => sm.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sm => sm.Quantity)
            .IsRequired()
            .HasPrecision(14, 4);

        builder.Property(sm => sm.UnitCost)
            .HasPrecision(14, 4);

        builder.Property(sm => sm.TotalCost)
            .HasPrecision(14, 4);

        builder.Property(sm => sm.Reference)
            .HasMaxLength(256);

        builder.Property(sm => sm.Notes);

        builder.Property(sm => sm.PerformedByUserId)
            .HasMaxLength(256);

        builder.Property(sm => sm.MovementDate)
            .IsRequired();

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(sm => sm.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sm => sm.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(sm => sm.Tenant)
            .WithMany()
            .HasForeignKey(sm => sm.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.Branch)
            .WithMany()
            .HasForeignKey(sm => sm.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.SupplyItem)
            .WithMany(si => si.StockMovements)
            .HasForeignKey(sm => sm.SupplyItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.Merchandise)
            .WithMany(m => m.StockMovements)
            .HasForeignKey(sm => sm.MerchandiseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(sm => new { sm.TenantId, sm.SupplyItemId, sm.MovementDate });
        builder.HasIndex(sm => new { sm.TenantId, sm.MerchandiseId, sm.MovementDate });
        builder.HasIndex(sm => new { sm.TenantId, sm.BranchId, sm.Type });
    }
}
