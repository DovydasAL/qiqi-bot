using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QiQiBot.Migrations
{
    /// <inheritdoc />
    public partial class GuildUserRsns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guild_user_rsns",
                schema: "qiqi",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    rsn = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_user_rsns", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guild_user_rsns_guild_id_user_id",
                schema: "qiqi",
                table: "guild_user_rsns",
                columns: new[] { "guild_id", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guild_user_rsns",
                schema: "qiqi");
        }
    }
}
