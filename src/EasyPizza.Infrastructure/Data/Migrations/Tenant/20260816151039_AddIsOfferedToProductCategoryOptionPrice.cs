using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddIsOfferedToProductCategoryOptionPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalPrice",
                table: "ProductCategoryOptionPrices",
                type: "numeric(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            // Toda linha existente representava, no modelo antigo, um produto que OFERECE aquele
            // item (a ausência de linha é que significava "não oferece") — então o padrão pras
            // linhas já existentes tem que ser true, senão o catálogo inteiro fica sem opções.
            migrationBuilder.AddColumn<bool>(
                name: "IsOffered",
                table: "ProductCategoryOptionPrices",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "533dbd27-52d0-48b4-bc40-1df8ddf01639");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOffered",
                table: "ProductCategoryOptionPrices");

            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalPrice",
                table: "ProductCategoryOptionPrices",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "f935848d-b7f7-4a7b-8e75-36d79da69802");
        }
    }
}
