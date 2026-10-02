using System.IO.Compression;
using System.Net;

namespace ChromeForTestingAutomatedDownload.Tests;

/// <summary>
/// What the download writes, and what it refuses, when the JSON or the ZIP file is not what
/// Chrome for Testing serves.
/// </summary>
[Collection("downloads")]
public class DownloadSafetyTests
{
    private const string Version = "120.0.6099.109";

    /// <summary>The milestone endpoint with one milestone whose win64 chromedriver is at <paramref name="url"/>.</summary>
    private static string MilestonesWithDriverAt(string url) =>
        """{"timestamp":"2026-10-02T09:24:10.666Z","milestones":{"120":{"milestone":"120","version":"""
        + $"\"{Version}\""
        + ""","revision":"1217362","downloads":{"chrome":[],"chromedriver":[{"platform":"win64","url":"""
        + $"\"{url}\""
        + """}],"chrome-headless-shell":[]}}}}""";

    private static FakeHttpHandler Serving(string driverUrl, byte[] zip) =>
        new FakeHttpHandler()
            .Serve(Fixtures.EndpointBase + Fixtures.Milestones, MilestonesWithDriverAt(driverUrl))
            .Serve(driverUrl, zip);

    private static Task<string> Download(HttpClient client, string folder, int milestone = 120) =>
        AutomatedDownload.DownloadChromeDriverAsync(Platform.Win64, milestone, folder, client, TestContext.Current.CancellationToken);

    private static byte[] DriverZip => Fixtures.Zip(
        ("chromedriver-win64/LICENSE.chromedriver", "license"),
        ("chromedriver-win64/chromedriver.exe", "driver"));

    [Fact]
    public async Task Only_chromedriver_is_written_and_the_ZIP_file_is_not_left_behind()
    {
        using var folder = new TempFolder();
        using var client = new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version, "win64"), DriverZip)
            .Client();

        await Download(client, folder.Download);

        Assert.Equal(new[] { "download/chromedriver.exe" }, folder.Files());
    }

    [Fact]
    public async Task The_file_name_in_the_URL_does_not_decide_what_is_written()
    {
        using var folder = new TempFolder();
        var url = "https://storage.googleapis.com/chrome-for-testing-public/120.0.6099.109/win64/ChromeForTestingAutomatedDownload.dll";
        File.WriteAllText(Path.Combine(folder.Download, "ChromeForTestingAutomatedDownload.dll"), "the application's own file");
        using var client = Serving(url, DriverZip).Client();

        await Download(client, folder.Download);

        Assert.Equal("the application's own file", File.ReadAllText(Path.Combine(folder.Download, "ChromeForTestingAutomatedDownload.dll")));
        Assert.Equal(new[] { "download/ChromeForTestingAutomatedDownload.dll", "download/chromedriver.exe" }, folder.Files());
    }

    [Theory]
    [InlineData("http://storage.googleapis.com/chrome-for-testing-public/120.0.6099.109/win64/chromedriver-win64.zip")]
    [InlineData("ftp://storage.googleapis.com/chromedriver-win64.zip")]
    [InlineData("chromedriver-win64.zip")]
    public async Task A_download_URL_that_is_not_absolute_HTTPS_is_refused(string url)
    {
        using var folder = new TempFolder();
        var handler = new FakeHttpHandler().Serve(Fixtures.EndpointBase + Fixtures.Milestones, MilestonesWithDriverAt(url));
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute)) handler.Serve(url, DriverZip);
        using var client = handler.Client();

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => Download(client, folder.Download));

        Assert.Contains(url, error.Message);
        Assert.Single(handler.Requests);
        Assert.Empty(folder.Files());
    }

    [Theory]
    [InlineData("../chromedriver.exe")]
    [InlineData("chromedriver-win64/../../chromedriver.exe")]
    [InlineData("/chromedriver.exe")]
    [InlineData("C:/chromedriver.exe")]
    [InlineData("..\\chromedriver.exe")]
    public async Task A_chromedriver_entry_that_leaves_the_folder_is_refused(string entryName)
    {
        using var folder = new TempFolder();
        using var client = new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version, "win64"), Fixtures.Zip((entryName, "driver")))
            .Client();

        await Assert.ThrowsAsync<InvalidDataException>(() => Download(client, folder.Download));

        Assert.Empty(folder.Files());
    }

    [Fact]
    public async Task A_ZIP_file_without_chromedriver_throws_InvalidDataException()
    {
        using var folder = new TempFolder();
        using var client = new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version, "win64"), Fixtures.Zip(("chromedriver-win64/LICENSE.chromedriver", "license")))
            .Client();

        var error = await Assert.ThrowsAsync<InvalidDataException>(() => Download(client, folder.Download));

        Assert.Contains("chromedriver", error.Message);
        Assert.Empty(folder.Files());
    }

    [Fact]
    public async Task A_download_that_is_not_a_ZIP_file_writes_nothing()
    {
        using var folder = new TempFolder();
        using var client = new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version, "win64"), "<html>not a zip</html>"u8.ToArray())
            .Client();

        await Assert.ThrowsAsync<InvalidDataException>(() => Download(client, folder.Download));

        Assert.Empty(folder.Files());
    }

    [Fact]
    public async Task A_milestone_without_chromedriver_says_which_milestone_and_platform()
    {
        using var folder = new TempFolder();
        var handler = new FakeHttpHandler().ServeFixture(Fixtures.Milestones);
        using var client = handler.Client();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Download(client, folder.Download, milestone: 114));

        Assert.Contains("114", error.Message);
        Assert.Contains("win64", error.Message);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_failed_download_throws_HttpRequestException_with_the_status()
    {
        using var folder = new TempFolder();
        using var client = new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version, "win64"), Array.Empty<byte>(), HttpStatusCode.NotFound)
            .Client();

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => Download(client, folder.Download));

        Assert.Equal(HttpStatusCode.NotFound, error.StatusCode);
        Assert.Empty(folder.Files());
    }

    [Fact]
    public async Task The_temporary_ZIP_file_is_deleted_even_when_extraction_fails()
    {
        using var folder = new TempFolder();
        using var client = new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version, "win64"), Fixtures.Zip(("readme.txt", "no driver")))
            .Client();
        // A temporary folder of the test's own, since test processes for the other target
        // frameworks download into the shared system temp folder at the same time.
        var temporary = Path.Combine(folder.Root, "tmp");
        Directory.CreateDirectory(temporary);

        await Assert.ThrowsAsync<InvalidDataException>(() => AutomatedDownload.DownloadChromeDriverAsync(
            Platform.Win64, 120, folder.Download, client, temporary, TestContext.Current.CancellationToken));

        Assert.Empty(Directory.GetFiles(temporary));
        Assert.Empty(Directory.GetFiles(folder.Download));
    }

    [Fact]
    public async Task The_temporary_ZIP_file_is_deleted_after_a_download()
    {
        using var folder = new TempFolder();
        using var client = new FakeHttpHandler()
            .ServeFixture(Fixtures.Milestones)
            .Serve(Fixtures.DriverUrl(Version, "win64"), DriverZip)
            .Client();
        var temporary = Path.Combine(folder.Root, "tmp");
        Directory.CreateDirectory(temporary);

        await AutomatedDownload.DownloadChromeDriverAsync(
            Platform.Win64, 120, folder.Download, client, temporary, TestContext.Current.CancellationToken);

        Assert.Empty(Directory.GetFiles(temporary));
    }
}
