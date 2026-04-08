using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class FranchiseSettingsConfiguration : IEntityTypeConfiguration<FranchiseSettings>
{
    public void Configure(EntityTypeBuilder<FranchiseSettings> builder)
    {
        builder.ToTable("FranchiseSettings");

        builder.HasKey(fs => fs.Id);
        builder.Property(fs => fs.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(fs => fs.TenantId).IsRequired().HasMaxLength(256);

        // Royalty configuration
        builder.Property(fs => fs.RoyaltyRate).IsRequired().HasPrecision(10, 4);
        builder.Property(fs => fs.MarketingFeeRate).IsRequired().HasPrecision(10, 4);
        builder.Property(fs => fs.TechnologyFeeRate).IsRequired().HasPrecision(10, 4);
        builder.Property(fs => fs.RoyaltyBasis).IsRequired().HasConversion<int>();
        builder.Property(fs => fs.RoyaltyFrequency).IsRequired().HasConversion<int>();

        // Standardization controls
        builder.Property(fs => fs.EnforceStandardServices).IsRequired().HasDefaultValue(false);
        builder.Property(fs => fs.EnforceStandardPricing).IsRequired().HasDefaultValue(false);
        builder.Property(fs => fs.AllowLocalServices).IsRequired().HasDefaultValue(true);
        builder.Property(fs => fs.MaxPriceVariance).HasPrecision(10, 4);
        builder.Property(fs => fs.EnforceBranding).IsRequired().HasDefaultValue(false);

        // Network defaults
        builder.Property(fs => fs.DefaultFranchiseePlan).IsRequired().HasConversion<int>();
        builder.Property(fs => fs.MaxBranchesPerFranchisee).IsRequired().HasDefaultValue(3);

        builder.Property(fs => fs.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(fs => fs.UpdatedAt).IsRequired();

        // One-to-one with Tenant (franchisor)
        builder.HasIndex(fs => fs.TenantId).IsUnique();
        builder.HasOne(fs => fs.Tenant).WithOne(t => t.FranchiseSettings)
            .HasForeignKey<FranchiseSettings>(fs => fs.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
