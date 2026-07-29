using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddMultipleCategoriesToAddon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "CategoryIds",
                table: "CategoryAddons",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'");

            // Copia o dado antigo para o novo formato de array (Migração sem perda de dados)
            migrationBuilder.Sql("UPDATE \"CategoryAddons\" SET \"CategoryIds\" = ARRAY[\"CategoryId\"] WHERE \"CategoryId\" IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryAddons_ProductCategories_CategoryId",
                table: "CategoryAddons");

            migrationBuilder.DropIndex(
                name: "IX_CategoryAddons_CategoryId",
                table: "CategoryAddons");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CategoryAddons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryIds",
                table: "CategoryAddons");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "CategoryAddons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CategoryAddons_CategoryId",
                table: "CategoryAddons",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryAddons_ProductCategories_CategoryId",
                table: "CategoryAddons",
                column: "CategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
