using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class FranchiseServiceTemplateConfiguration : IEntityTypeConfiguration<FranchiseServiceTemplate>
{
    public void Configure(EntityTypeBuilder<FranchiseServiceTemplate> builder)
    {
        builder.ToTable("FranchiseServiceTemplates");

        builder.HasKey(fst => fst.Id);
        builder.Property(fst => fst.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(fst => fst.FranchisorTenantId).IsRequired().HasMaxLength(256);
        builder.Property(fst => fst.ServiceName).IsRequired().HasMaxLength(256);
        builder.Property(fst => fst.Description).HasMaxLength(1024);
        builder.Property(fst => fst.CategoryName).HasMaxLength(256);
        builder.Property(fst => fst.BasePrice).IsRequired().HasPrecision(10, 2);
        builder.Property(fst => fst.DurationMinutes).IsRequired();
        builder.Property(fst => fst.IsRequired).IsRequired().HasDefaultValue(false);
        builder.Property(fst => fst.PricingMatrixJson).HasColumnType("jsonb");
        builder.Property(fst => fst.CommissionMatrixJson).HasColumnType("jsonb");
        builder.Property(fst => fst.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(fst => fst.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(fst => fst.UpdatedAt).IsRequired();

        builder.HasOne(fst => fst.FranchisorTenant).WithMany()
            .HasForeignKey(fst => fst.FranchisorTenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
