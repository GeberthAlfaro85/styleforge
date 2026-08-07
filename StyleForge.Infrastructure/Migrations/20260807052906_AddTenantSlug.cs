using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Tenants");
        }
    }
}
