# ChromeForTestingAutomatedDownload

Support library that interfaces with the JSON endpoints included in https://github.com/GoogleChromeLabs/chrome-for-testing

### Attributions

* Foundation of local version checking logic credited to Niels Swimberghe https://swimburger.net/blog/dotnet

### Chrome Version Models

* KnownGoodVersions.ChromeVersionModel
* KnownGoodVersionsWithDownloads.ChromeVersionModel
* LastKnownGoodVersion.ChromeVersionModel
* LastKnownGoodVersionWithDownloads.ChromeVersionModel
* LatestPatchVersionsPerBuild.ChromeVersionModel
* LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel
* LatestVersionsPerMilestone.ChromeVersionModel
* LatestVersionsPerMilestoneWithDownloads.ChromeVersionModel

### Chrome Version Model Factory

```csharp
public static class ChromeVersionModelFactory
{
    public static async Task<T> CreateChromeVersionModelAsync<T>() where T : IChromeVersionModel, new()
    {
        var response = await new T().QueryEndpointAsync();

        var deserializedObject = JsonSerializer.Deserialize<T>(response);
        if (deserializedObject != null) return deserializedObject;

        throw new JsonException("Failed to deserialize endpoint.");
    }
}
```

### Example Usage

#### Download the Latest Version of ChromeDriver that Matches the Major Release Version of Chrome Installed on the Machine
Note: For windows, the logic will default to downloading the binary that matches the architecture of Chrome installed on the machine, with x64 taking priority.
```csharp
using ChromeForTestingAutomatedDownload;

//Will download chrome driver with platform matching the OS of the machine the .exe is running on
await AutomatedDownload.DownloadChromeDriverAsync();

//Download for a specific platform
await AutomatedDownload.DownloadChromeDriverAsync(Platform.Linux64);
```

#### Get the URL of the Most Recent Asset
```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

var url = await result.GetMostRecentAssetURLAsync(Binary.ChromeDriver, Platform.MacX64);

Console.WriteLine(url);
```

#### Get the URL of the Most Recent Asset by Major Release Number
```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

var url = await result
    .GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary.ChromeDriver, Platform.Win64, 118);

Console.WriteLine(url);
```

#### Get a Specific Download URL by Filtering with LINQ
```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LastKnownGoodVersionsWithDownloads.ChromeVersionModel>();

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

#### Get a List of Last Known Good Versions by Binary Type / Platform
```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel>();

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

#### Get a List of All Milestones Download URLs
```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

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
