using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoyaltyRedemptionId",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PointsEarned",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LoyaltyProgramSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PointsPerCurrencyUnit = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CurrencyUnitAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PointsExpirationMonths = table.Column<int>(type: "integer", nullable: true),
                    AutoEnroll = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyProgramSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyRewards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    PointsCost = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    PackageId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyRewards_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoyaltyRewards_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoyaltyRewards_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembershipCards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    CardNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentTier = table.Column<int>(type: "integer", nullable: false),
                    PointsBalance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LifetimePointsEarned = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LifetimePointsRedeemed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipCards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipCards_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTierConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LoyaltyProgramSettingsId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinimumLifetimePoints = table.Column<int>(type: "integer", nullable: false),
                    PointsMultiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTierConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyTierConfigs_LoyaltyProgramSettings_LoyaltyProgramSet~",
                        column: x => x.LoyaltyProgramSettingsId,
                        principalTable: "LoyaltyProgramSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    TenantId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MembershipCardId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    BalanceAfter = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    RewardId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointTransactions_LoyaltyRewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "LoyaltyRewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PointTransactions_MembershipCards_MembershipCardId",
                        column: x => x.MembershipCardId,
                        principalTable: "MembershipCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PointTransactions_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramSettings_TenantId",
                table: "LoyaltyProgramSettings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_PackageId",
                table: "LoyaltyRewards",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_ServiceId",
                table: "LoyaltyRewards",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_TenantId",
                table: "LoyaltyRewards",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTierConfigs_LoyaltyProgramSettingsId_Tier",
                table: "LoyaltyTierConfigs",
                columns: new[] { "LoyaltyProgramSettingsId", "Tier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipCards_CardNumber",
                table: "MembershipCards",
                column: "CardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipCards_CustomerId",
                table: "MembershipCards",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipCards_TenantId_CustomerId",
                table: "MembershipCards",
                columns: new[] { "TenantId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_MembershipCardId",
                table: "PointTransactions",
                column: "MembershipCardId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_MembershipCardId_CreatedAt",
                table: "PointTransactions",
                columns: new[] { "MembershipCardId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_RewardId",
                table: "PointTransactions",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_TransactionId",
                table: "PointTransactions",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyTierConfigs");

            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "LoyaltyProgramSettings");

            migrationBuilder.DropTable(
                name: "LoyaltyRewards");

            migrationBuilder.DropTable(
                name: "MembershipCards");

            migrationBuilder.DropColumn(
                name: "LoyaltyRedemptionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PointsEarned",
                table: "Transactions");
        }
    }
}
