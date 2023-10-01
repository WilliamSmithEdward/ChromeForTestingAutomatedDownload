using System.IO.Compression;

namespace ChromeForTestingAutomatedDownload
{
    public static class AutomatedDownload
    {
        public static async Task DownloadChromeDriverAsync(string downloadPath = "")
        {
            if (string.IsNullOrWhiteSpace(downloadPath)) downloadPath = AppDomain.CurrentDomain.BaseDirectory;

            var localMajorRelease = (await LocalVersionChecking.GetChromeVersion()).MajorReleaseNumber;

            var model = await ChromeVersionModelFactory.CreateChromeVersionModelAsync<LatestVersionsPerMilestoneWithDownload.ChromeVersionModel>();

            var url = await model.GetMostRecentAssetURLByMajorReleaseNumberAsync(Binary.ChromeDriver, Platform.Win64, localMajorRelease);

            using var httpClient = new HttpClient();
            
            try
            {
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    
                    using var fileStream = File.Create(Path.Combine(downloadPath, Path.GetFileName(url) ?? string.Empty));
                        
                    await stream.CopyToAsync(fileStream);

                    using ZipArchive archive = new ZipArchive(fileStream);

                    var driver = archive.Entries.Where(x => x.FullName.Contains("chromedriver.exe")).First();

                    await Task.Run(() => driver.ExtractToFile(Path.Combine(downloadPath, "chromedriver.exe"), true));

                    Console.WriteLine("File downloaded successfully!");
                }

                else
                {
                    Console.WriteLine($"Failed to download file. Status code: {response.StatusCode}");
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
