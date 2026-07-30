using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class FixGroupTypeDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Corrige grupos criados antes desta migration que têm GroupType vazio
            migrationBuilder.Sql(
                "UPDATE \"ProductOptionGroups\" SET \"GroupType\" = 'single' WHERE \"GroupType\" = '' OR \"GroupType\" IS NULL;"
            );

            // Define o valor padrão correto para novos registros
            migrationBuilder.AlterColumn<string>(
                name: "GroupType",
                table: "ProductOptionGroups",
                type: "text",
                nullable: false,
                defaultValue: "single",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GroupType",
                table: "ProductOptionGroups",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "single");
        }
    }
}
