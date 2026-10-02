using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// The downloads of one version in latest-versions-per-milestone-with-downloads.json, per binary.
    /// </summary>
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
}
