using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddCrossSellDiscountPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CrossSellDiscountPrice",
                table: "Products",
                type: "numeric",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "5336cb37-639a-4daa-836c-96d967e676e1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrossSellDiscountPrice",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "cb78e7fb-2ee7-4110-bedd-def1db26bd77");
        }
    }
}
