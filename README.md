# ChromeForTestingAutomatedDownload

ChromeForTestingAutomatedDownload reads the JSON endpoints that Google publishes for [Chrome for Testing](https://github.com/GoogleChromeLabs/chrome-for-testing) and turns them into C# objects. From those you can look up the download URL of Chrome, chromedriver or chrome-headless-shell for a version and platform. One helper goes further and downloads the chromedriver that matches the Chrome installed on the machine.

```
dotnet add package ChromeForTestingAutomatedDownload
```

Everything is in the `ChromeForTestingAutomatedDownload` namespace, and every network call is async.

---

## Download the chromedriver that matches the installed Chrome

```csharp
using ChromeForTestingAutomatedDownload;

// Detects the platform, reads the local Chrome version and writes chromedriver
// (chromedriver.exe on Windows) to the application's base directory.
await AutomatedDownload.DownloadChromeDriverAsync();

// The same, into a folder of your choice.
await AutomatedDownload.DownloadChromeDriverAsync(@"C:\tools\chromedriver");

// For a given platform. The version still comes from the Chrome installed here.
await AutomatedDownload.DownloadChromeDriverAsync(Platform.Win64, @"C:\tools\chromedriver");
```

`DownloadChromeDriverAsync` does this, in order:

1. Works out the platform with `MachineOSPlatform.GetPlatform()`, unless you pass one (see "Platform detection" below).
2. Reads the major version of the installed Chrome with `LocalVersionChecking.GetChromeVersion()` (see "Local Chrome version" below). Chrome must be installed even when you pass a platform, and its major version is used for that platform too.
3. Reads `latest-versions-per-milestone-with-downloads.json` and takes the chromedriver URL for that milestone and platform.
4. Downloads the ZIP file with a new `HttpClient` (default 100 second timeout) and saves it in the download folder under the file name at the end of the URL, for example `chromedriver-win64.zip`.
5. Extracts the first entry named `chromedriver.exe` or `chromedriver` into the same folder, without its subfolder.

Things to know before you use it:

- The download folder defaults to `AppDomain.CurrentDomain.BaseDirectory`, the folder your application runs from. Pass a folder of your own to keep downloads out of it. The folder must already exist.
- An existing file with the ZIP's name is replaced, and so is an existing chromedriver. On Windows, replacing a chromedriver.exe that is still running fails with an `IOException`.
- The ZIP file stays in the folder after extraction. Delete it yourself if you do not want it.
- Nothing is verified beyond HTTPS: Chrome for Testing publishes no checksums, and the library does not check the URL's host or scheme, or the file name it takes from the URL.
- Chrome for Testing has chromedriver from milestone 115. For an older Chrome, or a milestone the endpoint does not list yet, there is no URL and the download fails with an `InvalidOperationException` ("An invalid request URI was provided").
- A response that is not a success status throws `Exception` ("Failed to download file. Status code: ..."). A ZIP without a chromedriver entry throws `InvalidOperationException`.

Only chromedriver has a download helper. For Chrome and chrome-headless-shell, look up the URL as below and download it with your own code.

---

## Look up a download URL

`LatestVersionsPerMilestoneWithDownload.ChromeVersionModel` has three lookup methods. Each reads `latest-versions-per-milestone-with-downloads.json` again, which lists one version per milestone, and returns `null` when nothing matches.

| Method | Returns |
|---|---|
| `GetMostRecentAssetURLAsync(binary, platform)` | The URL from the highest milestone listed. That is usually a Canary or Dev build, not Stable. |
| `GetMostRecentAssetURLByMajorReleaseNumberAsync(binary, platform, major)` | The URL for that milestone. |
| `GetAssetURLByFullVersionNumberAsync(binary, platform, version)` | The URL for that exact version, but only if it is the version the milestone endpoint lists for its milestone. Other versions return `null`. |

`binary` is `Binary.Chrome`, `Binary.ChromeDriver` or `Binary.ChromeHeadlessShell`. `platform` is one of the values in "Platforms" below.

```csharp
using ChromeForTestingAutomatedDownload;

var model = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

// Newest milestone, often Canary.
Console.WriteLine(await model.GetMostRecentAssetURLAsync(Binary.ChromeDriver, Platform.MacX64));

// Milestone 118.
Console.WriteLine(await model.GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary.ChromeDriver, Platform.Win64, 118));

// The exact version the endpoint lists for milestone 120.
Console.WriteLine(await model.GetAssetURLByFullVersionNumberAsync(Binary.ChromeHeadlessShell, Platform.Linux64, "120.0.6099.109"));
```

The output looks like this (the first line changes as new milestones appear):

```
https://storage.googleapis.com/chrome-for-testing-public/157.0.8083.0/mac-x64/chromedriver-mac-x64.zip
https://storage.googleapis.com/chrome-for-testing-public/118.0.5993.70/win64/chromedriver-win64.zip
https://storage.googleapis.com/chrome-for-testing-public/120.0.6099.109/linux64/chrome-headless-shell-linux64.zip
```

`AssetList.GetAssetListAsync<T>(binary, platform)` returns the whole map behind these methods: version string to URL, for every milestone that has the binary on the platform. `T` is `LatestVersionsPerMilestoneWithDownload.ChromeVersionModel`, the only model that implements `IDownload`.

### The current Stable chromedriver

The lookup methods do not know about channels. `last-known-good-versions-with-downloads.json` does:

```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LastKnownGoodVersionsWithDownloads.ChromeVersionModel>();

var url = result.Channels.Stable.Downloads.ChromeDriver
    .First(x => x.Platform == "win64")
    .Url;

Console.WriteLine($"{result.Channels.Stable.Version}: {url}");
```

`Channels` also has `Beta`, `Dev` and `Canary`.

### Every chromedriver URL for one platform

```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel>();

var urls = result.Builds.Values
    .SelectMany(x => x.Downloads.ChromeDriver)
    .Where(x => x.Platform == "win64")
    .Select(x => x.Url);

foreach (var url in urls)
{
    Console.WriteLine(url);
}
```

### Every chromedriver URL per milestone

```csharp
using ChromeForTestingAutomatedDownload;

var result = await ChromeVersionModelFactory
    .CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

foreach (var (milestone, entry) in result.Milestones)
{
    foreach (var download in entry.Downloads.ChromeDriver)
    {
        Console.WriteLine($"{milestone} {download.Platform} {download.Url}");
    }
}
```

---

## The models

`ChromeVersionModelFactory.CreateChromeVersionModelAsync<T>()` reads one endpoint and deserializes it into `T`. It throws `HttpRequestException` if the request fails and `JsonException` if the response cannot be read. Each model class sits inside a class named after its endpoint:

| Model | Endpoint under `https://googlechromelabs.github.io/chrome-for-testing/` | Main property |
|---|---|---|
| `KnownGoodVersions.ChromeVersionModel` | `known-good-versions.json` | `Versions` (version, revision) |
| `KnownGoodVersionsWithDownload.ChromeVersionModel` | `known-good-versions-with-downloads.json` | `Versions`, each with `Downloads` |
| `LastKnownGoodVersions.ChromeVersionModel` | `last-known-good-versions.json` | `Channels` (`Stable`, `Beta`, `Dev`, `Canary`) |
| `LastKnownGoodVersionsWithDownloads.ChromeVersionModel` | `last-known-good-versions-with-downloads.json` | `Channels`, each with `Downloads` |
| `LatestPatchVersionsPerBuild.ChromeVersionModel` | `latest-patch-versions-per-build.json` | `Builds`, keyed by build such as `113.0.5672` |
| `LatestPatchVersionsPerBuildWithDownloads.ChromeVersionModel` | `latest-patch-versions-per-build-with-downloads.json` | `Builds`, each with `Downloads` |
| `LatestVersionsPerMilestone.ChromeVersionModel` | `latest-versions-per-milestone.json` | `Milestones`, keyed by milestone such as `120` |
| `LatestVersionsPerMilestoneWithDownload.ChromeVersionModel` | `latest-versions-per-milestone-with-downloads.json` | `Milestones`, each with `Downloads`; the lookup methods above |

Every model also has `TimeStamp`, the time the endpoint was generated, in UTC. A `Downloads` object has `Chrome`, `ChromeDriver` and `ChromeHeadlessShell` lists of `Platform` and `Url` pairs; older versions have empty lists for the binaries Chrome for Testing did not publish then (chromedriver starts at milestone 115, chrome-headless-shell at 120). In the two milestone models, the `Channel` property holds the milestone number (`"120"`), not a channel name.

The model classes hold a `QueryEndpointAsync` delegate, so `System.Text.Json` cannot serialize them back to JSON (it throws `NotSupportedException`). The factory always uses a new instance's default delegate.

To get an endpoint's JSON as a string, call `GoogleChromeLabsEndpointQueries` directly:

```csharp
using ChromeForTestingAutomatedDownload;

string json = await GoogleChromeLabsEndpointQueries.GetLastKnownGoodVersionsAsync();
Console.WriteLine(json);
```

It has one method per endpoint: `GetKnownGoodVersionsAsync`, `GetKnownGoodVersionsWithDownloadAsync`, `GetLastKnownGoodVersionsAsync`, `GetLastKnownGoodVersionsWithDownloadAsync`, `GetLatestPatchVersionsPerBuildAsync`, `GetLatestPatchVersionsPerBuildAsyncWithDownloads`, `GetLatestVersionsPerMilestoneAsync` and `GetLatestVersionsPerMilestoneWithDownloadAsync`. They share one `HttpClient` with the default 100 second timeout.

---

## Platforms

| `Platform` | Chrome for Testing name |
|---|---|
| `Linux64` | `linux64` |
| `MacArm64` | `mac-arm64` |
| `MacX64` | `mac-x64` |
| `Win32` | `win32` |
| `Win64` | `win64` |

Chrome for Testing also publishes `linux-arm64` from milestone 153. There is no `Platform` value for it, so the lookup methods cannot return those URLs; read them from the models.

### Platform detection

`MachineOSPlatform.GetPlatform()` returns:

- Windows: `Win32` if `chrome.exe` is in `Program Files (x86)\Google\Chrome\Application`, otherwise `Win64` if it is in `Program Files\Google\Chrome\Application`, otherwise it throws ("Google Chrome not found on the machine."). The answer depends on where Chrome is installed, not on the processor. A per-user install under `AppData` is not found.
- Linux: `Linux64` on x64. Any other processor throws ("Unknown Linux architecture.").
- macOS: `MacX64` or `MacArm64`, by the processor the .NET process runs on.
- Anything else throws ("Unknown OS platform.").

### Local Chrome version

`LocalVersionChecking.GetChromeVersion()` returns a `LocalVersion` with `VersionString` and `MajorReleaseNumber` (0 if the version cannot be parsed).

- Windows: reads the file version of `chrome.exe` in `Program Files\Google\Chrome\Application`. Nothing is run. A Chrome installed only in `Program Files (x86)` is reported as not found ("Google Chrome not found on the machine.").
- Linux: runs `google-chrome --product-version`. .NET looks for `google-chrome` in the application's folder first, then the current directory, then `PATH`. Any output on standard error is treated as a failure.
- macOS: runs `/Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version`.
- Neither command has a timeout.

```csharp
using ChromeForTestingAutomatedDownload;

var local = await LocalVersionChecking.GetChromeVersion();
Console.WriteLine($"{local.VersionString} (major {local.MajorReleaseNumber}) on {MachineOSPlatform.GetPlatform()}");
```

Errors from detection and version checking are thrown as `Exception` or, on an unsupported operating system, `PlatformNotSupportedException`.

---

## License

MIT. See [LICENSE](https://github.com/WilliamSmithEdward/ChromeForTestingAutomatedDownload/blob/main/LICENSE).

Source and issues: https://github.com/WilliamSmithEdward/ChromeForTestingAutomatedDownload

## Attributions

- Icon "ChromeForTestingAutomatedDownload.png" designed by Reddit user u/warsponge: https://i.redd.it/nita41bof5481.png
- The local version checking logic started from the work of Niels Swimberghe: https://swimburger.net/blog/dotnet
