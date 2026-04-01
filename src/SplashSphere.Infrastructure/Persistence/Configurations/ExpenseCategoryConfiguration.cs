using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(c => c.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).HasMaxLength(50);
        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();

        builder.HasOne(c => c.Tenant).WithMany()
            .HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
