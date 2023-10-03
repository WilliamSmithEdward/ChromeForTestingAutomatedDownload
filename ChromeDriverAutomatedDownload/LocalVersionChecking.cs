using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload
{
    public static class LocalVersionChecking
    {
        public static async Task<LocalVersion> GetChromeVersion()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string chromePath = Path.Combine(programFilesPath, "Google\\Chrome\\Application\\chrome.exe");

                await Console.Out.WriteLineAsync(programFilesPath);
                await Console.Out.WriteLineAsync(chromePath);

                if (File.Exists(chromePath))
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);

                    if (!string.IsNullOrEmpty(fileVersionInfo.FileVersion))
                    {
                        return new LocalVersion(fileVersionInfo.FileVersion);
                    }

                    else
                    {
                        throw new Exception("Unsupported Google Chrome configuration on this machine.");
                    }
                }
                
                else
                {
                    throw new Exception("Google Chrome not found on the machine.");
                }
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "google-chrome",
                            ArgumentList = { "--product-version" },
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );

                    string output = await (process?.StandardOutput ?? new StreamReader(new MemoryStream())).ReadToEndAsync() ?? string.Empty;
                    string error = await (process?.StandardError ?? new StreamReader(new MemoryStream())).ReadToEndAsync() ?? string.Empty;
                    await (process?.WaitForExitAsync() ?? new Task(() => { }));
                    process?.Kill(true);

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    return new LocalVersion(output);
                }
                
                catch (Exception ex)
                {
                    throw new Exception("An error occurred trying to execute 'google-chrome --product-version'", ex);
                }
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                            ArgumentList = { "--version" },
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    string output = await (process?.StandardOutput ?? new StreamReader(new MemoryStream())).ReadToEndAsync() ?? string.Empty;
                    string error = await (process?.StandardError ?? new StreamReader(new MemoryStream())).ReadToEndAsync() ?? string.Empty;
                    await (process?.WaitForExitAsync() ?? new Task(() => { }));
                    process?.Kill(true);

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    output = output.Replace("Google Chrome ", "");
                    return new LocalVersion(output);
                }
                
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred trying to execute '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version'", ex);
                }
            }

            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
        }
    }
}
