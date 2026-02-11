using System.Text.Json.Serialization;

namespace QiQiBot.Models
{
    public class RuneMetricsProfileDTO
    {
        [JsonIgnore]
        public DateTime ScrapedDate { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("magic")]
        public long Magic { get; set; }

        [JsonPropertyName("questsstarted")]
        public long QuestsStarted { get; set; }

        [JsonPropertyName("totalskill")]
        public long TotalSkill { get; set; }

        [JsonPropertyName("questscomplete")]
        public long QuestsComplete { get; set; }

        [JsonPropertyName("questsnotstarted")]
        public long QuestsNotStarted { get; set; }

        [JsonPropertyName("totalxp")]
        public long TotalExp { get; set; }

        [JsonPropertyName("ranged")]
        public long Ranged { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("rank")]
        public string? Rank { get; set; }

        [JsonPropertyName("melee")]
        public long Melee { get; set; }

        [JsonPropertyName("combatlevel")]
        public long CombatLevel { get; set; }

        [JsonPropertyName("loggedIn")]
        public string? LoggedIn { get; set; }

        [JsonPropertyName("activities")]
        public List<RuneMetricsActivityDTO> Activities { get; set; } = [];

        [JsonPropertyName("skillvalues")]
        public List<RuneMetricsSkillValueDTO> SkillValues { get; set; } = [];

        public class RuneMetricsActivityDTO
        {
            [JsonPropertyName("date")]
            public string Date { get; set; } = null!;

            [JsonPropertyName("details")]
            public string? Details { get; set; }

            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }

        public class RuneMetricsSkillValueDTO
        {
            [JsonPropertyName("level")]
            public long Level { get; set; }

            [JsonPropertyName("xp")]
            public long Exp { get; set; }

            [JsonPropertyName("rank")]
            public long Rank { get; set; }

            [JsonPropertyName("id")]
            public long Id { get; set; }
        }
    }
}
