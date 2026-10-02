namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// A model of one Chrome for Testing endpoint.
    /// </summary>
    public interface IChromeVersionModel
    {
        /// <summary>
        /// Reads the model's endpoint and returns the JSON. <see cref="ChromeVersionModelFactory"/> calls the
        /// default value of a new instance, so setting this property does not change what the factory reads.
        /// </summary>
        public Func<Task<string>> QueryEndpointAsync { get; set; }
    }
}