using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QiQiBot.Models
{
    [Index(nameof(GuildId), IsUnique = true)]
    [Table("guilds", Schema = "qiqi")]
    public class Guild
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("guild_id")]
        public ulong GuildId { get; set; }

        [Column("achievements_channel_id")]
        public ulong? AchievementsChannelId { get; set; }

        [Column("cap_reset_day")]
        public long? CapResetDay { get; set; }

        [Column("cap_reset_time")]
        public string? CapResetTime { get; set; }

        [Column("clan_id")]
        public long? ClanId { get; set; }
        public Clan? Clan { get; set; }
    }
}
