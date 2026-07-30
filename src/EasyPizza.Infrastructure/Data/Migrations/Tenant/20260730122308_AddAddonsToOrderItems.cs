using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAddonsToOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemAddons_CategoryAddons_CategoryAddonId",
                table: "OrderItemAddons");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryAddonId",
                table: "OrderItemAddons",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductOptionItemId",
                table: "OrderItemAddons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderItemAddons",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemAddons_CategoryAddons_CategoryAddonId",
                table: "OrderItemAddons",
                column: "CategoryAddonId",
                principalTable: "CategoryAddons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItemAddons_CategoryAddons_CategoryAddonId",
                table: "OrderItemAddons");

            migrationBuilder.DropColumn(
                name: "ProductOptionItemId",
                table: "OrderItemAddons");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderItemAddons");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryAddonId",
                table: "OrderItemAddons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItemAddons_CategoryAddons_CategoryAddonId",
                table: "OrderItemAddons",
                column: "CategoryAddonId",
                principalTable: "CategoryAddons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
