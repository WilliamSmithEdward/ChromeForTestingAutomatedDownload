using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class LastKnownGoodVersionsWithDownloads
    {
        public class ChromeVersionModel : IChromeVersionModel
        {
            public Func<Task<string>> QueryEndpoint { get; set; } = GoogleChromeLabsEndpointQueries.GetLastKnownGoodVersionsWithDownloadAsync;

            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            [JsonPropertyName("channels")]
            public Channels Channels { get; set; } = new Channels();
        }

        public class Channels
        {
            [JsonPropertyName("Stable")]
            public ChannelMetaData Stable { get; set; } = new ChannelMetaData();

            [JsonPropertyName("Beta")]
            public ChannelMetaData Beta { get; set; } = new ChannelMetaData();

            [JsonPropertyName("Dev")]
            public ChannelMetaData Dev { get; set; } = new ChannelMetaData();

            [JsonPropertyName("Canary")]
            public ChannelMetaData Canary { get; set; } = new ChannelMetaData();
        }

        public class ChannelMetaData
        {
            [JsonPropertyName("channel")]
            public string Channel { get; set; } = string.Empty;

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
