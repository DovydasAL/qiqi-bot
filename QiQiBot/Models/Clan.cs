using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QiQiBot.Models
{
    [Table("clans", Schema = "qiqi")]
    public class Clan
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("last_scraped")]
        public DateTime? LastScraped { get; set; }

        public virtual ICollection<Player> ClanMembers { get; } = new List<Player>();
        public virtual ICollection<Guild> Guilds { get; } = new List<Guild>();

    }
}
