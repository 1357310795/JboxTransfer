using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JboxTransfer.Core.Migrations
{
    /// <inheritdoc />
    public partial class addextradata2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStats_Users_UserId",
                table: "UserStats");

            migrationBuilder.DropIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_StatId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StatId",
                table: "Users",
                column: "StatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserStats_StatId",
                table: "Users",
                column: "StatId",
                principalTable: "UserStats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserStats_Users_UserId",
                table: "UserStats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserStats_StatId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_UserStats_Users_UserId",
                table: "UserStats");

            migrationBuilder.DropIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats");

            migrationBuilder.DropIndex(
                name: "IX_Users_StatId",
                table: "Users");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_StatId",
                table: "Users",
                column: "StatId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserStats_Users_UserId",
                table: "UserStats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "StatId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
