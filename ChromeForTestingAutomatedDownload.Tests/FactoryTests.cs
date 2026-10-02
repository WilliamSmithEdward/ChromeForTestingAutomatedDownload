using System.Text.Json;

namespace ChromeForTestingAutomatedDownload.Tests;

public class FactoryTests
{
    [Fact]
    public async Task The_HttpClient_overload_reads_the_models_endpoint()
    {
        var handler = new FakeHttpHandler().ServeFixture(Fixtures.Milestones);
        using var client = handler.Client();

        var model = await ChromeVersionModelFactory
            .CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>(client, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { new Uri(Fixtures.EndpointBase + Fixtures.Milestones) }, handler.Requests);
        Assert.Equal(new[] { "114", "120", "154", "157" }, model.Milestones.Keys);
        Assert.Equal("120.0.6099.109", model.Milestones["120"].Version);
        Assert.Equal("120", model.Milestones["120"].Channel);
        Assert.Equal(DateTimeKind.Utc, model.TimeStamp.Kind);
    }

    [Fact]
    public async Task The_HttpClient_overload_maps_every_model_to_its_endpoint()
    {
        var expected = new Dictionary<Type, string>
        {
            [typeof(KnownGoodVersions.ChromeVersionModel)] = "known-good-versions.json",
            [typeof(KnownGoodVersionsWithDownload.ChromeVersionModel)] = "known-good-versions-with-downloads.json",
            [typeof(LastKnownGoodVersions.ChromeVersionModel)] = "last-known-good-versions.json",
            [typeof(LastKnownGoodVersionsWithDownloads.ChromeVersionModel)] = "last-known-good-versions-with-downloads.json",
            [typeof(LatestPatchVersionsPerBuild.ChromeVersionModel)] = "latest-patch-versions-per-build.json",
            [typeof(LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel)] = "latest-patch-versions-per-build-with-downloads.json",
            [typeof(LatestVersionsPerMilestone.ChromeVersionModel)] = "latest-versions-per-milestone.json",
            [typeof(LatestVersionsPerMilestoneWithDownload.ChromeVersionModel)] = "latest-versions-per-milestone-with-downloads.json",
        };
        var handler = new FakeHttpHandler();
        foreach (var file in expected.Values) handler.Serve(Fixtures.EndpointBase + file, "{}");
        using var client = handler.Client();
        var token = TestContext.Current.CancellationToken;

        await ChromeVersionModelFactory.CreateChromeVersionModelAsync<KnownGoodVersions.ChromeVersionModel>(client, token);
        await ChromeVersionModelFactory.CreateChromeVersionModelAsync<KnownGoodVersionsWithDownload.ChromeVersionModel>(client, token);
        await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LastKnownGoodVersions.ChromeVersionModel>(client, token);
        await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LastKnownGoodVersionsWithDownloads.ChromeVersionModel>(client, token);
        await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestPatchVersionsPerBuild.ChromeVersionModel>(client, token);
        await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel>(client, token);
        await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestVersionsPerMilestone.ChromeVersionModel>(client, token);
        await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>(client, token);

        Assert.Equal(expected.Values.Select(f => Fixtures.EndpointBase + f), handler.Requests.Select(u => u.AbsoluteUri));
    }

    private sealed class OtherModel : IChromeVersionModel
    {
        public Func<Task<string>> QueryEndpointAsync { get; set; } = () => Task.FromResult("{}");
    }

    [Fact]
    public async Task The_HttpClient_overload_refuses_a_model_it_does_not_know()
    {
        var handler = new FakeHttpHandler();
        using var client = handler.Client();

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            ChromeVersionModelFactory.CreateChromeVersionModelAsync<OtherModel>(client, TestContext.Current.CancellationToken));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task The_HttpClient_overload_needs_a_client()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestVersionsPerMilestone.ChromeVersionModel>(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Malformed_JSON_throws_JsonException()
    {
        var handler = new FakeHttpHandler().Serve(Fixtures.EndpointBase + Fixtures.Milestones, "{ not json");
        using var client = handler.Client();

        await Assert.ThrowsAnyAsync<JsonException>(() =>
            ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>(client, TestContext.Current.CancellationToken));
    }
}
