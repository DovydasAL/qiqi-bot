using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class LastScrapedMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_scraped_rss_feed",
                schema: "qiqi",
                table: "clan_members",
                newName: "most_recent_runemetrics_event");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_scraped_rune_metrics_profile",
                schema: "qiqi",
                table: "clan_members",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_scraped_rune_metrics_profile",
                schema: "qiqi",
                table: "clan_members");

            migrationBuilder.RenameColumn(
                name: "most_recent_runemetrics_event",
                schema: "qiqi",
                table: "clan_members",
                newName: "last_scraped_rss_feed");
        }
    }
}
