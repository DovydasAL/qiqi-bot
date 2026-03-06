using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QiQiBot.Models;

[Table("guild_user_rsns", Schema = "qiqi")]
[Index(nameof(GuildId), nameof(UserId), IsUnique = true)]
public class GuildUserRsn
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("guild_id")]
    public ulong GuildId { get; set; }

    [Required]
    [Column("user_id")]
    public ulong UserId { get; set; }

    [Required]
    [Column("rsn")]
    public string RuneScapeName { get; set; } = string.Empty;
}
