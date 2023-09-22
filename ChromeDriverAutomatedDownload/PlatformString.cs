namespace ChromeForTestingAutomatedDownload
{
    internal class PlatformString
    {
        public static string? GetPlatformString(Platform _platform)
        {
            switch (_platform)
            {
                case Platform.Linux64:
                    return "linux64";
                case Platform.MacArm64:
                    return "mac-arm64";
                case Platform.MacX64:
                    return "mac-x64";
                case Platform.Win32:
                    return "win32";
                case Platform.Win64:
                    return "win64";
                default:
                    return null;
            }
        }
    }
}
