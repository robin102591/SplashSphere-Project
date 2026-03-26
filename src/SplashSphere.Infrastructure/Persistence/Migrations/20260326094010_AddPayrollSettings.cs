using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_TenantId_Year_CutOffWeek",
                table: "PayrollPeriods");

            migrationBuilder.CreateTable(
                name: "PayrollSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CutOffStartDay = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_TenantId_StartDate",
                table: "PayrollPeriods",
                columns: new[] { "TenantId", "StartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSettings_TenantId",
                table: "PayrollSettings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollSettings");

            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_TenantId_StartDate",
                table: "PayrollPeriods");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_TenantId_Year_CutOffWeek",
                table: "PayrollPeriods",
                columns: new[] { "TenantId", "Year", "CutOffWeek" },
                unique: true);
        }
    }
}
