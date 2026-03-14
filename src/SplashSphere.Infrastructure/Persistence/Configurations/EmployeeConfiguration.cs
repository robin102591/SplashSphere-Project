using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.BranchId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.Email)
            .HasMaxLength(256);

        builder.Property(e => e.ContactNumber)
            .HasMaxLength(50);

        builder.Property(e => e.EmployeeType)
            .IsRequired()
            .HasConversion<int>();

        // Null for Commission-type employees; required for Daily-type
        builder.Property(e => e.DailyRate)
            .HasPrecision(10, 2);

        // DateOnly → PostgreSQL "date"
        builder.Property(e => e.HiredDate)
            .HasColumnType("date");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ── Computed / ignored ────────────────────────────────────────────────
        builder.Ignore(e => e.FullName);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: cannot delete a branch that still has employees assigned to it
        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.BranchId);

        // Payroll close queries employees by type within a branch
        builder.HasIndex(e => new { e.BranchId, e.EmployeeType });

        builder.HasIndex(e => new { e.LastName, e.TenantId });
    }
}
