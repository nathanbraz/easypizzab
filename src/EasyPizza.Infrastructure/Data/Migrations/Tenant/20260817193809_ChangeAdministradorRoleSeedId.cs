using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyPizza.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class ChangeAdministradorRoleSeedId : Migration
    {
        private const string OldId = "11111111-1111-1111-1111-111111111111";
        private const string NewId = "866de267-9cb3-4f41-8b6d-b814038798b5";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O EF gera por padrão um DeleteData+InsertData pra trocar a PK da role — isso
            // dispararia o ON DELETE CASCADE das FKs de "RoleClaims" e "UserRoles", apagando
            // as permissões e os vínculos de usuário de toda pizzaria que já tem essa role.
            // Em vez disso: solta as FKs, atualiza os 3 valores em UPDATE (preservando as
            // linhas) e recria as FKs exatamente como estavam.
            migrationBuilder.DropForeignKey(name: "FK_RoleClaims_Roles_RoleId", table: "RoleClaims");
            migrationBuilder.DropForeignKey(name: "FK_UserRoles_Roles_RoleId", table: "UserRoles");

            migrationBuilder.Sql($@"UPDATE ""RoleClaims"" SET ""RoleId"" = '{NewId}' WHERE ""RoleId"" = '{OldId}';");
            migrationBuilder.Sql($@"UPDATE ""UserRoles"" SET ""RoleId"" = '{NewId}' WHERE ""RoleId"" = '{OldId}';");
            migrationBuilder.Sql($@"UPDATE ""Roles"" SET ""Id"" = '{NewId}' WHERE ""Id"" = '{OldId}';");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleClaims_Roles_RoleId",
                table: "RoleClaims",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_RoleClaims_Roles_RoleId", table: "RoleClaims");
            migrationBuilder.DropForeignKey(name: "FK_UserRoles_Roles_RoleId", table: "UserRoles");

            migrationBuilder.Sql($@"UPDATE ""RoleClaims"" SET ""RoleId"" = '{OldId}' WHERE ""RoleId"" = '{NewId}';");
            migrationBuilder.Sql($@"UPDATE ""UserRoles"" SET ""RoleId"" = '{OldId}' WHERE ""RoleId"" = '{NewId}';");
            migrationBuilder.Sql($@"UPDATE ""Roles"" SET ""Id"" = '{OldId}' WHERE ""Id"" = '{NewId}';");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleClaims_Roles_RoleId",
                table: "RoleClaims",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
