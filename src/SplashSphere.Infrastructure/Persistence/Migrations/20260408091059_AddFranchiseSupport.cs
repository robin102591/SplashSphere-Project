using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFranchiseSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessPermitNo",
                table: "Tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FranchiseCode",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentTenantId",
                table: "Tenants",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantType",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FranchiseAgreements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    FranchisorTenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FranchiseeTenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AgreementNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TerritoryName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TerritoryDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ExclusiveTerritory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InitialFranchiseFee = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CustomRoyaltyRate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    CustomMarketingFeeRate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseAgreements_Tenants_FranchiseeTenantId",
                        column: x => x.FranchiseeTenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FranchiseAgreements_Tenants_FranchisorTenantId",
                        column: x => x.FranchisorTenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FranchiseInvitations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    FranchisorTenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FranchiseCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TerritoryName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AcceptedByTenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseInvitations_Tenants_FranchisorTenantId",
                        column: x => x.FranchisorTenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FranchiseServiceTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    FranchisorTenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CategoryName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PricingMatrixJson = table.Column<string>(type: "jsonb", nullable: true),
                    CommissionMatrixJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseServiceTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseServiceTemplates_Tenants_FranchisorTenantId",
                        column: x => x.FranchisorTenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FranchiseSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RoyaltyRate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    MarketingFeeRate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    TechnologyFeeRate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    RoyaltyBasis = table.Column<int>(type: "integer", nullable: false),
                    RoyaltyFrequency = table.Column<int>(type: "integer", nullable: false),
                    EnforceStandardServices = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EnforceStandardPricing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllowLocalServices = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MaxPriceVariance = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    EnforceBranding = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DefaultFranchiseePlan = table.Column<int>(type: "integer", nullable: false),
                    MaxBranchesPerFranchisee = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoyaltyPeriods",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    FranchisorTenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FranchiseeTenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AgreementId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrossRevenue = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    RoyaltyRate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    RoyaltyAmount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    MarketingFeeRate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    MarketingFeeAmount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    TechnologyFeeRate = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    TechnologyFeeAmount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    TotalDue = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoyaltyPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoyaltyPeriods_FranchiseAgreements_AgreementId",
                        column: x => x.AgreementId,
                        principalTable: "FranchiseAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoyaltyPeriods_Tenants_FranchiseeTenantId",
                        column: x => x.FranchiseeTenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoyaltyPeriods_Tenants_FranchisorTenantId",
                        column: x => x.FranchisorTenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_FranchiseCode",
                table: "Tenants",
                column: "FranchiseCode",
                unique: true,
                filter: "\"FranchiseCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ParentTenantId",
                table: "Tenants",
                column: "ParentTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseAgreements_FranchiseeTenantId",
                table: "FranchiseAgreements",
                column: "FranchiseeTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseAgreements_FranchisorTenantId_FranchiseeTenantId",
                table: "FranchiseAgreements",
                columns: new[] { "FranchisorTenantId", "FranchiseeTenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseInvitations_FranchisorTenantId",
                table: "FranchiseInvitations",
                column: "FranchisorTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseInvitations_Token",
                table: "FranchiseInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseServiceTemplates_FranchisorTenantId",
                table: "FranchiseServiceTemplates",
                column: "FranchisorTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseSettings_TenantId",
                table: "FranchiseSettings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoyaltyPeriods_AgreementId",
                table: "RoyaltyPeriods",
                column: "AgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_RoyaltyPeriods_FranchiseeTenantId_PeriodStart_PeriodEnd",
                table: "RoyaltyPeriods",
                columns: new[] { "FranchiseeTenantId", "PeriodStart", "PeriodEnd" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoyaltyPeriods_FranchisorTenantId",
                table: "RoyaltyPeriods",
                column: "FranchisorTenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Tenants_ParentTenantId",
                table: "Tenants",
                column: "ParentTenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Tenants_ParentTenantId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "FranchiseInvitations");

            migrationBuilder.DropTable(
                name: "FranchiseServiceTemplates");

            migrationBuilder.DropTable(
                name: "FranchiseSettings");

            migrationBuilder.DropTable(
                name: "RoyaltyPeriods");

            migrationBuilder.DropTable(
                name: "FranchiseAgreements");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_FranchiseCode",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_ParentTenantId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BusinessPermitNo",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "FranchiseCode",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ParentTenantId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TenantType",
                table: "Tenants");
        }
    }
}
