using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JboxTransfer.Core.Migrations
{
    /// <inheritdoc />
    public partial class addextradata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Preference",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StatId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_StatId",
                table: "Users",
                column: "StatId");

            migrationBuilder.CreateTable(
                name: "UserStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTransferredBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    JboxSpaceUsedBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    OnlyFullTransfer = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "StatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserStats_UserId",
                table: "UserStats",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStats");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_StatId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Preference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StatId",
                table: "Users");
        }
    }
}
