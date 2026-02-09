using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
