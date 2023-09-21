using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class KnownGoodVersionsWithDownload
    {
        public class ChromeVersionModel : IChromeVersionModel
        {
            public Func<Task<string>> QueryEndpoint { get; set; } = GoogleChromeLabsEndpointQueries.GetKnownGoodVersionsWithDownloadAsync;

            [JsonPropertyName("versions")]
            public List<VersionMetaData> Versions { get; set; } = new List<VersionMetaData>();

            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }
        }

        public class VersionMetaData
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
