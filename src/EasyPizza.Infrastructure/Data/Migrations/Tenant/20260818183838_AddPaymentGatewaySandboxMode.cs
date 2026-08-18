using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPaymentGatewaySandboxMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Default true (sandbox) — preserva o comportamento que já existia pra qualquer loja
            // configurada antes dessa coluna existir (todo mundo usava @testuser.com sem
            // distinção). A pizzatop10 (já em produção de verdade) precisa ser virada pra false
            // manualmente depois, pelo toggle na aba Pagamentos.
            migrationBuilder.AddColumn<bool>(
                name: "PaymentGatewaySandboxMode",
                table: "StoreSettings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "00e1acac-893c-4eab-ab96-8e7aa0d09aa9");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentGatewaySandboxMode",
                table: "StoreSettings");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("866de267-9cb3-4f41-8b6d-b814038798b5"),
                column: "ConcurrencyStamp",
                value: "3ac0143e-5bec-4636-8292-be69e4268277");
        }
    }
}
