using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ServiceSupplyUsageConfiguration : IEntityTypeConfiguration<ServiceSupplyUsage>
{
    public void Configure(EntityTypeBuilder<ServiceSupplyUsage> builder)
    {
        builder.ToTable("ServiceSupplyUsages");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(ssu => ssu.Id);
        builder.Property(ssu => ssu.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(ssu => ssu.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ssu => ssu.ServiceId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(ssu => ssu.SupplyItemId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(ssu => ssu.SizeId)
            .HasMaxLength(36);

        builder.Property(ssu => ssu.QuantityPerUse)
            .IsRequired()
            .HasPrecision(14, 4);

        builder.Property(ssu => ssu.Notes);

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(ssu => ssu.Service)
            .WithMany(s => s.SupplyUsages)
            .HasForeignKey(ssu => ssu.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ssu => ssu.SupplyItem)
            .WithMany(si => si.ServiceUsages)
            .HasForeignKey(ssu => ssu.SupplyItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ssu => ssu.Size)
            .WithMany(sz => sz.ServiceSupplyUsages)
            .HasForeignKey(ssu => ssu.SizeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Uniqueness ────────────────────────────────────────────────────────
        // One usage definition per service + supply item + size combination.
        // SizeId is nullable — PostgreSQL unique indexes include NULLs correctly
        // by default in recent versions, but for safety this works with EF Core 9.
        builder.HasIndex(ssu => new { ssu.ServiceId, ssu.SupplyItemId, ssu.SizeId })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(ssu => ssu.TenantId);
    }
}
