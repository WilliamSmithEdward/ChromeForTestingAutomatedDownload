using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>Model of known-good-versions.json. The type to deserialize is the nested <see cref="ChromeVersionModel"/>.</summary>
    public class KnownGoodVersions
    {
        /// <summary>The content of known-good-versions.json. Create it with <see cref="ChromeVersionModelFactory"/>.</summary>
        public class ChromeVersionModel : IChromeVersionModel
        {
            /// <summary>Reads known-good-versions.json. <see cref="ChromeVersionModelFactory"/> uses the default value of a new instance, so setting this does not change what the factory reads.</summary>
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetKnownGoodVersionsAsync;

            /// <summary>Every version in the endpoint, oldest first.</summary>
            [JsonPropertyName("versions")]
            public List<VersionMetaData> Versions { get; set; } = new List<VersionMetaData>();
                
            /// <summary>When the endpoint was generated, in UTC.</summary>
            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }
        }

        /// <summary>One version in known-good-versions.json.</summary>
        public class VersionMetaData
        {
            /// <summary>The full version, such as 120.0.6099.109.</summary>
            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            /// <summary>The Chromium revision.</summary>
            [JsonPropertyName("revision")]
            public string Revision { get; set; } = string.Empty;
        }
    }
}
