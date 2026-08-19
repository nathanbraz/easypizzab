using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class DropPaymentGatewaySandboxMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentGatewaySandboxMode",
                table: "StoreSettings");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "cb78e7fb-2ee7-4110-bedd-def1db26bd77");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PaymentGatewaySandboxMode",
                table: "StoreSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "04950f33-30ed-4f61-97e6-050226d56e48");
        }
    }
}
