using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RemoveWhatsappServerUrlAndInstanceName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsappInstanceName",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "WhatsappServerUrl",
                table: "StoreSettings");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "3ac0143e-5bec-4636-8292-be69e4268277");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsappInstanceName",
                table: "StoreSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappServerUrl",
                table: "StoreSettings",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "709acf91-c7a1-47fe-ae17-50d6206fa2cb");
        }
    }
}
