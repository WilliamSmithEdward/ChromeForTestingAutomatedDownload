using System.Text.Json;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// Reads a Chrome for Testing endpoint and deserializes it into its model.
    /// </summary>
    public static class ChromeVersionModelFactory
    {
        // The endpoint each model reads, for the overload that takes an HttpClient.
        private static readonly Dictionary<Type, string> _endpoints = new()
        {
            [typeof(KnownGoodVersions.ChromeVersionModel)] = "known-good-versions.json",
            [typeof(KnownGoodVersionsWithDownload.ChromeVersionModel)] = "known-good-versions-with-downloads.json",
            [typeof(LastKnownGoodVersions.ChromeVersionModel)] = "last-known-good-versions.json",
            [typeof(LastKnownGoodVersionsWithDownloads.ChromeVersionModel)] = "last-known-good-versions-with-downloads.json",
            [typeof(LatestPatchVersionsPerBuild.ChromeVersionModel)] = "latest-patch-versions-per-build.json",
            [typeof(LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel)] = "latest-patch-versions-per-build-with-downloads.json",
            [typeof(LatestVersionsPerMilestone.ChromeVersionModel)] = "latest-versions-per-milestone.json",
            [typeof(LatestVersionsPerMilestoneWithDownload.ChromeVersionModel)] = "latest-versions-per-milestone-with-downloads.json",
        };

        /// <summary>
        /// Calls the default <see cref="IChromeVersionModel.QueryEndpointAsync"/> of a new <typeparamref name="T"/>
        /// and deserializes the JSON it returns into a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">One of the ChromeVersionModel classes, such as <see cref="LatestVersionsPerMilestoneWithDownload.ChromeVersionModel"/>.</typeparam>
        /// <returns>The deserialized model.</returns>
        /// <exception cref="HttpRequestException">The endpoint could not be read.</exception>
        /// <exception cref="JsonException">The response could not be deserialized.</exception>
        public static async Task<T> CreateChromeVersionModelAsync<T>() where T : IChromeVersionModel, new()
        {
            var response = await new T().QueryEndpointAsync();

            var deserializedObject = JsonSerializer.Deserialize<T>(response);
            if (deserializedObject != null) return deserializedObject;

            throw new JsonException("Failed to deserialize endpoint.");
        }

        /// <summary>
        /// Reads the endpoint of <typeparamref name="T"/> with <paramref name="httpClient"/> and deserializes it.
        /// Use it to set your own timeout, proxy or handler. The request goes to
        /// https://googlechromelabs.github.io/chrome-for-testing/ plus the endpoint's file name.
        /// </summary>
        /// <typeparam name="T">One of the eight ChromeVersionModel classes in this library.</typeparam>
        /// <param name="httpClient">The client that sends the request. It is not disposed.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The deserialized model.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="httpClient"/> is null.</exception>
        /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not one of this library's models.</exception>
        /// <exception cref="HttpRequestException">The endpoint could not be read.</exception>
        /// <exception cref="JsonException">The response could not be deserialized.</exception>
        public static async Task<T> CreateChromeVersionModelAsync<T>(HttpClient httpClient, CancellationToken cancellationToken = default)
            where T : IChromeVersionModel, new()
        {
            ArgumentNullException.ThrowIfNull(httpClient);

            if (!_endpoints.TryGetValue(typeof(T), out var endpoint))
            {
                throw new NotSupportedException($"{typeof(T).FullName} is not one of this library's Chrome for Testing models.");
            }

            var response = await GoogleChromeLabsEndpointQueries.GetStringAsync(httpClient, endpoint, cancellationToken);

            var deserializedObject = JsonSerializer.Deserialize<T>(response);
            if (deserializedObject != null) return deserializedObject;

            throw new JsonException("Failed to deserialize endpoint.");
        }
    }
}
