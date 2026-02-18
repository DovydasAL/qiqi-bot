using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class CapStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_capped",
                schema: "qiqi",
                table: "players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "cap_reset_day",
                schema: "qiqi",
                table: "guilds",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "cap_reset_time",
                schema: "qiqi",
                table: "guilds",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_capped",
                schema: "qiqi",
                table: "players");

            migrationBuilder.DropColumn(
                name: "cap_reset_day",
                schema: "qiqi",
                table: "guilds");

            migrationBuilder.DropColumn(
                name: "cap_reset_time",
                schema: "qiqi",
                table: "guilds");
        }
    }
}
