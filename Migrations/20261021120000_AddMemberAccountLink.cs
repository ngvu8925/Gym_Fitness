using GymManagement.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261021120000_AddMemberAccountLink")]
    public partial class AddMemberAccountLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Members",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_AccountId",
                table: "Members",
                column: "AccountId",
                unique: true,
                filter: "[AccountId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Accounts_AccountId",
                table: "Members",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Accounts_AccountId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_AccountId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Members");
        }
    }
}
