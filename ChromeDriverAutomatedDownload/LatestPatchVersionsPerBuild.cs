using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>Model of latest-patch-versions-per-build.json. The type to deserialize is the nested <see cref="ChromeVersionModel"/>.</summary>
    public class LatestPatchVersionsPerBuild
    {
        /// <summary>The content of latest-patch-versions-per-build.json. Create it with <see cref="ChromeVersionModelFactory"/>.</summary>
        public class ChromeVersionModel : IChromeVersionModel
        {
            /// <summary>Reads latest-patch-versions-per-build.json. <see cref="ChromeVersionModelFactory"/> uses the default value of a new instance, so setting this does not change what the factory reads.</summary>
            [JsonIgnore]
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLatestPatchVersionsPerBuildAsync;

            /// <summary>When the endpoint was generated, in UTC.</summary>
            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            /// <summary>The latest patch version per build, keyed by build such as 113.0.5672.</summary>
            [JsonPropertyName("builds")]
            public Dictionary<string, Build> Builds { get; set; } = new Dictionary<string, Build>();
        }

        /// <summary>The latest patch version of one build.</summary>
        public class Build
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

