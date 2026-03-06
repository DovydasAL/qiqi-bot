using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class GuildDebugMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "debug_mode_enabled",
                schema: "qiqi",
                table: "guilds",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "debug_mode_enabled",
                schema: "qiqi",
                table: "guilds");
        }
    }
}
