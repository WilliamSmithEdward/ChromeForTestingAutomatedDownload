using System.Net;

namespace ChromeForTestingAutomatedDownload.Tests;

[Collection("downloads")]
public class DownloadTests
{
    private const string Version120 = "120.0.6099.109";

    private static FakeHttpHandler Serving(string platform, byte[] zip) =>
        new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version120, platform), zip);

    [Fact]
    public async Task Extracts_chromedriver_exe_for_the_milestone_and_platform()
    {
        using var folder = new TempFolder();
        var zip = Fixtures.Zip(
            ("chromedriver-win64/LICENSE.chromedriver", "license"),
            ("chromedriver-win64/chromedriver.exe", "driver for win64"));
        var handler = Serving("win64", zip);
        using var client = handler.Client();

        var path = await AutomatedDownload.DownloadChromeDriverAsync(
            Platform.Win64, 120, folder.Download, client, TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(folder.Download, "chromedriver.exe"), path);
        Assert.Equal("driver for win64", File.ReadAllText(path));
        Assert.Equal(
            new[] { Fixtures.EndpointBase + Fixtures.Milestones, Fixtures.DriverUrl(Version120, "win64") },
            handler.Requests.Select(u => u.AbsoluteUri));
    }

    [Fact]
    public async Task Extracts_chromedriver_without_extension_on_Linux()
    {
        using var folder = new TempFolder();
        var zip = Fixtures.Zip(("chromedriver-linux64/chromedriver", "driver for linux64"));
        using var client = Serving("linux64", zip).Client();

        var path = await AutomatedDownload.DownloadChromeDriverAsync(
            Platform.Linux64, 120, folder.Download, client, TestContext.Current.CancellationToken);

        Assert.Equal(Path.Combine(folder.Download, "chromedriver"), path);
        Assert.Equal("driver for linux64", File.ReadAllText(path));
    }

    [Fact]
    public async Task Replaces_an_existing_chromedriver()
    {
        using var folder = new TempFolder();
        File.WriteAllText(Path.Combine(folder.Download, "chromedriver.exe"), "old driver");
        var zip = Fixtures.Zip(("chromedriver-win64/chromedriver.exe", "new driver"));
        using var client = Serving("win64", zip).Client();

        var path = await AutomatedDownload.DownloadChromeDriverAsync(
            Platform.Win64, 120, folder.Download, client, TestContext.Current.CancellationToken);

        Assert.Equal("new driver", File.ReadAllText(path));
    }

    [Fact]
    public async Task A_failed_download_throws()
    {
        using var folder = new TempFolder();
        var handler = new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version120, "win64"), Array.Empty<byte>(), HttpStatusCode.NotFound);
        using var client = handler.Client();

        await Assert.ThrowsAnyAsync<Exception>(() => AutomatedDownload.DownloadChromeDriverAsync(
            Platform.Win64, 120, folder.Download, client, TestContext.Current.CancellationToken));
        Assert.False(File.Exists(Path.Combine(folder.Download, "chromedriver.exe")));
    }

    [Fact]
    public async Task The_HttpClient_overload_needs_a_client()
    {
        using var folder = new TempFolder();

        await Assert.ThrowsAsync<ArgumentNullException>(() => AutomatedDownload.DownloadChromeDriverAsync(
            Platform.Win64, 120, folder.Download, null!, TestContext.Current.CancellationToken));
    }
}
