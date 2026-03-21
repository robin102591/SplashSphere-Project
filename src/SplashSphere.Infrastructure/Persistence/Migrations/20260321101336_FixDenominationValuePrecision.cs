using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDenominationValuePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DenominationValue",
                table: "ShiftDenominations",
                type: "numeric(8,4)",
                precision: 8,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,4)",
                oldPrecision: 6,
                oldScale: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DenominationValue",
                table: "ShiftDenominations",
                type: "numeric(6,4)",
                precision: 6,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,4)",
                oldPrecision: 8,
                oldScale: 4);
        }
    }
}
