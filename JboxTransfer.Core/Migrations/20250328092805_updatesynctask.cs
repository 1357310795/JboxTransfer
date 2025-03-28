using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JboxTransfer.Core.Migrations
{
    /// <inheritdoc />
    public partial class updatesynctask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChunkFactor",
                table: "SyncTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ErrorCause",
                table: "SyncTasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "None");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChunkFactor",
                table: "SyncTasks");

            migrationBuilder.DropColumn(
                name: "ErrorCause",
                table: "SyncTasks");
        }
    }
}
