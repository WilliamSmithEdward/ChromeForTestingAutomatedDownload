﻿using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class PlatformMetaData
    {
        [JsonPropertyName("platform")]
        public string? Platform { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string? Url { get; set; } = string.Empty;
    }
}
