using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class LatestVersionsPerMilestoneWithDownload
    {
        public class ChromeVersionModel : IChromeVersionModel, IDownload
        {
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLatestVersionsPerMilestoneWithDownloadAsync;

            public Dictionary<string, IVersionObject> GetVersionObject()
            {
                return Milestones.ToDictionary(x => x.Key, x => (IVersionObject)x.Value);
            }

            public async Task<string?> GetMostRecentAssetURLAsync(Binary binary, Platform platform)
            {
                var platformList = await AssetList.GetAssetListAsync<ChromeVersionModel>(binary, platform);

                return platformList?
                    .OrderByDescending(x => x.Key)
                    .FirstOrDefault()
                    .Value;
            }

            public async Task<string?> GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary binary, Platform platform, int majorReleaseNumber)
            {
                var platformList = await AssetList.GetAssetListAsync<ChromeVersionModel>(binary, platform);

                if (platformList == null) return null;

                foreach (var item in platformList.Keys)
                {
                    await Console.Out.WriteLineAsync(item);
                }

                return platformList?
                    .OrderByDescending(x => x.Key)
                    .Where(x => x.Key.Split('.')[0].Equals(majorReleaseNumber.ToString()))
                    .FirstOrDefault()
                    .Value;
            }

            public async Task<string?> GetAssetURLByFullVersionNumberAsync(Binary binary, Platform platform, string fullVersionNumber)
            {
                var platformList = await AssetList.GetAssetListAsync<ChromeVersionModel>(binary, platform);

                return platformList?
                    .Where(x => x.Key.Equals(fullVersionNumber))
                    .FirstOrDefault()
                    .Value;
            }

            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            [JsonPropertyName("milestones")]
            public Dictionary<string, Milestones> Milestones { get; set; } = new Dictionary<string, Milestones>();
        }

        public class Milestones : IVersionObject
        {
            [JsonPropertyName("milestone")]
            public string Channel { get; set; } = string.Empty;

            [JsonPropertyName("version")]
            public string Version { get; set; } = string.Empty;

            [JsonPropertyName("revision")]
            public string Revision { get; set; } = string.Empty;

            [JsonPropertyName("downloads")]
            public DownloadMetaData Downloads { get; set; } = new DownloadMetaData();
        }
    }
}
