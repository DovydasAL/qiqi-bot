using Microsoft.EntityFrameworkCore;

namespace QiQiBot.Models
{
    public class ClanContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Clan> Clans { get; set; }
        public DbSet<Guild> Guilds { get; set; }

        public ClanContext(DbContextOptions<ClanContext> options) : base(options) { }


    }
}
