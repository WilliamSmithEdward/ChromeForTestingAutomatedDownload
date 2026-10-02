using System.Runtime.InteropServices;

namespace ChromeForTestingAutomatedDownload.Tests;

/// <summary>
/// Detection and version checking throw exception types a caller can catch on their own, each
/// still an <see cref="Exception"/>, so code that catches Exception is unaffected.
/// </summary>
public class ExceptionTypeTests
{
    [Fact]
    public void No_Chrome_on_Windows_throws_FileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => LocalVersionChecking.FindWindowsChrome(_ => @"C:\Program Files", _ => false));
    }

    [Fact]
    public void A_failed_version_command_throws_InvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => LocalVersionChecking.CheckResult(1, "", "error"));
    }

    [Theory]
    [InlineData("Linux 6.8.0", Architecture.X86)]
    [InlineData("Darwin 24.0.0", Architecture.X86)]
    [InlineData("FreeBSD 14.1-RELEASE", Architecture.X64)]
    public void An_unsupported_system_or_processor_throws_PlatformNotSupportedException(string osDescription, Architecture architecture)
    {
        Assert.Throws<PlatformNotSupportedException>(() => MachineOSPlatform.Detect(osDescription, architecture, true));
    }
}
