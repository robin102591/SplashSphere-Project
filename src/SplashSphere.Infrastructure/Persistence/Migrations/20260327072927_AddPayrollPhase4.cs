using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollPhase4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoCalcGovernmentDeductions",
                table: "PayrollSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PayReleaseDayOffset",
                table: "PayrollSettings",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleasedAt",
                table: "PayrollPeriods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ScheduledReleaseDate",
                table: "PayrollPeriods",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GovernmentContributionBrackets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    DeductionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MinSalary = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MaxSalary = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    EmployeeShare = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    EffectiveYear = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernmentContributionBrackets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GovernmentContributionBrackets_DeductionType_EffectiveYear_~",
                table: "GovernmentContributionBrackets",
                columns: new[] { "DeductionType", "EffectiveYear", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GovernmentContributionBrackets");

            migrationBuilder.DropColumn(
                name: "AutoCalcGovernmentDeductions",
                table: "PayrollSettings");

            migrationBuilder.DropColumn(
                name: "PayReleaseDayOffset",
                table: "PayrollSettings");

            migrationBuilder.DropColumn(
                name: "ReleasedAt",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "ScheduledReleaseDate",
                table: "PayrollPeriods");
        }
    }
}
