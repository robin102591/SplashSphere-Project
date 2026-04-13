using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class MaintenanceLogConfiguration : IEntityTypeConfiguration<MaintenanceLog>
{
    public void Configure(EntityTypeBuilder<MaintenanceLog> builder)
    {
        builder.ToTable("MaintenanceLogs");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(ml => ml.Id);
        builder.Property(ml => ml.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(ml => ml.EquipmentId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(ml => ml.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ml => ml.Description)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(ml => ml.Cost)
            .HasPrecision(14, 2);

        builder.Property(ml => ml.PerformedBy)
            .HasMaxLength(256);

        builder.Property(ml => ml.PerformedDate)
            .IsRequired();

        builder.Property(ml => ml.NextDueDate);

        builder.Property(ml => ml.NextDueHours);

        builder.Property(ml => ml.Notes);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(ml => ml.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(ml => ml.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(ml => ml.Equipment)
            .WithMany(e => e.MaintenanceLogs)
            .HasForeignKey(ml => ml.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(ml => ml.EquipmentId);
        builder.HasIndex(ml => new { ml.EquipmentId, ml.NextDueDate });
    }
}
