using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class GuildClanChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clans_guild_id",
                schema: "qiqi",
                table: "clans");

            migrationBuilder.DropColumn(
                name: "guild_id",
                schema: "qiqi",
                table: "clans");

            migrationBuilder.CreateTable(
                name: "guilds",
                schema: "qiqi",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    achievements_channel_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    clan_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guilds", x => x.id);
                    table.ForeignKey(
                        name: "FK_guilds_clans_clan_id",
                        column: x => x.clan_id,
                        principalSchema: "qiqi",
                        principalTable: "clans",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_guilds_clan_id",
                schema: "qiqi",
                table: "guilds",
                column: "clan_id");

            migrationBuilder.CreateIndex(
                name: "IX_guilds_guild_id",
                schema: "qiqi",
                table: "guilds",
                column: "guild_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guilds",
                schema: "qiqi");

            migrationBuilder.AddColumn<decimal>(
                name: "guild_id",
                schema: "qiqi",
                table: "clans",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_clans_guild_id",
                schema: "qiqi",
                table: "clans",
                column: "guild_id",
                unique: true);
        }
    }
}
