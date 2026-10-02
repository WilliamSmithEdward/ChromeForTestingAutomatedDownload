using System.Text.Json;

namespace ChromeForTestingAutomatedDownload.Tests;

/// <summary>
/// The lookup methods answer from the model they are called on, order versions as numbers, and
/// the models serialize.
/// </summary>
public class InstanceLookupTests
{
    private static async Task<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel> Milestones()
    {
        using var client = new FakeHttpHandler().ServeFixture(Fixtures.Milestones).Client();
        return await ChromeVersionModelFactory
            .CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>(client, TestContext.Current.CancellationToken);
    }

    private static LatestVersionsPerMilestoneWithDownload.Milestones Milestone(string milestone, string version) => new()
    {
        Channel = milestone,
        Version = version,
        Downloads = new DownloadMetaData
        {
            ChromeDriver = { new PlatformMetaData { Platform = "win64", Url = $"https://example.test/{version}/chromedriver-win64.zip" } },
        },
    };

    [Fact]
    public async Task The_most_recent_URL_comes_from_the_model_it_is_called_on()
    {
        var model = await Milestones();
        model.Milestones.Remove("157");

        var url = await model.GetMostRecentAssetURLAsync(Binary.ChromeDriver, Platform.Win64);

        Assert.Equal(Fixtures.DriverUrl("154.0.8037.92", "win64"), url);
    }

    [Fact]
    public async Task The_URL_by_milestone_comes_from_the_model_it_is_called_on()
    {
        var model = await Milestones();
        model.Milestones["120"] = Milestone("120", "120.0.0.1");

        var url = await model.GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary.ChromeDriver, Platform.Win64, 120);

        Assert.Equal("https://example.test/120.0.0.1/chromedriver-win64.zip", url);
    }

    [Fact]
    public async Task The_URL_by_full_version_comes_from_the_model_it_is_called_on()
    {
        var model = await Milestones();
        model.Milestones["120"] = Milestone("120", "120.0.0.1");

        Assert.Equal("https://example.test/120.0.0.1/chromedriver-win64.zip",
            await model.GetAssetURLByFullVersionNumberAsync(Binary.ChromeDriver, Platform.Win64, "120.0.0.1"));
        Assert.Null(await model.GetAssetURLByFullVersionNumberAsync(Binary.ChromeDriver, Platform.Win64, "120.0.6099.109"));
    }

    [Fact]
    public async Task Versions_are_ordered_as_numbers_not_as_text()
    {
        var model = new LatestVersionsPerMilestoneWithDownload.ChromeVersionModel();
        model.Milestones["999"] = Milestone("999", "999.0.9999.99");
        model.Milestones["1000"] = Milestone("1000", "1000.0.1.0");

        var url = await model.GetMostRecentAssetURLAsync(Binary.ChromeDriver, Platform.Win64);

        Assert.Equal("https://example.test/1000.0.1.0/chromedriver-win64.zip", url);
    }

    [Fact]
    public async Task Patch_numbers_are_ordered_as_numbers_within_a_milestone()
    {
        var model = new LatestVersionsPerMilestoneWithDownload.ChromeVersionModel();
        model.Milestones["a"] = Milestone("120", "120.0.6099.99");
        model.Milestones["b"] = Milestone("120", "120.0.6099.109");

        var url = await model.GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary.ChromeDriver, Platform.Win64, 120);

        Assert.Equal("https://example.test/120.0.6099.109/chromedriver-win64.zip", url);
    }

    [Fact]
    public async Task The_lookups_return_null_when_nothing_matches()
    {
        var model = await Milestones();

        Assert.Null(await model.GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary.ChromeDriver, Platform.Win64, 114));
        Assert.Null(await model.GetMostRecentAssetURLAsync(Binary.ChromeDriver, (Platform)99));
        Assert.Null(await model.GetMostRecentAssetURLAsync((Binary)99, Platform.Win64));
    }

    [Fact]
    public async Task A_model_serializes_back_to_JSON()
    {
        var model = await Milestones();

        var json = JsonSerializer.Serialize(model);
        var copy = JsonSerializer.Deserialize<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>(json);

        Assert.DoesNotContain("QueryEndpointAsync", json);
        Assert.NotNull(copy);
        Assert.Equal(model.Milestones.Keys, copy.Milestones.Keys);
        Assert.Equal(model.Milestones["120"].Downloads.ChromeDriver.Count, copy.Milestones["120"].Downloads.ChromeDriver.Count);
    }

    [Fact]
    public void Every_model_serializes()
    {
        object[] models =
        {
            new KnownGoodVersions.ChromeVersionModel(),
            new KnownGoodVersionsWithDownload.ChromeVersionModel(),
            new LastKnownGoodVersions.ChromeVersionModel(),
            new LastKnownGoodVersionsWithDownloads.ChromeVersionModel(),
            new LatestPatchVersionsPerBuild.ChromeVersionModel(),
            new LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel(),
            new LatestVersionsPerMilestone.ChromeVersionModel(),
            new LatestVersionsPerMilestoneWithDownload.ChromeVersionModel(),
        };

        foreach (var model in models)
        {
            Assert.DoesNotContain("QueryEndpointAsync", JsonSerializer.Serialize(model, model.GetType()));
        }
    }
}
