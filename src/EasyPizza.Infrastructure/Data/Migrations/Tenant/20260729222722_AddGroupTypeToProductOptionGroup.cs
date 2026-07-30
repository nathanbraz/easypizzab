using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddGroupTypeToProductOptionGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupType",
                table: "ProductOptionGroups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupType",
                table: "ProductOptionGroups");
        }
    }
}
