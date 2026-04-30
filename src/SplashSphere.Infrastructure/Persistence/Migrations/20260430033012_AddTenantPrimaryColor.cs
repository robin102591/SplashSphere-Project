using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantPrimaryColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryColorHex",
                table: "Tenants",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryColorHex",
                table: "Tenants");
        }
    }
}
