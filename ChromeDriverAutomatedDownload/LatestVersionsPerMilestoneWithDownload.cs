using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>Model of latest-versions-per-milestone-with-downloads.json. The type to deserialize is the nested <see cref="ChromeVersionModel"/>.</summary>
    public class LatestVersionsPerMilestoneWithDownload
    {
        /// <summary>The content of latest-versions-per-milestone-with-downloads.json. Create it with <see cref="ChromeVersionModelFactory"/>.</summary>
        public class ChromeVersionModel : IChromeVersionModel, IDownload
        {
            /// <summary>Reads latest-versions-per-milestone-with-downloads.json. <see cref="ChromeVersionModelFactory"/> uses the default value of a new instance, so setting this does not change what the factory reads.</summary>
            [JsonIgnore]
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLatestVersionsPerMilestoneWithDownloadAsync;

            /// <inheritdoc/>
            public Dictionary<string, IVersionObject> GetVersionObject()
            {
                return Milestones.ToDictionary(x => x.Key, x => (IVersionObject)x.Value);
            }

            /// <inheritdoc/>
            /// <remarks>Answers from this model's milestones; nothing is read from the network.</remarks>
            public Task<string?> GetMostRecentAssetURLAsync(Binary binary, Platform platform)
            {
                return Task.FromResult(AssetList.NewestFirst(AssetList.FromModel(this, binary, platform))
                    .Select(x => (string?)x.Value)
                    .FirstOrDefault());
            }

            /// <inheritdoc/>
            /// <remarks>Answers from this model's milestones; nothing is read from the network.</remarks>
            public Task<string?> GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary binary, Platform platform, int majorReleaseNumber)
            {
                return Task.FromResult(AssetList.NewestFirst(AssetList.FromModel(this, binary, platform))
                    .Where(x => x.Key.Split('.')[0].Equals(majorReleaseNumber.ToString()))
                    .Select(x => (string?)x.Value)
                    .FirstOrDefault());
            }

            /// <inheritdoc/>
            /// <remarks>Answers from this model's milestones; nothing is read from the network.</remarks>
            public Task<string?> GetAssetURLByFullVersionNumberAsync(Binary binary, Platform platform, string fullVersionNumber)
            {
                var platformList = AssetList.FromModel(this, binary, platform);

                return Task.FromResult(platformList != null && platformList.TryGetValue(fullVersionNumber, out var url) ? url : null);
            }

            /// <summary>When the endpoint was generated, in UTC.</summary>
            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            /// <summary>The latest version per milestone, keyed by milestone such as 120.</summary>
            [JsonPropertyName("milestones")]
            public Dictionary<string, Milestones> Milestones { get; set; } = new Dictionary<string, Milestones>();
        }

        /// <summary>The latest version of one milestone.</summary>
        public class Milestones : IVersionObject
        {
            /// <summary>The milestone number, such as 120. The JSON property is milestone.</summary>
            [JsonPropertyName("milestone")]
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
    }
}
