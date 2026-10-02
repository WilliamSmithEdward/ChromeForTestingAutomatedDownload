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
            if (string.IsNullOrWhiteSpace(downloadPath)) downloadPath = AppDomain.CurrentDomain.BaseDirectory;

            var localMajorRelease = (await LocalVersionChecking.GetChromeVersion()).MajorReleaseNumber;

            var model = await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

            var url = await model.GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary.ChromeDriver, platform, localMajorRelease);

            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();

                using var fileStream = File.Create(Path.Combine(downloadPath, Path.GetFileName(url) ?? string.Empty));

                await stream.CopyToAsync(fileStream);

                using var archive = new ZipArchive(fileStream);

                var driver = archive.Entries.Where(x =>
                    Path.GetFileName(x.FullName).Equals("chromedriver.exe") ||
                    Path.GetFileName(x.FullName).Equals("chromedriver"))
                .First();

                await Task.Run(() => driver.ExtractToFile(Path.Combine(downloadPath, Path.GetFileName(driver.FullName)), true));
            }

            else
            {
                throw new Exception($"Failed to download file. Status code: {response.StatusCode}");
            }
        }
    }
}