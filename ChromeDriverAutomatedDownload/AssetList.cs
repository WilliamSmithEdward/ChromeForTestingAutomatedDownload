namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// Builds a map of version to download URL from a model that implements <see cref="IDownload"/>.
    /// </summary>
    public static class AssetList
    {
        /// <summary>
        /// Reads the endpoint of <typeparamref name="T"/> again and returns, for every version that has
        /// <paramref name="_binary"/> on <paramref name="_platform"/>, the version string and its URL.
        /// </summary>
        /// <typeparam name="T">The model to read. Only <see cref="LatestVersionsPerMilestoneWithDownload.ChromeVersionModel"/> implements <see cref="IDownload"/>.</typeparam>
        /// <param name="_binary">The binary to look up.</param>
        /// <param name="_platform">The platform to look up.</param>
        /// <returns>The map, which is empty when no version has the binary on the platform, or <see langword="null"/> for a <see cref="Binary"/> value outside the enum.</returns>
        /// <exception cref="HttpRequestException">The endpoint could not be read.</exception>
        /// <exception cref="System.Text.Json.JsonException">The response could not be deserialized.</exception>
        public static async Task<Dictionary<string, string>?> GetAssetListAsync<T>(Binary _binary, Platform _platform)
            where T : IChromeVersionModel, IDownload, new()
        {
            var model = await ChromeVersionModelFactory.CreateChromeVersionModelAsync<T>();

            return FromModel(model, _binary, _platform);
        }

        // The map for a model already read.
        internal static Dictionary<string, string>? FromModel(IDownload model, Binary binary, Platform platform)
        {
            string platformString = PlatformString.GetPlatformString(platform) ?? string.Empty;

            var versionObject = model.GetVersionObject().Values;

            return binary switch
            {
                Binary.Chrome => versionObject
                    .ToDictionary(
                        x => x.Version,
                        x => x.Downloads.Chrome
                            .Where(x => (x.Platform ?? string.Empty).Equals(platformString))
                            .Select(x => x.Url)
                            .FirstOrDefault()
                    )
                    .Where(x => string.IsNullOrEmpty(x.Value) == false)
                    .ToDictionary(x => x.Key ?? string.Empty, x => x.Value ?? string.Empty),
                Binary.ChromeDriver => versionObject
                    .ToDictionary(
                        x => x.Version,
                        x => x.Downloads.ChromeDriver
                            .Where(x => (x.Platform ?? string.Empty).Equals(platformString))
                            .Select(x => x.Url)
                            .FirstOrDefault()
                    )
                    .Where(x => string.IsNullOrEmpty(x.Value) == false)
                    .ToDictionary(x => x.Key ?? string.Empty, x => x.Value ?? string.Empty),
                Binary.ChromeHeadlessShell => versionObject
                    .ToDictionary(
                        x => x.Version,
                        x => x.Downloads.ChromeHeadlessShell
                            .Where(x => (x.Platform ?? string.Empty).Equals(platformString))
                            .Select(x => x.Url)
                            .FirstOrDefault()
                    )
                    .Where(x => string.IsNullOrEmpty(x.Value) == false)
                    .ToDictionary(x => x.Key ?? string.Empty, x => x.Value ?? string.Empty),
                _ => null,
            };
        }
    }
}
