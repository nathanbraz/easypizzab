using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RenameMercadoPagoToPaymentGateway : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O EF gerou isso ao contrário por padrão (queria renomear a coluna que guarda o
            // token em si para "PaymentGatewayProvider"). Corrigido à mão: o valor que já existe
            // é a credencial, então ele tem que ir para PaymentGatewayAccessToken — e o provider
            // é uma coluna nova, preenchida a seguir só para quem já tinha um token salvo (nesse
            // caso só pode ter sido o Mercado Pago, único gateway que já existiu).
            migrationBuilder.RenameColumn(
                name: "MercadoPagoAccessToken",
                table: "StoreSettings",
                newName: "PaymentGatewayAccessToken");

            migrationBuilder.AddColumn<string>(
                name: "PaymentGatewayProvider",
                table: "StoreSettings",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"StoreSettings\" SET \"PaymentGatewayProvider\" = 'MercadoPago' WHERE \"PaymentGatewayAccessToken\" IS NOT NULL;");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "083e5638-8147-417e-916e-ec299799d599");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentGatewayProvider",
                table: "StoreSettings");

            migrationBuilder.RenameColumn(
                name: "PaymentGatewayAccessToken",
                table: "StoreSettings",
                newName: "MercadoPagoAccessToken");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "01d7636d-b0d9-4d3b-9b12-202718cb9666");
        }
    }
}
