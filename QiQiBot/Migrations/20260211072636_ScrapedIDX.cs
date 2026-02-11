using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class ScrapedIDX : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_clan_members_last_scraped_runemetrics_profile",
                schema: "qiqi",
                table: "clan_members",
                column: "last_scraped_runemetrics_profile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clan_members_last_scraped_runemetrics_profile",
                schema: "qiqi",
                table: "clan_members");
        }
    }
}
