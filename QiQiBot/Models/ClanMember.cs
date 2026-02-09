using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace QiQiBot.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class ClanMember
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; } = null!;
        public long Experience { get; set; }
        public DateTime LastExperienceUpdate { get; set; }

        public long ClanId { get; set; }
        public Clan Clan { get; set; } = null!;
    }
}
