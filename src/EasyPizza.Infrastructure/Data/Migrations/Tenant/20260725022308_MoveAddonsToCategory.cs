using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class MoveAddonsToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemAddons_ProductAddons_ProductAddonId",
                table: "OrderItemAddons");

            migrationBuilder.DropTable(
                name: "ProductAddons");

            migrationBuilder.RenameColumn(
                name: "ProductAddonId",
                table: "OrderItemAddons",
                newName: "CategoryAddonId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemAddons_ProductAddonId",
                table: "OrderItemAddons",
                newName: "IX_OrderItemAddons_CategoryAddonId");

            migrationBuilder.CreateTable(
                name: "CategoryAddons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryAddons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryAddons_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("18f3a382-3d84-46b2-a4f6-8c4d28d0b8c4"),
                column: "ImageUrls",
                value: new List<string>());

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("8ebde348-18e3-4c07-b358-fc24d1eb4df4"),
                column: "ImageUrls",
                value: new List<string>());

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a0f7c1d3-3b12-4c28-98e3-f61b0c034298"),
                column: "ImageUrls",
                value: new List<string>());

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAddons_CategoryId",
                table: "CategoryAddons",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemAddons_CategoryAddons_CategoryAddonId",
                table: "OrderItemAddons",
                column: "CategoryAddonId",
                principalTable: "CategoryAddons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemAddons_CategoryAddons_CategoryAddonId",
                table: "OrderItemAddons");

            migrationBuilder.DropTable(
                name: "CategoryAddons");

            migrationBuilder.RenameColumn(
                name: "CategoryAddonId",
                table: "OrderItemAddons",
                newName: "ProductAddonId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItemAddons_CategoryAddonId",
                table: "OrderItemAddons",
                newName: "IX_OrderItemAddons_ProductAddonId");

            migrationBuilder.CreateTable(
                name: "ProductAddons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAddons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAddons_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("18f3a382-3d84-46b2-a4f6-8c4d28d0b8c4"),
                column: "ImageUrls",
                value: new List<string>());

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("8ebde348-18e3-4c07-b358-fc24d1eb4df4"),
                column: "ImageUrls",
                value: new List<string>());

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a0f7c1d3-3b12-4c28-98e3-f61b0c034298"),
                column: "ImageUrls",
                value: new List<string>());

            migrationBuilder.CreateIndex(
                name: "IX_ProductAddons_ProductId",
                table: "ProductAddons",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemAddons_ProductAddons_ProductAddonId",
                table: "OrderItemAddons",
                column: "ProductAddonId",
                principalTable: "ProductAddons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
