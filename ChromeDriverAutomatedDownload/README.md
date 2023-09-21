# ChromeForTestingAutomatedDownload

Support library that interfaces with the JSON endpoints included in https://github.com/GoogleChromeLabs/chrome-for-testing

### Chrome Version Models

* KnownGoodVersions.ChromeVersionModel
* KnownGoodVersionsWithDownloads.ChromeVersionModel
* LastKnownGoodVersion.ChromeVersionModel
* LastKnownGoodVersionWithDownloads.ChromeVersionModel
* LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel
* LatestVersionsPerMilestone.ChromeVersionModel
* LatestVersionsPerMilestoneWithDownloads.ChromeVersionModel

### Chrome Version Model Factory

```csharp
public static class ChromeVersionModelFactory
{
    public static async Task<T> CreateChromeVersionModel<T>() where T : IChromeVersionModel, new()
    {
        var response = await new T().QueryEndpointAsync();

        var deserializedObject = JsonSerializer.Deserialize<T>(response);
        if (deserializedObject != null) return deserializedObject;

        throw new JsonException("Failed to deserialize endpoint.");
    }
}
```

### Example Usage

```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModel<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

var url = await result.GetMostRecentAssetURL(Binary.ChromeDriver, Platform.MacX64);

Console.WriteLine(url);
```

```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModel<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

var url = await result
    .GetMostRecentAssetURLByMajorReleaseNumber(Binary.ChromeDriver, Platform.Win64, 118);

Console.WriteLine(url);
```

```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModel<LastKnownGoodVersionsWithDownloads.ChromeVersionModel>();

var downloadURL = result
    .Channels
    .Stable
    .Downloads
    .ChromeDriver
    .Where(x => x.Platform.Equals("win64"))
    .First()
    .Url;

Console.WriteLine(downloadURL);
```

```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModel<LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel>();

var builds = result.Builds.Values;

var downloads = builds
    .Select(x => x.Downloads);

var chromeDriverDownloads = download
    .SelectMany(x => x.ChromeDriver);

var chromeDriverURLs = chromeDriverDownloads
    .Where(x => x.Platform.Equals("win64") && string.IsNullOrEmpty(x.Url) == false)
    .Select(x => x.Url);

foreach (var url in chromeDriverURLs)
{
    Console.WriteLine(url);
}
```

```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModel<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

var chromeDriverDownlods = result
    .Milestones
    .Values
    .Select(x => x.Downloads)
    .SelectMany(x => x.ChromeDriver)
    .ToList();

foreach (var item in chromeDriverDownlods)
{
    Console.WriteLine(item.Platform + " " + item.Url);
}
```