using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class KnownGoodVersions
    {
        public class ChromeVersionModel : IChromeVersionModel
        {
            public Func<Task<string>> QueryEndpoint { get; set; } = GoogleChromeLabsEndpointQueries.GetKnownGoodVersionsAsync;

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
        }
    }
}
