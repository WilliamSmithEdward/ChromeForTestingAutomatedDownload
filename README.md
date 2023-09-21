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
        var response = await new T().QueryEndpoint();

        var deserializedObject = JsonSerializer.Deserialize<T>(response);
        if (deserializedObject != null) return deserializedObject;

        throw new JsonException("Failed to deserialize endpoint.");
    }
}
```

### Example Usage

```csharp
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
