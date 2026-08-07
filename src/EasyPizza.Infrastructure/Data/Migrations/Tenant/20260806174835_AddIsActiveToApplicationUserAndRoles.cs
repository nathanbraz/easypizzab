using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddIsActiveToApplicationUserAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "17dd542a-ac03-4049-b287-aa4ad190079b", "Owner", "OWNER" });

            migrationBuilder.InsertData(
                table: "RoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "Permission", "Orders:View", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 2, "Permission", "Orders:Edit", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 3, "Permission", "Catalog:Manage", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 4, "Permission", "Settings:Manage", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 5, "Permission", "Coupons:Manage", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 6, "Permission", "Couriers:Manage", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 7, "Permission", "Team:View", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 8, "Permission", "Team:Create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 9, "Permission", "Team:Edit", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 10, "Permission", "Team:Block", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 11, "Permission", "Team:Delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 12, "Permission", "Roles:View", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 13, "Permission", "Roles:Create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 14, "Permission", "Roles:Edit", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 15, "Permission", "Roles:Delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 16, "Permission", "Customers:View", new Guid("11111111-1111-1111-1111-111111111111") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");
        }
    }
}
