using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload.Tests;

/// <summary>
/// Finding the local Chrome and the machine's platform, through the seams the public methods
/// call with the real environment. Nothing here runs a process or reads a real file.
/// </summary>
public class LocalDetectionTests
{
    private static readonly Dictionary<string, string?> WindowsEnvironment = new()
    {
        ["ProgramW6432"] = @"C:\Program Files",
        ["ProgramFiles(x86)"] = @"C:\Program Files (x86)",
        ["LOCALAPPDATA"] = @"C:\Users\someone\AppData\Local",
    };

    private static string? Env(string name) => WindowsEnvironment.GetValueOrDefault(name);

    private const string X64Chrome = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
    private const string X86Chrome = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
    private const string UserChrome = @"C:\Users\someone\AppData\Local\Google\Chrome\Application\chrome.exe";

    [Fact]
    public void Chrome_in_Program_Files_is_found()
    {
        Assert.Equal(X64Chrome, LocalVersionChecking.FindWindowsChrome(Env, p => p == X64Chrome));
    }

    [Fact]
    public void Chrome_only_in_Program_Files_x86_is_found()
    {
        Assert.Equal(X86Chrome, LocalVersionChecking.FindWindowsChrome(Env, p => p == X86Chrome));
    }

    [Fact]
    public void Chrome_installed_for_one_user_is_found()
    {
        Assert.Equal(UserChrome, LocalVersionChecking.FindWindowsChrome(Env, p => p == UserChrome));
    }

    [Fact]
    public void Program_Files_wins_when_Chrome_is_in_both()
    {
        Assert.Equal(X64Chrome, LocalVersionChecking.FindWindowsChrome(Env, p => p == X64Chrome || p == X86Chrome));
    }

    [Fact]
    public void No_Chrome_on_Windows_says_so()
    {
        var error = Assert.ThrowsAny<Exception>(() => LocalVersionChecking.FindWindowsChrome(Env, _ => false));
        Assert.Contains("not found", error.Message);
    }

    [Fact]
    public void google_chrome_is_taken_from_PATH_not_from_the_application_folder_or_the_current_directory()
    {
        var onPath = Path.Combine("/usr/bin", "google-chrome");
        var inApp = Path.Combine("/srv/app", "google-chrome");
        var inCwd = Path.Combine("/home/someone", "google-chrome");

        var found = LocalVersionChecking.FindLinuxChrome(
            "/usr/local/bin" + Path.PathSeparator + "/usr/bin", "/srv/app", "/home/someone",
            p => p == onPath || p == inApp || p == inCwd);

        Assert.Equal(onPath, found);
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("bin")]
    public void A_relative_or_empty_PATH_entry_is_skipped(string entry)
    {
        var relative = Path.Combine(entry, "google-chrome");
        var onPath = Path.Combine("/usr/bin", "google-chrome");

        var found = LocalVersionChecking.FindLinuxChrome(
            entry + Path.PathSeparator + "/usr/bin", "/srv/app", "/home/someone",
            p => p == relative || p == onPath);

        Assert.Equal(onPath, found);
    }

    [Fact]
    public void No_google_chrome_on_PATH_gives_null()
    {
        Assert.Null(LocalVersionChecking.FindLinuxChrome("/usr/bin", "/srv/app", "/home/someone", _ => false));
    }

    [Theory]
    [InlineData("120.0.6099.109\n", "120.0.6099.109")]
    [InlineData("Google Chrome 120.0.6099.109 \n", "120.0.6099.109")]
    [InlineData("  154.0.8037.97\r\n", "154.0.8037.97")]
    public void The_version_output_is_trimmed(string output, string version)
    {
        var parsed = LocalVersionChecking.ParseVersion(output);

        Assert.Equal(version, parsed.VersionString);
    }

    [Fact]
    public void A_warning_on_standard_error_does_not_fail_a_command_that_succeeded()
    {
        Assert.Equal("120.0.6099.109\n",
            LocalVersionChecking.CheckResult(0, "120.0.6099.109\n", "[1234:5678:ERROR] some warning\n"));
    }

    [Fact]
    public void A_command_that_failed_throws_with_its_error_output()
    {
        var error = Assert.ThrowsAny<Exception>(() => LocalVersionChecking.CheckResult(1, "", "cannot open display"));

        Assert.Contains("cannot open display", error.Message);
    }

    [Theory]
    [InlineData(Architecture.X64, true, Platform.Win64)]
    [InlineData(Architecture.X86, true, Platform.Win64)]
    [InlineData(Architecture.Arm64, true, Platform.Win64)]
    [InlineData(Architecture.X86, false, Platform.Win32)]
    public void Windows_is_win64_on_a_64_bit_system_and_win32_otherwise(Architecture architecture, bool is64Bit, Platform platform)
    {
        Assert.Equal(platform, MachineOSPlatform.Detect("Microsoft Windows 10.0.26300", architecture, is64Bit));
    }

    [Theory]
    [InlineData("Linux 6.8.0-1017-azure #20-Ubuntu SMP", Architecture.X64, Platform.Linux64)]
    [InlineData("Linux 6.8.0-1017-azure #20-Ubuntu SMP", Architecture.Arm64, Platform.LinuxArm64)]
    [InlineData("Darwin 24.0.0 Darwin Kernel Version 24.0.0", Architecture.X64, Platform.MacX64)]
    [InlineData("Darwin 24.0.0 Darwin Kernel Version 24.0.0", Architecture.Arm64, Platform.MacArm64)]
    public void Linux_and_macOS_follow_the_process_architecture(string osDescription, Architecture architecture, Platform platform)
    {
        Assert.Equal(platform, MachineOSPlatform.Detect(osDescription, architecture, true));
    }

    [Fact]
    public void An_unknown_system_throws()
    {
        Assert.ThrowsAny<Exception>(() => MachineOSPlatform.Detect("FreeBSD 14.1-RELEASE", Architecture.X64, true));
    }

    [Fact]
    public void LinuxArm64_has_its_Chrome_for_Testing_name()
    {
        Assert.Equal("linux-arm64", PlatformString.GetPlatformString(Platform.LinuxArm64));
    }
}
