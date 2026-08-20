using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddFlavorGroupSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowsHalfAndHalf",
                table: "ProductCategories");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "CategoryOptionItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlavorPriceStrategy",
                table: "CategoryOptionGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlavorGroup",
                table: "CategoryOptionGroups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "30d81917-93a5-4054-9a86-c411e7eb948a");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryOptionItems_ProductId",
                table: "CategoryOptionItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryOptionItems_Products_ProductId",
                table: "CategoryOptionItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryOptionItems_Products_ProductId",
                table: "CategoryOptionItems");

            migrationBuilder.DropIndex(
                name: "IX_CategoryOptionItems_ProductId",
                table: "CategoryOptionItems");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "CategoryOptionItems");

            migrationBuilder.DropColumn(
                name: "FlavorPriceStrategy",
                table: "CategoryOptionGroups");

            migrationBuilder.DropColumn(
                name: "IsFlavorGroup",
                table: "CategoryOptionGroups");

            migrationBuilder.AddColumn<bool>(
                name: "AllowsHalfAndHalf",
                table: "ProductCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "5336cb37-639a-4daa-836c-96d967e676e1");
        }
    }
}
