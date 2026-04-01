using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).IsRequired().HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(e => e.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(e => e.BranchId).IsRequired().HasMaxLength(36);
        builder.Property(e => e.RecordedById).IsRequired().HasMaxLength(36);
        builder.Property(e => e.CategoryId).IsRequired().HasMaxLength(36);
        builder.Property(e => e.Amount).IsRequired().HasPrecision(10, 2);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Vendor).HasMaxLength(200);
        builder.Property(e => e.ReceiptReference).HasMaxLength(200);
        builder.Property(e => e.Frequency).IsRequired().HasConversion<int>();
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("now()");
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => new { e.TenantId, e.ExpenseDate });
        builder.HasIndex(e => new { e.TenantId, e.BranchId, e.ExpenseDate });

        builder.HasOne(e => e.Tenant).WithMany()
            .HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Branch).WithMany()
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.RecordedBy).WithMany()
            .HasForeignKey(e => e.RecordedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Category).WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
