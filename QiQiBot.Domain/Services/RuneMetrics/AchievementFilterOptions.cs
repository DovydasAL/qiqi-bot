namespace QiQiBot.Services.RuneMetrics;

public sealed class AchievementFilterOptions
{
    public List<string> TextPatterns { get; set; } = [];
    public List<string> DetailPatterns { get; set; } = [];

    public static AchievementFilterOptions CreateDefault() => new()
    {
        TextPatterns =
        [
            @".*(?!200000000(?:\D|$))\d+XP.*",
            @".*songs unlocked.*",
            @".*Quest complete.*",
            @".*Clan Fealty.*",
            @".*Visited my Clan Citadel.*",
            @".*capped at my clan citadel.*",
            @".*crystal triskelion fragment.*",
            @".*abyssal whip.*",
            @".*dragon helm.*",
            @".*shield left half.*",
            @".*dragon boots.*",
            @".*dragon hatchet.*",
            @".*dragon platelegs.*",
            @".*ancient effigy.*",
            @".*looted a book.*",
            @".*archaeological mystery.*",
            @".*songs unlocked.*",
            @".*charm sprites.*",
            @".*killed.*",
            @".*defeated.*",
            @".*dungeon floor \d+.*",
            @".*granite maul.*",
            @".*godsword shard.*",
            @".*jaws of the abyss.*",
            @".*demon slayer.*",
            @".*tetracompass.*",
            @".*fight kiln.*",
            @".*amulet of ranging.*",
            @".*bandos (?:helmet|chestplate|tassets|gloves|boots|warshield|hilt).*",
            @".*armadyl (?:helmet|chestplate|chainskirt|gloves|boots|buckler|crossbow|hilt).*",
            @".*saradomin (?:hilt|sword).*",
            @".*(?:whisper|murmur|hiss) of saradomin.*",
            @".*(?:zamorak hilt|zamorakian spear).*",
            @".*(?:hood|garb|gown|gloves|boots|ward) of subjugation.*",
            @".*silver spine.*",
            @".*sanguine spine.*",
        ],
        DetailPatterns =
        [
            @".*am now level (?!99\b|110\b|120\b)\d+.*",
            @".*at least level (?!(10|20|30|40|50|60|70|80|90|99|110|120)\b)\d+ in all skills.*",
            @".*QP milestone.*",
        ]
    };
}
