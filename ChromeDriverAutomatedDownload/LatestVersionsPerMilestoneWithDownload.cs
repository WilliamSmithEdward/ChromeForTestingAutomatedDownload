using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    public class LatestVersionsPerMilestoneWithDownload
    {
        public class ChromeVersionModel : IChromeVersionModel, IDownload
        {
            public Func<Task<string>> QueryEndpointAsync { get; set; } = GoogleChromeLabsEndpointQueries.GetLatestVersionsPerMilestoneWithDownloadAsync;

            public async Task<string?> GetMostRecentAssetURL(Binary binary, Platform platform)
            {
                var platformList = await GetAssetList(binary, platform);

                return platformList?
                    .OrderByDescending(x => x.Key)
                    .FirstOrDefault()
                    .Value;
            }

            public async Task<string?> GetMostRecentAssetURLByMajorReleaseNumber(Binary binary, Platform platform, int majorReleaseNumber)
            {
                var platformList = await GetAssetList(binary, platform);

                return platformList?
                    .OrderByDescending(x => x.Key)
                    .Where(x => x.Key.Split('.')[0].Equals(majorReleaseNumber.ToString()))
                    .FirstOrDefault()
                    .Value;
            }

            public async Task<Dictionary<string, string>?> GetAssetList(Binary _binary, Platform _platform)
            {
                var model = await ChromeVersionModelFactory.CreateChromeVersionModelAsync<ChromeVersionModel>();

                string platform = "";

                switch (_platform)
                {
                    case Platform.Linux64:
                        platform = "linux64";
                        break;
                    case Platform.MacArm64:
                        platform = "mac-arm64";
                        break;
                    case Platform.MacX64:
                        platform = "mac-x64";
                        break;
                    case Platform.Win32:
                        platform = "win32";
                        break;
                    case Platform.Win64:
                        platform = "win64";
                        break;
                }

                switch (_binary)
                {
                    case Binary.Chrome:
                        return model
                            .Milestones
                            .Values
                            .OrderByDescending(x => x.Version)
                            .ToDictionary(
                                milestone => milestone.Version,
                                milestone => milestone.Downloads.Chrome
                                    .Where(x => x.Platform.Equals(platform) && string.IsNullOrEmpty(x.Url) == false)
                                    .Select(x => x.Url)
                                    .FirstOrDefault()
                                ?? string.Empty
                            );
                    case Binary.ChromeDriver:
                        return model
                            .Milestones
                            .Values
                            .OrderByDescending(x => x.Version)
                            .ToDictionary(
                                milestone => milestone.Version,
                                milestone => milestone.Downloads.ChromeDriver
                                    .Where(x => x.Platform.Equals(platform) && string.IsNullOrEmpty(x.Url) == false)
                                    .Select(x => x.Url)
                                    .FirstOrDefault()
                                ?? string.Empty
                            );
                    case Binary.ChromeHeadlessShell:
                        return model
                            .Milestones
                            .Values
                            .OrderByDescending(x => x.Version)
                            .ToDictionary(
                                milestone => milestone.Version,
                                milestone => milestone.Downloads.ChromeHeadlessShell
                                    .Where(x => x.Platform.Equals(platform) && string.IsNullOrEmpty(x.Url) == false)
                                    .Select(x => x.Url)
                                    .FirstOrDefault()
                                ?? string.Empty
                            );
                }

                return null;
            }

            [JsonPropertyName("timestamp")]
            public DateTime TimeStamp { get; set; }

            [JsonPropertyName("milestones")]
            public Dictionary<string, Milestones> Milestones { get; set; } = new Dictionary<string, Milestones>();
        }

        public class Milestones
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

        public class DownloadMetaData
        {
            [JsonPropertyName("chrome")]
            public List<PlatformMetaData> Chrome { get; set; } = new List<PlatformMetaData>();

            [JsonPropertyName("chromedriver")]
            public List<PlatformMetaData> ChromeDriver { get; set; } = new List<PlatformMetaData>();

            [JsonPropertyName("chrome-headless-shell")]
            public List<PlatformMetaData> ChromeHeadlessShell { get; set; } = new List<PlatformMetaData>();
        }

        public class PlatformMetaData
        {
            [JsonPropertyName("platform")]
            public string Platform { get; set; } = string.Empty;

            [JsonPropertyName("url")]
            public string Url { get; set; } = string.Empty;
        }
    }
}
