using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class UniqueNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Clans_GuildId",
                table: "Clans",
                column: "GuildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClanMembers_Name",
                table: "ClanMembers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clans_GuildId",
                table: "Clans");

            migrationBuilder.DropIndex(
                name: "IX_ClanMembers_Name",
                table: "ClanMembers");
        }
    }
}
