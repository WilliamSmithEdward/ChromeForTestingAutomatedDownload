using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload
{
    public static class MachineOSPlatform
    {
        public static Platform GetPlatform()
        {
            string osDescription = RuntimeInformation.OSDescription;
            Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;

            return osDescription switch
            {
                var windows when windows.Contains("Windows") => processArchitecture switch
                {
                    Architecture.X64 => Platform.Win64,
                    Architecture.X86 => Platform.Win32,
                    _ => throw new Exception("Unknown Windows architecture."),
                },
                var linux when linux.Contains("Linux") => processArchitecture switch
                {
                    Architecture.X64 => Platform.Linux64,
                    _ => throw new Exception("Unknown Linux architecture."),
                },
                var darwin when darwin.Contains("Darwin") => processArchitecture switch
                {
                    Architecture.X64 => Platform.MacX64,
                    Architecture.Arm64 => Platform.MacArm64,
                    _ => throw new Exception("Unknown macOS architecture."),
                },
                _ => throw new Exception("Unknown operating system."),
            };
        }
    }
}
