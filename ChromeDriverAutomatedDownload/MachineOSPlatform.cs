using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload
{
    public static class MachineOSPlatform
    {
        public static Platform GetPlatform()
        {
            string osDescription = RuntimeInformation.OSDescription;
            Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;

            if (osDescription.Contains("Windows"))
            {
                if (File.Exists(Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"), "Google\\Chrome\\Application\\chrome.exe"))) return Platform.Win32;
                else if (File.Exists(Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), "Google\\Chrome\\Application\\chrome.exe"))) return Platform.Win64;
                else throw new Exception("Google Chrome not found on the machine.");
            }

            if (osDescription.Contains("Linux"))
            {
                if (processArchitecture == Architecture.X64)
                {
                    return Platform.Linux64;
                }

                else
                {
                    throw new Exception("Unknown Linux architecture.");
                }
            }

            if (osDescription.Contains("Darwin"))
            {
                if (processArchitecture == Architecture.X64)
                {
                    return Platform.MacX64;
                }

                else if (processArchitecture == Architecture.Arm64)
                {
                    return Platform.MacArm64;
                }

                else
                {
                    throw new Exception("Unknown macOS architecture.");
                }
            }

            throw new Exception("Unknown OS platform.");
        }
    }
}
