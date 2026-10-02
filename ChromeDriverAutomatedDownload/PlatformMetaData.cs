using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// One download in latest-versions-per-milestone-with-downloads.json.
    /// </summary>
    public class PlatformMetaData
    {
        /// <summary>The Chrome for Testing platform name, such as win64 or mac-arm64.</summary>
        [JsonPropertyName("platform")]
        public string? Platform { get; set; } = string.Empty;

        /// <summary>The download URL of the ZIP file.</summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; } = string.Empty;
    }
}
