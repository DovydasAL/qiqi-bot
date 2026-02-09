using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QiQiBot.Models
{
    [Index(nameof(GuildId), IsUnique = true)]
    [Table("clans", Schema = "qiqi")]
    public class Clan
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("guild_id")]
        public ulong GuildId { get; set; }

        [Column("last_scraped")]
        public DateTime? LastScraped { get; set; }

        public virtual ICollection<ClanMember> ClanMembers { get; } = new List<ClanMember>();
    }
}
