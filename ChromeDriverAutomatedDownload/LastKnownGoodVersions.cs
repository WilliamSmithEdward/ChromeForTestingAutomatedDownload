using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>Model of last-known-good-versions.json. The type to deserialize is the nested <see cref="ChromeVersionModel"/>.</summary>
    public class LastKnownGoodVersions
    {
        /// <summary>The content of last-known-good-versions.json. Create it with <see cref="ChromeVersionModelFactory"/>.</summary>
        public class ChromeVersionModel : IChromeVersionModel
        {
            /// <summary>Reads last-known-good-versions.json. <see cref="ChromeVersionModelFactory"/> uses the default value of a new instance, so setting this does not change what the factory reads.</summary>
            [JsonIgnore]
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLastKnownGoodVersionsAsync;

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
        }
    }
}
