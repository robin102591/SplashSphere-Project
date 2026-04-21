using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class GlobalModelConfiguration : IEntityTypeConfiguration<GlobalModel>
{
    public void Configure(EntityTypeBuilder<GlobalModel> builder)
    {
        builder.ToTable("GlobalModels");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(m => m.GlobalMakeId)
            .IsRequired()
            .HasMaxLength(36);

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

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(m => m.Make)
            .WithMany(gm => gm.Models)
            .HasForeignKey(m => m.GlobalMakeId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────────────────────────────────────
        // Model name unique within its parent make
        builder.HasIndex(m => new { m.GlobalMakeId, m.Name })
            .IsUnique()
            .HasDatabaseName("UX_GlobalModel_Make_Name");
    }
}
