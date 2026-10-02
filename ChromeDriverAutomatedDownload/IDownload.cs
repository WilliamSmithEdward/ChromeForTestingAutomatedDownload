namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// A binary that Chrome for Testing publishes.
    /// </summary>
    public enum Binary
    {
        /// <summary>Chrome.</summary>
        Chrome,
        /// <summary>chromedriver.</summary>
        ChromeDriver,
        /// <summary>chrome-headless-shell.</summary>
        ChromeHeadlessShell
    }

    /// <summary>
    /// A Chrome for Testing platform.
    /// </summary>
    public enum Platform
    {
        /// <summary>linux64.</summary>
        Linux64,
        /// <summary>mac-arm64.</summary>
        MacArm64,
        /// <summary>mac-x64.</summary>
        MacX64,
        /// <summary>win32.</summary>
        Win32,
        /// <summary>win64.</summary>
        Win64,
        /// <summary>linux-arm64, which Chrome for Testing publishes from milestone 153.</summary>
        LinuxArm64
    }

    /// <summary>
    /// A model whose versions carry download URLs, with lookups by binary and platform.
    /// Implemented by <see cref="LatestVersionsPerMilestoneWithDownload.ChromeVersionModel"/> only.
    /// </summary>
    public interface IDownload
    {
        /// <summary>Returns the model's versions, keyed as in the endpoint.</summary>
        /// <returns>The versions.</returns>
        public Dictionary<string, IVersionObject> GetVersionObject();

        /// <summary>Returns the URL from the highest version the endpoint lists, which is usually a Canary or Dev build.</summary>
        /// <param name="binary">The binary to look up.</param>
        /// <param name="platform">The platform to look up.</param>
        /// <returns>The URL, or <see langword="null"/> if there is none.</returns>
        public Task<string?> GetMostRecentAssetURLAsync(Binary binary, Platform platform);

        /// <summary>Returns the URL from the highest version with the given major version number.</summary>
        /// <param name="binary">The binary to look up.</param>
        /// <param name="platform">The platform to look up.</param>
        /// <param name="majorReleaseNumber">The major version, such as 120.</param>
        /// <returns>The URL, or <see langword="null"/> if there is none.</returns>
        public Task<string?> GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary binary, Platform platform, int majorReleaseNumber);

        /// <summary>Returns the URL for a version the endpoint lists exactly.</summary>
        /// <param name="binary">The binary to look up.</param>
        /// <param name="platform">The platform to look up.</param>
        /// <param name="fullVersionNumber">The full version, such as 120.0.6099.109.</param>
        /// <returns>The URL, or <see langword="null"/> if the endpoint does not list that version with the binary on the platform.</returns>
        public Task<string?> GetAssetURLByFullVersionNumberAsync(Binary binary, Platform platform, string fullVersionNumber);
    }
}
