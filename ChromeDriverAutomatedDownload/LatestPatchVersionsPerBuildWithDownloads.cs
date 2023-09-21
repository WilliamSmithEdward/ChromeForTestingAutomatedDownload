using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class LatestPatchVersionsPerBuildWithDownloads
    {
        public class ChromeVersionModel : IChromeVersionModel
        {
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLatestPatchVersionsPerBuildAsyncWithDownloads;

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

            [JsonPropertyName("downloads")]
            public DownloadMetaData Downloads { get; set; } = new DownloadMetaData();
        }

        public class DownloadMetaData
        {
            [JsonPropertyName("chrome")]
            public List<PlatformMetaData> Chrome { get; set; } = new List<PlatformMetaData>();

            [JsonPropertyName("chromedriver")]
            public List<PlatformMetaData> ChromeDriver { get; set; } = new List<PlatformMetaData>();

            [JsonPropertyName("chrome-headless-shell")]
            public List<PlatformMetaData> ChromeHeadlessShell { get; set; } = new List<PlatformMetaData>();
        }

        public class PlatformMetaData
        {
            [JsonPropertyName("platform")]
            public string Platform { get; set; } = string.Empty;

            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }
    }
}

