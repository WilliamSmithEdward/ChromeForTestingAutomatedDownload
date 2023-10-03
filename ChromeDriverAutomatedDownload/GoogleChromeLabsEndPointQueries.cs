namespace ChromeForTestingAutomatedDownload
{
    public class GoogleChromeLabsEndpointQueries
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<string> GetKnownGoodVersionsAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/known-good-versions.json");

        public static async Task<string> GetKnownGoodVersionsWithDownloadAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json");

        public static async Task<string> GetLastKnownGoodVersionsAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions.json");

        public static async Task<string> GetLastKnownGoodVersionsWithDownloadAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions-with-downloads.json");

        public static async Task<string> GetLatestPatchVersionsPerBuildAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/latest-patch-versions-per-build.json");

        public static async Task<string> GetLatestPatchVersionsPerBuildAsyncWithDownloads() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/latest-patch-versions-per-build-with-downloads.json");

        public static async Task<string> GetLatestVersionsPerMilestoneAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/latest-versions-per-milestone.json");

        public static async Task<string> GetLatestVersionsPerMilestoneWithDownloadAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/latest-versions-per-milestone-with-downloads.json");
    }
}