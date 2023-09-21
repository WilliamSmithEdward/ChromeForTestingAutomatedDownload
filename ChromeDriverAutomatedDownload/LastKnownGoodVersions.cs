using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class LastKnownGoodVersions
    {
        public class ChromeVersionModel : IChromeVersionModel
        {
            public Func<Task<string>> QueryEndpoint { get; set; } = GoogleChromeLabsEndpointQueries.GetLastKnownGoodVersionsAsync;

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
        }
    }
}
