using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddSharedCategoryOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoryOptionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MinChoices = table.Column<int>(type: "integer", nullable: false),
                    MaxChoices = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryOptionGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryOptionGroups_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryOptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryOptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryOptionItems_CategoryOptionGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "CategoryOptionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategoryOptionPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryOptionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdditionalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryOptionPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategoryOptionPrices_CategoryOptionItems_CategoryOpt~",
                        column: x => x.CategoryOptionItemId,
                        principalTable: "CategoryOptionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategoryOptionPrices_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "4d92e10c-a369-4ee1-8975-bcaaf364ab4b");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryOptionGroups_CategoryId",
                table: "CategoryOptionGroups",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryOptionItems_GroupId",
                table: "CategoryOptionItems",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryOptionPrices_CategoryOptionItemId",
                table: "ProductCategoryOptionPrices",
                column: "CategoryOptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryOptionPrices_ProductId_CategoryOptionItemId",
                table: "ProductCategoryOptionPrices",
                columns: new[] { "ProductId", "CategoryOptionItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategoryOptionPrices");

            migrationBuilder.DropTable(
                name: "CategoryOptionItems");

            migrationBuilder.DropTable(
                name: "CategoryOptionGroups");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "ConcurrencyStamp",
                value: "6306763e-b00c-4980-9ef4-8e73eb402514");
        }
    }
}
