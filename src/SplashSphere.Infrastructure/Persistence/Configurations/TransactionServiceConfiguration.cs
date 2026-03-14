using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class TransactionServiceConfiguration : IEntityTypeConfiguration<TransactionService>
{
    public void Configure(EntityTypeBuilder<TransactionService> builder)
    {
        builder.ToTable("TransactionServices");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(ts => ts.Id);
        builder.Property(ts => ts.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(ts => ts.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ts => ts.TransactionId)
            .IsRequired()
            .HasMaxLength(26); // ULID

        builder.Property(ts => ts.ServiceId)
            .IsRequired()
            .HasMaxLength(36);

        // Snapshots of vehicle dimension at transaction time
        builder.Property(ts => ts.VehicleTypeId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(ts => ts.SizeId)
            .IsRequired()
            .HasMaxLength(36);

        // Resolved price after matrix lookup + modifier application
        builder.Property(ts => ts.UnitPrice)
            .IsRequired()
            .HasPrecision(10, 2);

        // Commission pool before employee split
        builder.Property(ts => ts.TotalCommission)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(ts => ts.Notes);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(ts => ts.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(ts => ts.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: line items are deleted when the parent transaction is deleted
        builder.HasOne(ts => ts.Transaction)
            .WithMany(t => t.Services)
            .HasForeignKey(ts => ts.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete a service with historical transaction records
        builder.HasOne(ts => ts.Service)
            .WithMany(s => s.TransactionServices)
            .HasForeignKey(ts => ts.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict: vehicle dimension snapshots reference master data
        builder.HasOne(ts => ts.VehicleType)
            .WithMany()
            .HasForeignKey(ts => ts.VehicleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ts => ts.Size)
            .WithMany()
            .HasForeignKey(ts => ts.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(ts => ts.TenantId);
        builder.HasIndex(ts => ts.TransactionId);
    }
}
