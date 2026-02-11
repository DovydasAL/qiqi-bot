using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class LastScrapedRSS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_scraped_rss_feed",
                schema: "qiqi",
                table: "clan_members",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_scraped_rss_feed",
                schema: "qiqi",
                table: "clan_members");
        }
    }
}
