namespace ChromeForTestingAutomatedDownload
{
    public static class AssetList
    {
        public static async Task<Dictionary<string, string>?> GetAssetListAsync<T>(Binary _binary, Platform _platform) 
            where T : IChromeVersionModel, IDownload, new()
        {
            string platform = PlatformString.GetPlatformString(_platform) ?? string.Empty;

            var model = await ChromeVersionModelFactory.CreateChromeVersionModelAsync<T>();

            var versionObject = model.GetVersionObject().Values;

            return versionObject
                .ToDictionary(
                    x => x.Version,
                    x => x.Downloads.ChromeDriver
                        .Where(x => (x.Platform ?? string.Empty).Equals(platform))
                        .Select(x => x.Url)
                        .FirstOrDefault()
                )
                .Where(x => string.IsNullOrEmpty(x.Value) == false)
                .ToDictionary(x => x.Key ?? string.Empty, x => x.Value ?? string.Empty);
        }
    }
}
