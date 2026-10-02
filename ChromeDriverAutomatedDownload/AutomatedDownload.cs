using System.IO.Compression;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// Downloads the chromedriver that matches the major version of the Chrome installed on this machine.
    /// </summary>
    public static class AutomatedDownload
    {
        /// <summary>
        /// Downloads the chromedriver for this machine's platform, as detected by
        /// <see cref="MachineOSPlatform.GetPlatform"/>. See
        /// <see cref="DownloadChromeDriverAsync(Platform, int, string, HttpClient, CancellationToken)"/> for what is
        /// written and when it throws.
        /// </summary>
        /// <param name="downloadPath">An existing folder to write to. Empty or white space means <see cref="AppDomain.BaseDirectory"/> of the current domain.</param>
        /// <returns>A task that completes when chromedriver has been extracted.</returns>
        public static async Task DownloadChromeDriverAsync(string downloadPath = "")
        {
            await DownloadChromeDriverAsync(MachineOSPlatform.GetPlatform(), downloadPath);
        }

        /// <summary>
        /// Reads the major version of the locally installed Chrome with <see cref="LocalVersionChecking.GetChromeVersion"/>
        /// and downloads the chromedriver for that milestone and <paramref name="platform"/>, with the library's shared
        /// <see cref="HttpClient"/> (default 100 second timeout). See
        /// <see cref="DownloadChromeDriverAsync(Platform, int, string, HttpClient, CancellationToken)"/> for what is
        /// written and when it throws.
        /// </summary>
        /// <param name="platform">The platform to download for. The version still comes from the Chrome installed on this machine.</param>
        /// <param name="downloadPath">An existing folder to write to. Empty or white space means <see cref="AppDomain.BaseDirectory"/> of the current domain.</param>
        /// <returns>A task that completes when chromedriver has been extracted.</returns>
        /// <exception cref="Exception">Chrome was not found or its version could not be read.</exception>
        public static async Task DownloadChromeDriverAsync(Platform platform, string downloadPath = "")
        {
            var localMajorRelease = (await LocalVersionChecking.GetChromeVersion()).MajorReleaseNumber;

            await DownloadChromeDriverAsync(platform, localMajorRelease, downloadPath, GoogleChromeLabsEndpointQueries.SharedClient);
        }

        /// <summary>
        /// Downloads the chromedriver for a given milestone and platform with <paramref name="httpClient"/>, without
        /// looking at the Chrome installed on this machine. It reads latest-versions-per-milestone-with-downloads.json,
        /// downloads the ZIP file at the chromedriver URL it gives, which must be absolute HTTPS, into a temporary file
        /// that is deleted afterwards, and extracts the entry named chromedriver.exe or chromedriver into
        /// <paramref name="downloadPath"/>, replacing an existing file of that name. Nothing else is written there.
        /// Chrome for Testing publishes no checksums, so the download is trusted as far as HTTPS goes.
        /// </summary>
        /// <param name="platform">The platform to download for.</param>
        /// <param name="majorReleaseNumber">The milestone, such as 120.</param>
        /// <param name="downloadPath">An existing folder to write to. Empty or white space means <see cref="AppDomain.BaseDirectory"/> of the current domain.</param>
        /// <param name="httpClient">The client for the endpoint and the download. It is not disposed.</param>
        /// <param name="cancellationToken">Cancels the requests and the extraction.</param>
        /// <returns>The full path of the extracted chromedriver.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="httpClient"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The endpoint has no chromedriver for the milestone and platform, for example a milestone older than 115.</exception>
        /// <exception cref="InvalidDataException">The chromedriver URL is not absolute HTTPS, the download is not a ZIP file, the ZIP file has no chromedriver entry, or that entry's path is absolute or leaves its folder.</exception>
        /// <exception cref="HttpRequestException">The endpoint or the download could not be reached, or returned a status code that is not a success.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="downloadPath"/> does not exist.</exception>
        public static Task<string> DownloadChromeDriverAsync(Platform platform, int majorReleaseNumber, string downloadPath, HttpClient httpClient, CancellationToken cancellationToken = default) =>
            DownloadChromeDriverAsync(platform, majorReleaseNumber, downloadPath, httpClient, Path.GetTempPath(), cancellationToken);

        // The download with the folder for the temporary ZIP file as a parameter, so a test can watch it.
        internal static async Task<string> DownloadChromeDriverAsync(Platform platform, int majorReleaseNumber, string downloadPath, HttpClient httpClient, string temporaryFolder, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(httpClient);

            if (string.IsNullOrWhiteSpace(downloadPath)) downloadPath = AppDomain.CurrentDomain.BaseDirectory;

            var model = await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>(httpClient, cancellationToken);

            var url = AssetList.FromModel(model, Binary.ChromeDriver, platform)?
                .OrderByDescending(x => x.Key)
                .Where(x => x.Key.Split('.')[0].Equals(majorReleaseNumber.ToString()))
                .FirstOrDefault()
                .Value;

            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException(
                    $"Chrome for Testing lists no chromedriver for milestone {majorReleaseNumber} on {PlatformString.GetPlatformString(platform)}.");
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidDataException($"The chromedriver URL is not absolute HTTPS: {url}");
            }

            // The ZIP file goes to a temporary file of its own, deleted when it is closed, so the
            // download folder only ever receives chromedriver.
            var zipPath = Path.Combine(temporaryFolder, $"cft-chromedriver-{Guid.NewGuid():N}.zip");

            await using var zipFile = new FileStream(zipPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None,
                bufferSize: 81920, FileOptions.DeleteOnClose | FileOptions.Asynchronous);

            using (var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                await response.Content.CopyToAsync(zipFile, cancellationToken);
            }

            zipFile.Position = 0;

            using var archive = new ZipArchive(zipFile, ZipArchiveMode.Read, leaveOpen: true);

            var driver = archive.Entries.FirstOrDefault(IsChromeDriver)
                ?? throw new InvalidDataException($"The ZIP file at {url} has no chromedriver or chromedriver.exe entry.");

            var driverPath = DestinationInside(downloadPath, driver.FullName);

            await Task.Run(() => driver.ExtractToFile(driverPath, true), cancellationToken);

            return driverPath;
        }

        private static string EntryFileName(ZipArchiveEntry entry) =>
            entry.FullName.Replace('\\', '/').Split('/')[^1];

        private static bool IsChromeDriver(ZipArchiveEntry entry) =>
            EntryFileName(entry) is "chromedriver.exe" or "chromedriver";

        // The path the entry is extracted to: its file name in the folder. An entry whose own path is
        // rooted, names a drive or climbs with "..", or that would land anywhere but directly in the
        // folder, is refused rather than flattened.
        internal static string DestinationInside(string folder, string entryName)
        {
            var normalized = entryName.Replace('\\', '/');
            var segments = normalized.Split('/');

            if (normalized.StartsWith('/') || normalized.Contains(':') || segments.Contains(".."))
            {
                throw new InvalidDataException($"The ZIP entry {entryName} leaves the folder it is extracted to.");
            }

            var root = Path.GetFullPath(folder);
            if (!Path.EndsInDirectorySeparator(root)) root += Path.DirectorySeparatorChar;

            var destination = Path.GetFullPath(Path.Combine(root, Path.GetFileName(normalized)));

            if (!destination.StartsWith(root, StringComparison.Ordinal) || Path.GetRelativePath(root, destination) != segments[^1])
            {
                throw new InvalidDataException($"The ZIP entry {entryName} leaves the folder it is extracted to.");
            }

            return destination;
        }
    }
}
