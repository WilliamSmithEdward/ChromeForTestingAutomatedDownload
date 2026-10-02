using System.Text.Json;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// Reads a Chrome for Testing endpoint and deserializes it into its model.
    /// </summary>
    public static class ChromeVersionModelFactory
    {
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
    }
}
