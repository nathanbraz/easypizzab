using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class DropLegacyCategoryAddonColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryAddonId",
                table: "OrderItemAddons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryAddonId",
                table: "OrderItemAddons",
                type: "uuid",
                nullable: true);
        }
    }
}
