using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiQiBot.Models
{
    [Index(nameof(GuildId), IsUnique = true)]
    public class Clan
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public ulong GuildId { get; set; }

        public virtual ICollection<ClanMember> ClanMembers { get; } = new List<ClanMember>();
    }
}
