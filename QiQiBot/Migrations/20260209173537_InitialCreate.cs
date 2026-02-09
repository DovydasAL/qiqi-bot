using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "qiqi");

            migrationBuilder.CreateTable(
                name: "clans",
                schema: "qiqi",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    last_scraped = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clan_members",
                schema: "qiqi",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    clan_experience = table.Column<long>(type: "bigint", nullable: false),
                    last_clan_experience_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    clan_id = table.Column<long>(type: "bigint", nullable: false)
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
                name: "IX_clan_members_name",
                schema: "qiqi",
                table: "clan_members",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clans_guild_id",
                schema: "qiqi",
                table: "clans",
                column: "guild_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clan_members",
                schema: "qiqi");

            migrationBuilder.DropTable(
                name: "clans",
                schema: "qiqi");
        }
    }
}
