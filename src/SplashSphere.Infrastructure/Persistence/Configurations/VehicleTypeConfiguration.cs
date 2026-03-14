using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
{
    public void Configure(EntityTypeBuilder<VehicleType> builder)
    {
        builder.ToTable("VehicleTypes");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(v => v.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(v => v.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(v => v.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(v => v.Tenant)
            .WithMany(t => t.VehicleTypes)
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        // A tenant cannot have two vehicle types with the same name
        builder.HasIndex(v => new { v.Name, v.TenantId }).IsUnique();
        builder.HasIndex(v => v.TenantId);
    }
}
