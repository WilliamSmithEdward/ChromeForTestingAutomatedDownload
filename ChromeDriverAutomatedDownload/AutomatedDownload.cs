using System.IO.Compression;

namespace ChromeForTestingAutomatedDownload
{
    public static class AutomatedDownload
    {
        public static async Task DownloadChromeDriverAsync(string downloadPath = "")
        {
            await DownloadChromeDriverAsync(MachineOSPlatform.GetPlatform(), downloadPath);
        }

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

                using ZipArchive archive = new ZipArchive(fileStream);

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