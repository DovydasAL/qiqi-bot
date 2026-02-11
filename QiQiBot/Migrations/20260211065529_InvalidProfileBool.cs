using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class InvalidProfileBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_scraped_rune_metrics_profile",
                schema: "qiqi",
                table: "clan_members",
                newName: "last_scraped_runemetrics_profile");

            migrationBuilder.RenameColumn(
                name: "PrivateRuneMetrics",
                schema: "qiqi",
                table: "clan_members",
                newName: "private_runemetrics_profile");

            migrationBuilder.AddColumn<bool>(
                name: "invalid_runemetrics_profile",
                schema: "qiqi",
                table: "clan_members",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invalid_runemetrics_profile",
                schema: "qiqi",
                table: "clan_members");

            migrationBuilder.RenameColumn(
                name: "last_scraped_runemetrics_profile",
                schema: "qiqi",
                table: "clan_members",
                newName: "last_scraped_rune_metrics_profile");

            migrationBuilder.RenameColumn(
                name: "private_runemetrics_profile",
                schema: "qiqi",
                table: "clan_members",
                newName: "PrivateRuneMetrics");
        }
    }
}
