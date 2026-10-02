using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// Works out the Chrome for Testing platform of this machine.
    /// </summary>
    public static class MachineOSPlatform
    {
        /// <summary>
        /// On Windows, returns <see cref="Platform.Win32"/> if chrome.exe is in %ProgramFiles(x86)%\Google\Chrome\Application,
        /// otherwise <see cref="Platform.Win64"/> if it is in %ProgramW6432%\Google\Chrome\Application; the answer depends
        /// on where Chrome is installed, not on the processor. On Linux, returns <see cref="Platform.Linux64"/> for an x64
        /// process. On macOS, returns <see cref="Platform.MacX64"/> or <see cref="Platform.MacArm64"/> by the process architecture.
        /// </summary>
        /// <returns>The platform.</returns>
        /// <exception cref="Exception">Chrome was not found on Windows, or the operating system or architecture is not one of the above.</exception>
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
