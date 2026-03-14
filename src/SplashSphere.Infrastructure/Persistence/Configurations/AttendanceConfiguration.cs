using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances");

        // ── Primary key ───────────────────────────────────────────────────────
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        // ── Scalar properties ─────────────────────────────────────────────────
        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.EmployeeId)
            .IsRequired()
            .HasMaxLength(36);

        // DateOnly → PostgreSQL "date" (no time component)
        // Stores the Asia/Manila calendar date of the work day
        builder.Property(a => a.Date)
            .IsRequired()
            .HasColumnType("date");

        // UTC timestamp; converted to PHT (UTC+8) for display
        builder.Property(a => a.TimeIn)
            .IsRequired();

        // Null while employee is still clocked in
        builder.Property(a => a.TimeOut);

        builder.Property(a => a.Notes);

        // ── Audit timestamps ──────────────────────────────────────────────────
        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────────────────────────────────────
        // Cascade: attendance records are owned by the employee
        builder.HasOne(a => a.Employee)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Unique constraint: one clock-in record per employee per calendar day ─
        builder.HasIndex(a => new { a.EmployeeId, a.Date })
            .IsUnique();

        // ── Additional indexes ────────────────────────────────────────────────
        builder.HasIndex(a => a.TenantId);

        // Payroll close counts attendance rows within a date range per employee
        builder.HasIndex(a => new { a.EmployeeId, a.Date, a.TenantId });
    }
}
