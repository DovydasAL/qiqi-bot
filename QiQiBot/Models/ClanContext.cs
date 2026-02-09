using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiQiBot.Models
{
    public class ClanContext : DbContext
    {
        public DbSet<ClanMember> ClanMembers { get; set; }
        public DbSet<Clan> Clans { get; set; }

        public ClanContext(DbContextOptions<ClanContext> options) : base(options) { }


    }
}
