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
        /// <see cref="DownloadChromeDriverAsync(Platform, string)"/> for what is written and when it throws.
        /// </summary>
        /// <param name="downloadPath">An existing folder to write to. Empty or white space means <see cref="AppDomain.BaseDirectory"/> of the current domain.</param>
        /// <returns>A task that completes when chromedriver has been extracted.</returns>
        public static async Task DownloadChromeDriverAsync(string downloadPath = "")
        {
            await DownloadChromeDriverAsync(MachineOSPlatform.GetPlatform(), downloadPath);
        }

        /// <summary>
        /// Reads the major version of the locally installed Chrome with <see cref="LocalVersionChecking.GetChromeVersion"/>,
        /// takes the chromedriver URL for that milestone and <paramref name="platform"/> from
        /// latest-versions-per-milestone-with-downloads.json, saves the ZIP file in <paramref name="downloadPath"/>
        /// under the file name at the end of the URL, and extracts the first entry named chromedriver.exe or
        /// chromedriver into the same folder. Both files replace any existing file of the same name, and the
        /// ZIP file is left in place. Nothing is verified beyond HTTPS.
        /// </summary>
        /// <param name="platform">The platform to download for. The version still comes from the Chrome installed on this machine.</param>
        /// <param name="downloadPath">An existing folder to write to. Empty or white space means <see cref="AppDomain.BaseDirectory"/> of the current domain.</param>
        /// <returns>A task that completes when chromedriver has been extracted.</returns>
        /// <exception cref="Exception">Chrome was not found or its version could not be read, or the download returned a status code that is not a success.</exception>
        /// <exception cref="InvalidOperationException">The endpoint has no chromedriver for the local milestone and platform (for example Chrome older than 115), or the ZIP file has no chromedriver entry.</exception>
        /// <exception cref="HttpRequestException">An endpoint or the download could not be reached.</exception>
        public static async Task DownloadChromeDriverAsync(Platform platform, string downloadPath = "")
        {
            var localMajorRelease = (await LocalVersionChecking.GetChromeVersion()).MajorReleaseNumber;

            using var httpClient = new HttpClient();

            await DownloadChromeDriverAsync(platform, localMajorRelease, downloadPath, httpClient);
        }

        /// <summary>
        /// Downloads the chromedriver for a given milestone and platform with <paramref name="httpClient"/>, without
        /// looking at the Chrome installed on this machine. Otherwise the same as
        /// <see cref="DownloadChromeDriverAsync(Platform, string)"/>: it reads
        /// latest-versions-per-milestone-with-downloads.json, saves the ZIP file in <paramref name="downloadPath"/>
        /// under the file name at the end of the URL, and extracts the first entry named chromedriver.exe or
        /// chromedriver into the same folder.
        /// </summary>
        /// <param name="platform">The platform to download for.</param>
        /// <param name="majorReleaseNumber">The milestone, such as 120.</param>
        /// <param name="downloadPath">An existing folder to write to. Empty or white space means <see cref="AppDomain.BaseDirectory"/> of the current domain.</param>
        /// <param name="httpClient">The client for the endpoint and the download. It is not disposed.</param>
        /// <param name="cancellationToken">Cancels the requests.</param>
        /// <returns>The full path of the extracted chromedriver.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="httpClient"/> is null.</exception>
        /// <exception cref="Exception">The download returned a status code that is not a success.</exception>
        /// <exception cref="InvalidOperationException">The endpoint has no chromedriver for the milestone and platform, or the ZIP file has no chromedriver entry.</exception>
        /// <exception cref="HttpRequestException">The endpoint or the download could not be reached.</exception>
        public static async Task<string> DownloadChromeDriverAsync(Platform platform, int majorReleaseNumber, string downloadPath, HttpClient httpClient, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(httpClient);

            if (string.IsNullOrWhiteSpace(downloadPath)) downloadPath = AppDomain.CurrentDomain.BaseDirectory;

            var model = await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>(httpClient, cancellationToken);

            var url = AssetList.FromModel(model, Binary.ChromeDriver, platform)?
                .OrderByDescending(x => x.Key)
                .Where(x => x.Key.Split('.')[0].Equals(majorReleaseNumber.ToString()))
                .FirstOrDefault()
                .Value;

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                using var fileStream = File.Create(Path.Combine(downloadPath, Path.GetFileName(url) ?? string.Empty));

                await stream.CopyToAsync(fileStream, cancellationToken);

                using var archive = new ZipArchive(fileStream);

                var driver = archive.Entries.Where(x =>
                    Path.GetFileName(x.FullName).Equals("chromedriver.exe") ||
                    Path.GetFileName(x.FullName).Equals("chromedriver"))
                .First();

                var driverPath = Path.Combine(downloadPath, Path.GetFileName(driver.FullName));

                await Task.Run(() => driver.ExtractToFile(driverPath, true), cancellationToken);

                return driverPath;
            }

            else
            {
                throw new Exception($"Failed to download file. Status code: {response.StatusCode}");
            }
        }
    }
}
