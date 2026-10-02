namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// Reads the Chrome for Testing JSON endpoints under https://googlechromelabs.github.io/chrome-for-testing/
    /// and returns each response as a string. All methods share one <see cref="HttpClient"/> with the default
    /// 100 second timeout, and throw <see cref="HttpRequestException"/> when the request fails.
    /// </summary>
    public class GoogleChromeLabsEndpointQueries
    {
        private static readonly HttpClient _httpClient = new();

        /// <summary>Reads known-good-versions.json.</summary>
        /// <returns>The JSON response.</returns>
        public static async Task<string> GetKnownGoodVersionsAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/known-good-versions.json");

        /// <summary>Reads known-good-versions-with-downloads.json.</summary>
        /// <returns>The JSON response.</returns>
        public static async Task<string> GetKnownGoodVersionsWithDownloadAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json");

        /// <summary>Reads last-known-good-versions.json.</summary>
        /// <returns>The JSON response.</returns>
        public static async Task<string> GetLastKnownGoodVersionsAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions.json");

        /// <summary>Reads last-known-good-versions-with-downloads.json.</summary>
        /// <returns>The JSON response.</returns>
        public static async Task<string> GetLastKnownGoodVersionsWithDownloadAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions-with-downloads.json");

        /// <summary>Reads latest-patch-versions-per-build.json.</summary>
        /// <returns>The JSON response.</returns>
        public static async Task<string> GetLatestPatchVersionsPerBuildAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/latest-patch-versions-per-build.json");

        /// <summary>Reads latest-patch-versions-per-build-with-downloads.json.</summary>
        /// <returns>The JSON response.</returns>
        public static async Task<string> GetLatestPatchVersionsPerBuildAsyncWithDownloads() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/latest-patch-versions-per-build-with-downloads.json");

        /// <summary>Reads latest-versions-per-milestone.json.</summary>
        /// <returns>The JSON response.</returns>
        public static async Task<string> GetLatestVersionsPerMilestoneAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/latest-versions-per-milestone.json");

        /// <summary>Reads latest-versions-per-milestone-with-downloads.json.</summary>
        /// <returns>The JSON response.</returns>
        public static async Task<string> GetLatestVersionsPerMilestoneWithDownloadAsync() =>
            await _httpClient.GetStringAsync("https://googlechromelabs.github.io/chrome-for-testing/latest-versions-per-milestone-with-downloads.json");
    }
}