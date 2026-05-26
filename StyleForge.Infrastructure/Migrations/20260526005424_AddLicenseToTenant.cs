using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseExpiresAt",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseExpiresAt",
                table: "Tenants");
        }
    }
}
