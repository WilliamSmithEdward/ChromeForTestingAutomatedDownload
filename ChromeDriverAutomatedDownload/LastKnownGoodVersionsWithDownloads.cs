using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>Model of last-known-good-versions-with-downloads.json. The type to deserialize is the nested <see cref="ChromeVersionModel"/>.</summary>
    public class LastKnownGoodVersionsWithDownloads
    {
        /// <summary>The content of last-known-good-versions-with-downloads.json. Create it with <see cref="ChromeVersionModelFactory"/>.</summary>
        public class ChromeVersionModel : IChromeVersionModel
        {
            /// <summary>Reads last-known-good-versions-with-downloads.json. <see cref="ChromeVersionModelFactory"/> uses the default value of a new instance, so setting this does not change what the factory reads.</summary>
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLastKnownGoodVersionsWithDownloadAsync;

            /// <summary>When the endpoint was generated, in UTC.</summary>
            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            /// <summary>The release channels.</summary>
            [JsonPropertyName("channels")]
            public Channels Channels { get; set; } = new Channels();
        }

        /// <summary>The last known good version of each release channel.</summary>
        public class Channels
        {
            /// <summary>The Stable channel.</summary>
            [JsonPropertyName("Stable")]
            public ChannelMetaData Stable { get; set; } = new ChannelMetaData();

            /// <summary>The Beta channel.</summary>
            [JsonPropertyName("Beta")]
            public ChannelMetaData Beta { get; set; } = new ChannelMetaData();

            /// <summary>The Dev channel.</summary>
            [JsonPropertyName("Dev")]
            public ChannelMetaData Dev { get; set; } = new ChannelMetaData();

            /// <summary>The Canary channel.</summary>
            [JsonPropertyName("Canary")]
            public ChannelMetaData Canary { get; set; } = new ChannelMetaData();
        }

        /// <summary>The last known good version of one release channel.</summary>
        public class ChannelMetaData
        {
            /// <summary>The channel name, such as Stable.</summary>
            [JsonPropertyName("channel")]
            public string Channel { get; set; } = string.Empty;

            /// <summary>The full version, such as 120.0.6099.109.</summary>
            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            /// <summary>The Chromium revision.</summary>
            [JsonPropertyName("revision")]
            public string Revision { get; set; } = string.Empty;

            /// <summary>The download URLs per binary and platform.</summary>
            [JsonPropertyName("downloads")]
            public DownloadMetaData Downloads { get; set; } = new DownloadMetaData();
        }

        /// <summary>The downloads of one version, per binary.</summary>
        public class DownloadMetaData
        {
            /// <summary>Chrome downloads, one per platform.</summary>
            [JsonPropertyName("chrome")]
            public List<PlatformMetaData> Chrome { get; set; } = new List<PlatformMetaData>();

            /// <summary>chromedriver downloads, one per platform. Empty before milestone 115.</summary>
            [JsonPropertyName("chromedriver")]
            public List<PlatformMetaData> ChromeDriver { get; set; } = new List<PlatformMetaData>();

            /// <summary>chrome-headless-shell downloads, one per platform. Empty for versions published without it.</summary>
            [JsonPropertyName("chrome-headless-shell")]
            public List<PlatformMetaData> ChromeHeadlessShell { get; set; } = new List<PlatformMetaData>();
        }

        /// <summary>One download: a platform and its URL.</summary>
        public class PlatformMetaData
        {
            /// <summary>The Chrome for Testing platform name, such as win64 or mac-arm64.</summary>
            [JsonPropertyName("platform")]
            public string Platform { get; set; } = string.Empty;

            /// <summary>The download URL of the ZIP file.</summary>
            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }
    }
}
