using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>Model of latest-versions-per-milestone.json. The type to deserialize is the nested <see cref="ChromeVersionModel"/>.</summary>
    public class LatestVersionsPerMilestone
    {
        /// <summary>The content of latest-versions-per-milestone.json. Create it with <see cref="ChromeVersionModelFactory"/>.</summary>
        public class ChromeVersionModel : IChromeVersionModel
        {
            /// <summary>Reads latest-versions-per-milestone.json. <see cref="ChromeVersionModelFactory"/> uses the default value of a new instance, so setting this does not change what the factory reads.</summary>
            [JsonIgnore]
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLatestVersionsPerMilestoneAsync;

            /// <summary>When the endpoint was generated, in UTC.</summary>
            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            /// <summary>The latest version per milestone, keyed by milestone such as 120.</summary>
            [JsonPropertyName("milestones")]
            public Dictionary <string, Milestones> Milestones { get; set; } = new Dictionary<string, Milestones>();
        }

        /// <summary>The latest version of one milestone.</summary>
        public class Milestones
        {
            /// <summary>The milestone number, such as 120. The JSON property is milestone.</summary>
            [JsonPropertyName("milestone")]
            public string Channel { get; set; } = string.Empty;

            /// <summary>The full version, such as 120.0.6099.109.</summary>
            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            /// <summary>The Chromium revision.</summary>
            [JsonPropertyName("revision")]
            public string Revision { get; set; } = string.Empty;
        }
    }
}
