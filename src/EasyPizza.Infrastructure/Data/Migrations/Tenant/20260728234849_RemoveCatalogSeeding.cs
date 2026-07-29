using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RemoveCatalogSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("18f3a382-3d84-46b2-a4f6-8c4d28d0b8c4"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("8ebde348-18e3-4c07-b358-fc24d1eb4df4"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a0f7c1d3-3b12-4c28-98e3-f61b0c034298"));

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("41b711ea-eb8d-4ab0-b5cc-44b2f676451e"));

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("d866a152-4467-4d7a-8f4b-bfb6df7d6b38"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "CreatedAt", "DisplayOrder", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("41b711ea-eb8d-4ab0-b5cc-44b2f676451e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Bebidas", null },
                    { new Guid("d866a152-4467-4d7a-8f4b-bfb6df7d6b38"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Pizzas Tradicionais", null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrls", "IsAvailable", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("18f3a382-3d84-46b2-a4f6-8c4d28d0b8c4"), new Guid("41b711ea-eb8d-4ab0-b5cc-44b2f676451e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Refrigerante 2 Litros", new List<string>(), true, "Coca-Cola 2L", 14.00m, null },
                    { new Guid("8ebde348-18e3-4c07-b358-fc24d1eb4df4"), new Guid("d866a152-4467-4d7a-8f4b-bfb6df7d6b38"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Muçarela, calabresa e cebola", new List<string>(), true, "Calabresa", 49.90m, null },
                    { new Guid("a0f7c1d3-3b12-4c28-98e3-f61b0c034298"), new Guid("d866a152-4467-4d7a-8f4b-bfb6df7d6b38"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Muçarela, tomate e manjericão fresco", new List<string>(), true, "Marguerita", 45.00m, null }
                });
        }
    }
}
