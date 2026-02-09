using Microsoft.EntityFrameworkCore;

namespace QiQiBot.Models
{
    public class ClanContext : DbContext
    {
        public DbSet<ClanMember> ClanMembers { get; set; }
        public DbSet<Clan> Clans { get; set; }

        public ClanContext(DbContextOptions<ClanContext> options) : base(options) { }


    }
}
