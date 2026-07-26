using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddWhatsappSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsappApiKey",
                table: "StoreSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsappBotEnabled",
                table: "StoreSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappGreetingMessage",
                table: "StoreSettings",
                type: "text",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "WhatsappSupportPhone",
                table: "StoreSettings",
                type: "text",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsappApiKey",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "WhatsappBotEnabled",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "WhatsappGreetingMessage",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "WhatsappInstanceName",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "WhatsappServerUrl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "WhatsappSupportPhone",
                table: "StoreSettings");

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
        }
    }
}
