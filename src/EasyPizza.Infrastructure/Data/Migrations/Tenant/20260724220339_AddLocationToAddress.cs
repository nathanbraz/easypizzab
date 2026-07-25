using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddLocationToAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "CustomerAddresses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "CustomerAddresses",
                type: "double precision",
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
                name: "Latitude",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "CustomerAddresses");

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
