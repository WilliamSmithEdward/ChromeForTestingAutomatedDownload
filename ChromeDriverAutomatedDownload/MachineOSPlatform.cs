using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload
{
    public static class MachineOSPlatform
    {
        public static Platform GetPlatform()
        {
            string osDescription = RuntimeInformation.OSDescription;
            Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;

            switch (osDescription)
            {
                case var windows when windows.Contains("Windows"):
                    switch (processArchitecture)
                    {
                        case Architecture.X64:
                            return Platform.Win64;
                        case Architecture.X86:
                            return Platform.Win32;
                        default:
                            throw new Exception("Unknown Windows architecture.");
                    }

                case var linux when linux.Contains("Linux"):
                    switch (processArchitecture)
                    {
                        case Architecture.X64:
                            return Platform.Linux64;
                        default:
                            throw new Exception("Unknown Linux architecture.");
                    }

                case var darwin when darwin.Contains("Darwin"):
                    switch (processArchitecture)
                    {
                        case Architecture.X64:
                            return Platform.MacX64;
                        case Architecture.Arm64:
                            return Platform.MacArm64;
                        default:
                            throw new Exception("Unknown macOS architecture.");
                    }

                default:
                    throw new Exception("Unknown operating system.");
            }
        }
    }
}
