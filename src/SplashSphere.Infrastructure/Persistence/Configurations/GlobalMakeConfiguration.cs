using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class GlobalMakeConfiguration : IEntityTypeConfiguration<GlobalMake>
{
    public void Configure(EntityTypeBuilder<GlobalMake> builder)
    {
        builder.ToTable("GlobalMakes");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(m => m.Name)
            .IsUnique()
            .HasDatabaseName("UX_GlobalMake_Name");
    }
}
