using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class LoyaltyTierConfigConfiguration : IEntityTypeConfiguration<LoyaltyTierConfig>
{
    public void Configure(EntityTypeBuilder<LoyaltyTierConfig> builder)
    {
        builder.ToTable("LoyaltyTierConfigs");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(t => t.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(t => t.LoyaltyProgramSettingsId).IsRequired().HasMaxLength(36);
        builder.Property(t => t.Tier).IsRequired().HasConversion<int>();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.MinimumLifetimePoints).IsRequired();
        builder.Property(t => t.PointsMultiplier).IsRequired().HasPrecision(5, 2);
        builder.Property(t => t.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(t => t.UpdatedAt).IsRequired();

        // One tier per type per settings record
        builder.HasIndex(t => new { t.LoyaltyProgramSettingsId, t.Tier }).IsUnique();

        builder.HasOne(t => t.Settings).WithMany(s => s.Tiers)
            .HasForeignKey(t => t.LoyaltyProgramSettingsId).OnDelete(DeleteBehavior.Cascade);
    }
}
