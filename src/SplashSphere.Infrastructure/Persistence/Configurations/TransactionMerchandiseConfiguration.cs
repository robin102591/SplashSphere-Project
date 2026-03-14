using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class TransactionMerchandiseConfiguration : IEntityTypeConfiguration<TransactionMerchandise>
{
    public void Configure(EntityTypeBuilder<TransactionMerchandise> builder)
    {
        builder.ToTable("TransactionMerchandise");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(tm => tm.Id);
        builder.Property(tm => tm.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(tm => tm.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(tm => tm.TransactionId)
            .IsRequired()
            .HasMaxLength(26); // ULID

        builder.Property(tm => tm.MerchandiseId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(tm => tm.Quantity)
            .IsRequired();

        // Snapshot of Merchandise.Price at sale time
        builder.Property(tm => tm.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        // ── Computed / ignored ────────────────────────────────────────────────
        // LineTotal = Quantity × UnitPrice — pure in-memory computation
        builder.Ignore(tm => tm.LineTotal);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(tm => tm.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(tm => tm.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: line items are deleted when the parent transaction is deleted
        builder.HasOne(tm => tm.Transaction)
            .WithMany(t => t.Merchandise)
            .HasForeignKey(tm => tm.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete merchandise with historical transaction records
        builder.HasOne(tm => tm.Merchandise)
            .WithMany(m => m.TransactionMerchandise)
            .HasForeignKey(tm => tm.MerchandiseId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(tm => tm.TenantId);
        builder.HasIndex(tm => tm.TransactionId);
    }
}
