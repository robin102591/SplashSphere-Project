using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Frequency",
                table: "PayrollSettings",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalTips",
                table: "PayrollEntries",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "PayrollSettings");

            migrationBuilder.DropColumn(
                name: "TotalTips",
                table: "PayrollEntries");
        }
    }
}
