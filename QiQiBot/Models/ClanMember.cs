using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QiQiBot.Models
{
    [Index(nameof(Name), IsUnique = true)]
    [Table("clan_members", Schema = "qiqi")]
    public class ClanMember
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("clan_experience")]
        public long ClanExperience { get; set; }

        [Column("last_clan_experience_update")]
        public DateTime? LastClanExperienceUpdate { get; set; }

        [Required]
        [Column("clan_id")]
        public long ClanId { get; set; }
        public Clan Clan { get; set; } = null!;
    }
}
