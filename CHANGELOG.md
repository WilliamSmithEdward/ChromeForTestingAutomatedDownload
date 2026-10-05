# Changelog

Each release's notes. The Publish workflow takes the section for the
version it releases as the GitHub release's body, so a section is written
here before the version is tagged.

The sections up to 1.1.3 were gathered from the nuget.org version history,
with the UTC date the nuget.org catalog records for each upload. Neither
nuget.org nor the READMEs carried release notes for them. Versions 1.0.0 to
1.1.2 are unlisted on nuget.org; 1.1.3 is the listed version. All of them
target net7.0.

## [2.0.1] - 2026-10-04

* The NuGet package now embeds the root GitHub `README.md`, including its badges, as its only README. The OpenSSF Scorecard badge is served through `img.shields.io`, which NuGet supports.
* CI and Publish verify that the packaged README exactly matches the root file.
* No library API or runtime behavior changes.

## [2.0.0] - 2026-10-02

The chromedriver download no longer writes anything but chromedriver into its folder, refuses URLs that are not HTTPS and ZIP entries that leave the folder, and the Linux version check no longer runs a `google-chrome` from the application's folder or the current directory. Several of the fixes change what callers see, hence the major version.

### Breaking changes

* The package targets net8.0, net9.0 and net10.0. net7.0, which is out of support, is dropped.
* `DownloadChromeDriverAsync` downloads the ZIP file to a temporary file that is deleted afterwards. It used to save it in the download folder under the name the URL ends with, and leave it there.
* `DownloadChromeDriverAsync` refuses a chromedriver URL that is not absolute HTTPS, and a ZIP entry whose path is absolute, names a drive or contains `..`, with `InvalidDataException`. An entry like that used to be flattened to its file name.
* A download that is not a ZIP file, or a ZIP file without chromedriver, throws `InvalidDataException`; the second used to throw `InvalidOperationException`. A failed download throws `HttpRequestException` with the status code instead of a plain `Exception`.
* `MachineOSPlatform.GetPlatform()` on Windows returns `Win64` on 64-bit Windows and `Win32` on 32-bit Windows. It used to answer from where Chrome was installed, preferring Program Files (x86), so a 64-bit Windows with Chrome there got `Win32`, and a machine without Chrome threw.
* On Linux Arm64, `GetPlatform()` returns the new `Platform.LinuxArm64` instead of throwing.
* On Linux, `LocalVersionChecking.GetChromeVersion()` runs `google-chrome` only from an absolute folder on `PATH`, and throws `FileNotFoundException` when there is none. .NET used to look in the application's folder and the current directory first.
* `LocalVersion.VersionString` is trimmed; on Linux and macOS it used to end with a line break.
* Detection and the version check throw `FileNotFoundException`, `InvalidOperationException` and `PlatformNotSupportedException` where they threw plain `Exception`. All three are `Exception`s, so `catch (Exception)` still catches them.
* The three lookup methods of `LatestVersionsPerMilestoneWithDownload.ChromeVersionModel` answer from the model they are called on instead of reading the endpoint again, so an old model gives old answers.

### Fixes

* A Chrome installed only in Program Files (x86) was reported as not found, because the check looked at the Program Files path twice. Chrome is found in Program Files, Program Files (x86) and the user's AppData\Local.
* The Linux version check failed whenever Chrome wrote anything to standard error, even with exit code 0. The exit code decides now. Both output streams are read at once, and a command still running after 30 seconds is stopped.
* A milestone without chromedriver (older than 115, or not listed yet) throws `InvalidOperationException` naming the milestone and platform, instead of passing a null URL to `HttpClient`.
* Versions were ordered as text, so 999.x sorted above 1000.x and 120.0.6099.99 above 120.0.6099.109. They are compared as numbers.
* The models could not be serialized with `System.Text.Json`, which threw on the `QueryEndpointAsync` delegate. The delegate is now left out.
* The download is streamed and its response disposed, and the overloads without an `HttpClient` share one client instead of creating one per call.
* The build is deterministic, so the same commit gives the same dll.

### Additions

* `ChromeVersionModelFactory.CreateChromeVersionModelAsync<T>(HttpClient, CancellationToken)` reads a model's endpoint with your own `HttpClient`.
* `AutomatedDownload.DownloadChromeDriverAsync(Platform, int majorReleaseNumber, string downloadPath, HttpClient, CancellationToken)` downloads the chromedriver for a given milestone with your own `HttpClient`, without looking at the local Chrome, and returns the extracted driver's path.
* `Platform.LinuxArm64` ("linux-arm64"), which Chrome for Testing publishes from milestone 153.
* The package ships its XML documentation, and the READMEs are rewritten against the code.
* The package is built in CI from the tagged commit, tested on all three frameworks, scanned for vulnerabilities and malware, and published through nuget.org trusted publishing. The GitHub release carries the package's signed build provenance.

## [1.1.3] - 2023-10-03

No notes were recorded.

## [1.1.2] - 2023-10-02

No notes were recorded.

## [1.1.1] - 2023-10-01

No notes were recorded.

## [1.1.0] - 2023-10-01

No notes were recorded.

## [1.0.8] - 2023-09-28

No notes were recorded.

## [1.0.7] - 2023-09-22

No notes were recorded.

## [1.0.6] - 2023-09-22

No notes were recorded.

## [1.0.5] - 2023-09-22

No notes were recorded.

## [1.0.4] - 2023-09-21

No notes were recorded.

## [1.0.3] - 2023-09-21

No notes were recorded.

## [1.0.2] - 2023-09-21

No notes were recorded.

## [1.0.1] - 2023-09-21

No notes were recorded.

## [1.0.0] - 2023-08-28

No notes were recorded.
