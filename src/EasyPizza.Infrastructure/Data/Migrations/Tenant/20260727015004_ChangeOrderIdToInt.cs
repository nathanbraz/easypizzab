using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class ChangeOrderIdToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"OrderItemAddons\";");
            migrationBuilder.Sql("DELETE FROM \"OrderItems\";");
            migrationBuilder.Sql("DELETE FROM \"Orders\";");
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" DROP CONSTRAINT IF EXISTS \"FK_OrderItems_Orders_OrderId\";");
            migrationBuilder.Sql("ALTER TABLE \"Orders\" ALTER COLUMN \"Id\" DROP DEFAULT;");
            migrationBuilder.Sql("ALTER TABLE \"Orders\" ALTER COLUMN \"Id\" TYPE integer USING 0;");
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" ALTER COLUMN \"OrderId\" TYPE integer USING 0;");
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" ADD CONSTRAINT \"FK_OrderItems_Orders_OrderId\" FOREIGN KEY (\"OrderId\") REFERENCES \"Orders\" (\"Id\") ON DELETE CASCADE;");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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
            migrationBuilder.Sql("DELETE FROM \"OrderItemAddons\";");
            migrationBuilder.Sql("DELETE FROM \"OrderItems\";");
            migrationBuilder.Sql("DELETE FROM \"Orders\";");
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" DROP CONSTRAINT IF EXISTS \"FK_OrderItems_Orders_OrderId\";");
            migrationBuilder.Sql("ALTER TABLE \"Orders\" ALTER COLUMN \"Id\" TYPE uuid USING '00000000-0000-0000-0000-000000000000'::uuid;");
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" ALTER COLUMN \"OrderId\" TYPE uuid USING '00000000-0000-0000-0000-000000000000'::uuid;");
            migrationBuilder.Sql("ALTER TABLE \"OrderItems\" ADD CONSTRAINT \"FK_OrderItems_Orders_OrderId\" FOREIGN KEY (\"OrderId\") REFERENCES \"Orders\" (\"Id\") ON DELETE CASCADE;");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

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
