using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// Reads the version of the Chrome installed on this machine.
    /// </summary>
    public static class LocalVersionChecking
    {
        /// <summary>How long a version command may run before it is stopped.</summary>
        internal static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// On Windows, reads the file version of the first chrome.exe found under Google\Chrome\Application in
        /// %ProgramW6432%, %ProgramFiles(x86)% or %LOCALAPPDATA%, in that order, without running it. On Linux, runs
        /// google-chrome --product-version, taking google-chrome from the first absolute folder on PATH that has it,
        /// never from the application's folder or the current directory. On macOS, runs
        /// /Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version. A command that does not finish
        /// within 30 seconds is stopped. Nothing the library downloads is run.
        /// </summary>
        /// <returns>The local version.</returns>
        /// <exception cref="FileNotFoundException">Chrome was not found.</exception>
        /// <exception cref="InvalidOperationException">Chrome's version could not be read, or the command could not start, exited with a code other than 0 or timed out (the inner exception says which).</exception>
        /// <exception cref="PlatformNotSupportedException">The operating system is not Windows, Linux or macOS.</exception>
        public static async Task<LocalVersion> GetChromeVersion()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var chromePath = FindWindowsChrome(Environment.GetEnvironmentVariable, File.Exists);

                var fileVersionInfo = FileVersionInfo.GetVersionInfo(chromePath);

                if (!string.IsNullOrEmpty(fileVersionInfo.FileVersion))
                {
                    return new LocalVersion(fileVersionInfo.FileVersion.Trim());
                }

                else
                {
                    throw new InvalidOperationException("Unsupported Google Chrome configuration on this machine.");
                }
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var command = FindLinuxChrome(
                    Environment.GetEnvironmentVariable("PATH"),
                    AppContext.BaseDirectory,
                    Environment.CurrentDirectory,
                    File.Exists)
                    ?? throw new FileNotFoundException("Google Chrome not found on the machine: no google-chrome in an absolute folder on PATH.");

                try
                {
                    return ParseVersion(await RunAsync(command, "--product-version"));
                }

                catch (Exception ex)
                {
                    throw new InvalidOperationException($"An error occurred trying to execute '{command} --product-version'", ex);
                }
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    return ParseVersion(await RunAsync(MacChrome, "--version"));
                }

                catch (Exception ex)
                {
                    throw new InvalidOperationException($"An error occurred trying to execute '{MacChrome} --version'", ex);
                }
            }

            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
        }

        internal const string MacChrome = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";

        internal static string WindowsChromePath(string? folder) => (folder ?? string.Empty) + "\\Google\\Chrome\\Application\\chrome.exe";

        // The chrome.exe whose version is read: a machine-wide 64-bit install, a machine-wide install
        // under Program Files (x86), or a per-user install, in that order.
        internal static string FindWindowsChrome(Func<string, string?> getEnvironmentVariable, Func<string, bool> exists)
        {
            var candidates = new[] { "ProgramW6432", "ProgramFiles(x86)", "LOCALAPPDATA" }
                .Select(getEnvironmentVariable)
                .Where(folder => !string.IsNullOrEmpty(folder))
                .Select(WindowsChromePath);

            return candidates.FirstOrDefault(exists)
                ?? throw new FileNotFoundException("Google Chrome not found on the machine.");
        }

        // The google-chrome that is run: the first one in an absolute PATH folder. A bare file name would
        // let .NET look in the application's folder and the current directory first, and an empty or
        // relative PATH entry means the current directory too, so both are skipped. The two folders are
        // parameters only so a test can show they are not searched.
        internal static string? FindLinuxChrome(string? pathVariable, string? applicationFolder, string currentDirectory, Func<string, bool> exists)
        {
            return (pathVariable ?? string.Empty)
                .Split(Path.PathSeparator)
                .Where(folder => folder.Length > 0 && Path.IsPathRooted(folder))
                .Select(folder => Path.Combine(folder, "google-chrome"))
                .FirstOrDefault(exists);
        }

        internal static LocalVersion ParseVersion(string output)
        {
            var version = output.Trim();
            if (version.StartsWith("Google Chrome ", StringComparison.Ordinal)) version = version["Google Chrome ".Length..].Trim();
            return new LocalVersion(version);
        }

        // What a finished version command means: its output if it exited with 0, otherwise an
        // exception carrying what it wrote to standard error. Chrome can log warnings to standard
        // error while printing its version, so standard error alone is not a failure.
        internal static string CheckResult(int exitCode, string output, string error)
        {
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"The command exited with code {exitCode}: {error.Trim()}");
            }

            return output;
        }

        private static async Task<string> RunAsync(string fileName, string argument)
        {
            using var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = fileName,
                    ArgumentList = { argument },
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            ) ?? throw new InvalidOperationException($"Could not start '{fileName}'.");

            // Both streams are read at once, so a full error pipe cannot stall the output.
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();

            using var timeout = new CancellationTokenSource(CommandTimeout);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                throw new TimeoutException($"'{fileName} {argument}' did not finish within {CommandTimeout.TotalSeconds} seconds.");
            }

            return CheckResult(process.ExitCode, await output, await error);
        }
    }
}
