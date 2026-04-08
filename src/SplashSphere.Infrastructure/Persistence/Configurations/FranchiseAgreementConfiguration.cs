using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class FranchiseAgreementConfiguration : IEntityTypeConfiguration<FranchiseAgreement>
{
    public void Configure(EntityTypeBuilder<FranchiseAgreement> builder)
    {
        builder.ToTable("FranchiseAgreements");

        builder.HasKey(fa => fa.Id);
        builder.Property(fa => fa.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(fa => fa.FranchisorTenantId).IsRequired().HasMaxLength(256);
        builder.Property(fa => fa.FranchiseeTenantId).IsRequired().HasMaxLength(256);
        builder.Property(fa => fa.AgreementNumber).IsRequired().HasMaxLength(50);

        // Territory
        builder.Property(fa => fa.TerritoryName).IsRequired().HasMaxLength(256);
        builder.Property(fa => fa.TerritoryDescription).HasMaxLength(1024);
        builder.Property(fa => fa.ExclusiveTerritory).IsRequired().HasDefaultValue(false);

        // Contract terms
        builder.Property(fa => fa.StartDate).IsRequired();
        builder.Property(fa => fa.InitialFranchiseFee).IsRequired().HasPrecision(14, 2);
        builder.Property(fa => fa.Status).IsRequired().HasConversion<int>();

        // Customized rates
        builder.Property(fa => fa.CustomRoyaltyRate).HasPrecision(10, 4);
        builder.Property(fa => fa.CustomMarketingFeeRate).HasPrecision(10, 4);

        builder.Property(fa => fa.Notes).HasMaxLength(2048);
        builder.Property(fa => fa.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(fa => fa.UpdatedAt).IsRequired();

        // Unique: one active agreement per franchisor-franchisee pair
        builder.HasIndex(fa => new { fa.FranchisorTenantId, fa.FranchiseeTenantId }).IsUnique();

        // Two FKs to Tenant — both Restrict (cannot delete tenant with agreements)
        builder.HasOne(fa => fa.FranchisorTenant).WithMany()
            .HasForeignKey(fa => fa.FranchisorTenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(fa => fa.FranchiseeTenant).WithMany()
            .HasForeignKey(fa => fa.FranchiseeTenantId).OnDelete(DeleteBehavior.Restrict);
    }
}
