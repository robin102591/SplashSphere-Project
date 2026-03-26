using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemDefault",
                table: "PayrollAdjustmentTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PayrollAdjustments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PayrollEntryId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TemplateId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_PayrollAdjustmentTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "PayrollAdjustmentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_PayrollEntries_PayrollEntryId",
                        column: x => x.PayrollEntryId,
                        principalTable: "PayrollEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_PayrollEntryId",
                table: "PayrollAdjustments",
                column: "PayrollEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_TemplateId",
                table: "PayrollAdjustments",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_TenantId",
                table: "PayrollAdjustments",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollAdjustments");

            migrationBuilder.DropColumn(
                name: "IsSystemDefault",
                table: "PayrollAdjustmentTemplates");
        }
    }
}
