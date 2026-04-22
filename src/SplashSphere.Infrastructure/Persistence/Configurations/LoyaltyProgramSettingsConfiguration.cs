using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class LoyaltyProgramSettingsConfiguration : IEntityTypeConfiguration<LoyaltyProgramSettings>
{
    public void Configure(EntityTypeBuilder<LoyaltyProgramSettings> builder)
    {
        builder.ToTable("LoyaltyProgramSettings");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(s => s.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(s => s.PointsPerCurrencyUnit).IsRequired().HasPrecision(10, 2);
        builder.Property(s => s.CurrencyUnitAmount).IsRequired().HasPrecision(10, 2);
        builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(s => s.AutoEnroll).IsRequired().HasDefaultValue(true);
        builder.Property(s => s.ReferrerRewardPoints);
        builder.Property(s => s.ReferredRewardPoints);
        builder.Property(s => s.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(s => s.UpdatedAt).IsRequired();

        // Singleton per tenant
        builder.HasIndex(s => s.TenantId).IsUnique();

        builder.HasOne(s => s.Tenant).WithMany()
            .HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
