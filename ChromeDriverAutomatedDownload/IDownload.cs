namespace ChromeForTestingAutomatedDownload
{
    public enum Binary
    {
        Chrome,
        ChromeDriver,
        ChromeHeadlessShell
    }

    public enum Platform
    {
        Linux64,
        MacArm64,
        MacX64,
        Win32,
        Win64
    }

    public interface IDownload
    {
        public Dictionary<string, IVersionObject> GetVersionObject();

        public Task<string?> GetMostRecentAssetURLAsync(Binary binary, Platform platform);

        public Task<string?> GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary binary, Platform platform, int majorReleaseNumber);

        public Task<string?> GetAssetURLByFullVersionNumberAsync(Binary binary, Platform platform, string fullVersionNumber);
    }
}
