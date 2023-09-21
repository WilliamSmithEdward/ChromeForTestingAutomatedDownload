using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class LatestVersionsPerMilestone
    {
        public class ChromeVersionModel : IChromeVersionModel
        {
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLatestVersionsPerMilestoneAsync;

            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            [JsonPropertyName("milestones")]
            public Dictionary <string, Milestones> Milestones { get; set; } = new Dictionary<string, Milestones>();
        }

        public class Milestones
        {
            [JsonPropertyName("milestone")]
            public string Channel { get; set; } = string.Empty;

            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            [JsonPropertyName("revision")]
            public string Revision { get; set; } = string.Empty;
        }
    }
}
