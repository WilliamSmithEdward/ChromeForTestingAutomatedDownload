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
        public Task<string?> GetMostRecentAssetURL(Binary binary, Platform platform);

        public Task<string?> GetMostRecentAssetURLByMajorReleaseNumber(Binary binary, Platform platform, int majorReleaseNumber);

        public Task<Dictionary<string, string>?> GetAssetList(Binary _binary, Platform _platform);
    }
}
