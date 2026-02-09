using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
