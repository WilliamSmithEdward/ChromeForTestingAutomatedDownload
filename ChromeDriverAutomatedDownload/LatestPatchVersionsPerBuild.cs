using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class LatestPatchVersionsPerBuild
    {
        public class ChromeVersionModel : IChromeVersionModel
        {
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLatestPatchVersionsPerBuildAsync;

            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            [JsonPropertyName("builds")]
            public Dictionary<string, Build> Builds { get; set; } = new Dictionary<string, Build>();
        }

        public class Build
        {
            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            [JsonPropertyName("revision")]
            public string Revision { get; set; } = string.Empty;
        }
    }
}

