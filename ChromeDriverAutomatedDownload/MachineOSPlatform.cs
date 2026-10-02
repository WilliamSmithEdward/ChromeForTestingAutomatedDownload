using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// Works out the Chrome for Testing platform of this machine.
    /// </summary>
    public static class MachineOSPlatform
    {
        /// <summary>
        /// On Windows, returns <see cref="Platform.Win64"/> on a 64-bit Windows, whatever the process or the Chrome
        /// install, and <see cref="Platform.Win32"/> on a 32-bit one; Chrome need not be installed. On Linux, returns
        /// <see cref="Platform.Linux64"/> or <see cref="Platform.LinuxArm64"/> by the process architecture. On macOS,
        /// returns <see cref="Platform.MacX64"/> or <see cref="Platform.MacArm64"/> by the process architecture.
        /// </summary>
        /// <returns>The platform.</returns>
        /// <exception cref="PlatformNotSupportedException">The operating system or architecture is not one of the above.</exception>
        public static Platform GetPlatform() =>
            Detect(RuntimeInformation.OSDescription, RuntimeInformation.ProcessArchitecture, Environment.Is64BitOperatingSystem);

        internal static Platform Detect(string osDescription, Architecture processArchitecture, bool is64BitOperatingSystem)
        {
            if (osDescription.Contains("Windows"))
            {
                // win64 chromedriver runs on any 64-bit Windows, Windows 11 on Arm64 through emulation,
                // and drives a 32-bit Chrome as well as a 64-bit one.
                return is64BitOperatingSystem ? Platform.Win64 : Platform.Win32;
            }

            if (osDescription.Contains("Linux"))
            {
                if (processArchitecture == Architecture.X64)
                {
                    return Platform.Linux64;
                }

                else if (processArchitecture == Architecture.Arm64)
                {
                    return Platform.LinuxArm64;
                }

                else
                {
                    throw new PlatformNotSupportedException("Unknown Linux architecture.");
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
                    throw new PlatformNotSupportedException("Unknown macOS architecture.");
                }
            }

            throw new PlatformNotSupportedException("Unknown OS platform.");
        }
    }
}
