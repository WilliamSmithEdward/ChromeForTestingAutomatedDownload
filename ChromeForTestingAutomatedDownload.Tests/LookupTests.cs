namespace ChromeForTestingAutomatedDownload.Tests;

public class LookupTests
{
    private static async Task<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel> Milestones()
    {
        using var client = new FakeHttpHandler().ServeFixture(Fixtures.Milestones).Client();
        return await ChromeVersionModelFactory
            .CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>(client, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task The_asset_list_holds_only_versions_with_the_binary_on_the_platform()
    {
        var model = await Milestones();

        var drivers = AssetList.FromModel(model, Binary.ChromeDriver, Platform.Win64);
        var chrome = AssetList.FromModel(model, Binary.Chrome, Platform.Win64);

        Assert.NotNull(drivers);
        Assert.Equal(new[] { "120.0.6099.109", "154.0.8037.92", "157.0.8083.0" }, drivers.Keys);
        Assert.NotNull(chrome);
        Assert.Contains("114.0.5735.133", chrome.Keys);
        Assert.Equal(Fixtures.DriverUrl("120.0.6099.109", "win64"), drivers["120.0.6099.109"]);
    }

    [Fact]
    public async Task An_unknown_binary_gives_null()
    {
        var model = await Milestones();

        Assert.Null(AssetList.FromModel(model, (Binary)99, Platform.Win64));
    }

    [Theory]
    [InlineData(Platform.Linux64, "linux64")]
    [InlineData(Platform.MacArm64, "mac-arm64")]
    [InlineData(Platform.MacX64, "mac-x64")]
    [InlineData(Platform.Win32, "win32")]
    [InlineData(Platform.Win64, "win64")]
    public void Each_platform_has_its_Chrome_for_Testing_name(Platform platform, string name)
    {
        Assert.Equal(name, PlatformString.GetPlatformString(platform));
    }

    [Theory]
    [InlineData("120.0.6099.109", 120)]
    [InlineData("154.0.8037.97", 154)]
    [InlineData("not a version", 0)]
    [InlineData("", 0)]
    public void The_major_release_is_the_number_before_the_first_dot(string version, int major)
    {
        Assert.Equal(major, new LocalVersion(version).MajorReleaseNumber);
    }
}
