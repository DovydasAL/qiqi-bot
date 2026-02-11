using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class OptionalClanId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clan_members",
                schema: "qiqi");

            migrationBuilder.CreateTable(
                name: "players",
                schema: "qiqi",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    clan_experience = table.Column<long>(type: "bigint", nullable: false),
                    last_clan_experience_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_scraped_runemetrics_profile = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    most_recent_runemetrics_event = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    private_runemetrics_profile = table.Column<bool>(type: "boolean", nullable: false),
                    invalid_runemetrics_profile = table.Column<bool>(type: "boolean", nullable: false),
                    clan_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                    table.ForeignKey(
                        name: "FK_players_clans_clan_id",
                        column: x => x.clan_id,
                        principalSchema: "qiqi",
                        principalTable: "clans",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_players_clan_id",
                schema: "qiqi",
                table: "players",
                column: "clan_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_last_scraped_runemetrics_profile",
                schema: "qiqi",
                table: "players",
                column: "last_scraped_runemetrics_profile");

            migrationBuilder.CreateIndex(
                name: "IX_players_name",
                schema: "qiqi",
                table: "players",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "players",
                schema: "qiqi");

            migrationBuilder.CreateTable(
                name: "clan_members",
                schema: "qiqi",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    clan_id = table.Column<long>(type: "bigint", nullable: false),
                    clan_experience = table.Column<long>(type: "bigint", nullable: false),
                    invalid_runemetrics_profile = table.Column<bool>(type: "boolean", nullable: false),
                    last_clan_experience_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_scraped_runemetrics_profile = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    most_recent_runemetrics_event = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    private_runemetrics_profile = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clan_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_clan_members_clans_clan_id",
                        column: x => x.clan_id,
                        principalSchema: "qiqi",
                        principalTable: "clans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clan_members_clan_id",
                schema: "qiqi",
                table: "clan_members",
                column: "clan_id");

            migrationBuilder.CreateIndex(
                name: "IX_clan_members_last_scraped_runemetrics_profile",
                schema: "qiqi",
                table: "clan_members",
                column: "last_scraped_runemetrics_profile");

            migrationBuilder.CreateIndex(
                name: "IX_clan_members_name",
                schema: "qiqi",
                table: "clan_members",
                column: "name",
                unique: true);
        }
    }
}
