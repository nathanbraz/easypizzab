using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class CleanupAndSecureOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemAddons_CategoryAddons_CategoryAddonId",
                table: "OrderItemAddons");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "CategoryAddons");

            migrationBuilder.DropIndex(
                name: "IX_OrderItemAddons_CategoryAddonId",
                table: "OrderItemAddons");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "OrderItems",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                type: "text",
                nullable: false,
                defaultValue: "");
                
            // Preenche o nome do produto nos pedidos antigos baseando-se no ProductId atual
            migrationBuilder.Sql("UPDATE \"OrderItems\" oi SET \"ProductName\" = p.\"Name\" FROM \"Products\" p WHERE oi.\"ProductId\" = p.\"Id\";");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CategoryAddons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CategoryIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryAddons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemAddons_CategoryAddonId",
                table: "OrderItemAddons",
                column: "CategoryAddonId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemAddons_CategoryAddons_CategoryAddonId",
                table: "OrderItemAddons",
                column: "CategoryAddonId",
                principalTable: "CategoryAddons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
