using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SplashSphere.Domain.Entities;

namespace SplashSphere.Infrastructure.Persistence.Configurations;

public sealed class GovernmentContributionBracketConfiguration
    : IEntityTypeConfiguration<GovernmentContributionBracket>
{
    public void Configure(EntityTypeBuilder<GovernmentContributionBracket> builder)
    {
        builder.ToTable("GovernmentContributionBrackets");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .IsRequired()
            .HasMaxLength(36)
            .HasDefaultValueSql("gen_random_uuid()::text");

        builder.Property(b => b.DeductionType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.MinSalary)
            .IsRequired()
            .HasPrecision(12, 2);

        builder.Property(b => b.MaxSalary)
            .HasPrecision(12, 2);

        builder.Property(b => b.EmployeeShare)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(b => b.Rate)
            .IsRequired()
            .HasPrecision(8, 6);

        builder.Property(b => b.EffectiveYear)
            .IsRequired();

        builder.Property(b => b.SortOrder)
            .IsRequired();

        // Efficient lookup: find brackets for a deduction type + year, ordered by salary range
        builder.HasIndex(b => new { b.DeductionType, b.EffectiveYear, b.SortOrder });
    }
}
