using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QiQiBot.Models
{
    [Index(nameof(Name), IsUnique = true)]
    [Index(nameof(LastScrapedRuneMetricsProfile))]
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

        [Column("last_scraped_runemetrics_profile")]
        public DateTime? LastScrapedRuneMetricsProfile { get; set; }

        [Column("most_recent_runemetrics_event")]
        public DateTime? MostRecentRuneMetricsEvent { get; set; }

        [Column("private_runemetrics_profile")]
        public bool PrivateRuneMetricsProfile { get; set; }

        [Column("invalid_runemetrics_profile")]
        public bool InvalidRuneMetricsProfile { get; set; }

        [Required]
        [Column("clan_id")]
        public long ClanId { get; set; }
        public Clan Clan { get; set; } = null!;
    }
}
