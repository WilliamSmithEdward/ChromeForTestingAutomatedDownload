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

// For a given milestone and platform, with your own HttpClient, without looking
// at the local Chrome. Returns the full path of the extracted chromedriver.
using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
string driver = await AutomatedDownload.DownloadChromeDriverAsync(Platform.Linux64, 120, "/opt/chromedriver", httpClient);
```

`DownloadChromeDriverAsync` does this, in order:

1. Works out the platform with `MachineOSPlatform.GetPlatform()`, unless you pass one (see "Platform detection" below).
2. Reads the major version of the installed Chrome with `LocalVersionChecking.GetChromeVersion()` (see "Local Chrome version" below). Chrome must be installed even when you pass a platform, and its major version is used for that platform too.
3. Reads `latest-versions-per-milestone-with-downloads.json` and takes the chromedriver URL for that milestone and platform. The URL must be absolute HTTPS.
4. Downloads the ZIP file into a temporary file of its own, which is deleted afterwards, whether or not the rest succeeds.
5. Extracts the first entry named `chromedriver.exe` or `chromedriver` into the download folder, without its subfolder.

The methods without an `HttpClient` use one client the library shares (default 100 second timeout). The overload that takes a milestone skips steps 1 and 2 and sends both requests with the `HttpClient` you pass, which it does not dispose.

Things to know before you use it:

- The download folder defaults to `AppDomain.CurrentDomain.BaseDirectory`, the folder your application runs from. Pass a folder of your own to keep downloads out of it. The folder must already exist (`DirectoryNotFoundException` otherwise).
- Only chromedriver is written to the folder. An existing chromedriver is replaced; on Windows, replacing a chromedriver.exe that is still running fails with an `IOException`.
- Chrome for Testing publishes no checksums for its downloads, so nothing is verified beyond HTTPS.
- The ZIP entry is refused, with `InvalidDataException`, if its path is absolute, names a drive or climbs out of its folder with `..`. So is a download that is not a ZIP file, or one without a chromedriver entry.
- Chrome for Testing has chromedriver from milestone 115. For an older Chrome, or a milestone the endpoint does not list yet, the download throws `InvalidOperationException` naming the milestone and platform.
- A response that is not a success status throws `HttpRequestException` with the status code.

Only chromedriver has a download helper. For Chrome and chrome-headless-shell, look up the URL as below and download it with your own code.

---

## Look up a download URL

`LatestVersionsPerMilestoneWithDownload.ChromeVersionModel` has three lookup methods. Each answers from the model you call it on, without reading the endpoint again; the endpoint lists one version per milestone. Versions compare as numbers, part by part. Each returns `null` when nothing matches.

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

`AssetList.GetAssetListAsync<T>(binary, platform)` reads the endpoint itself and returns the whole map behind these methods: version string to URL, for every milestone that has the binary on the platform. `T` is `LatestVersionsPerMilestoneWithDownload.ChromeVersionModel`, the only model that implements `IDownload`.

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

`ChromeVersionModelFactory.CreateChromeVersionModelAsync<T>()` reads one endpoint and deserializes it into `T`. It throws `HttpRequestException` if the request fails and `JsonException` if the response cannot be read. The overload `CreateChromeVersionModelAsync<T>(httpClient, cancellationToken)` sends the request with your `HttpClient`, for your own timeout, proxy or handler; it accepts only the eight models below. Each model class sits inside a class named after its endpoint:

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

The models serialize back to JSON with `System.Text.Json`, under the endpoint's property names; their `QueryEndpointAsync` delegate is left out. The factory always uses a new instance's default delegate, so setting it on a model changes nothing the factory reads.

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
| `LinuxArm64` | `linux-arm64` (from milestone 153) |
| `MacArm64` | `mac-arm64` |
| `MacX64` | `mac-x64` |
| `Win32` | `win32` |
| `Win64` | `win64` |

### Platform detection

`MachineOSPlatform.GetPlatform()` returns:

- Windows: `Win64` on a 64-bit Windows and `Win32` on a 32-bit one. Chrome need not be installed, and where it is installed does not matter: the win64 chromedriver works with a 32-bit Chrome too.
- Linux: `Linux64` on x64 and `LinuxArm64` on Arm64, by the processor the .NET process runs on. Any other processor throws ("Unknown Linux architecture.").
- macOS: `MacX64` or `MacArm64`, by the processor the .NET process runs on.
- Anything else throws ("Unknown OS platform.").

### Local Chrome version

`LocalVersionChecking.GetChromeVersion()` returns a `LocalVersion` with `VersionString`, trimmed, and `MajorReleaseNumber` (0 if the version cannot be parsed).

- Windows: reads the file version of the first `chrome.exe` found under `Google\Chrome\Application` in `Program Files`, `Program Files (x86)` or the user's `AppData\Local`, in that order. Nothing is run. If there is none, it throws ("Google Chrome not found on the machine.").
- Linux: runs `google-chrome --product-version`, with `google-chrome` taken from the first absolute folder on `PATH` that has it. The application's folder, the current directory and relative `PATH` entries are never searched. Without one it throws.
- macOS: runs `/Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version`.
- A command that exits with a code other than 0 fails with what it wrote to standard error; warnings on standard error from a command that succeeded are ignored. A command still running after 30 seconds is stopped and fails.
- Nothing the library downloads is ever run.

```csharp
using ChromeForTestingAutomatedDownload;

var local = await LocalVersionChecking.GetChromeVersion();
Console.WriteLine($"{local.VersionString} (major {local.MajorReleaseNumber}) on {MachineOSPlatform.GetPlatform()}");
```

Errors from detection and version checking:

| Exception | When |
|---|---|
| `PlatformNotSupportedException` | The operating system, or the processor on Linux or macOS, is not one Chrome for Testing serves. |
| `FileNotFoundException` | Chrome is not installed where the library looks. |
| `InvalidOperationException` | Chrome's version could not be read: the command could not start, exited with a code other than 0, or timed out (a `TimeoutException` inside), or Windows reports no file version. |

All three are `Exception`s, so code that catches `Exception` still catches them. `DownloadChromeDriverAsync` without a milestone passes them on.

---

## License

MIT. See [LICENSE](https://github.com/WilliamSmithEdward/ChromeForTestingAutomatedDownload/blob/main/LICENSE).

Source and issues: https://github.com/WilliamSmithEdward/ChromeForTestingAutomatedDownload

## Attributions

- Icon "ChromeForTestingAutomatedDownload.png" designed by Reddit user u/warsponge: https://i.redd.it/nita41bof5481.png
- The local version checking logic started from the work of Niels Swimberghe: https://swimburger.net/blog/dotnet
