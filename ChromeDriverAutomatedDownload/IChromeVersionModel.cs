namespace ChromeForTestingAutomatedDownload
{
    public interface IChromeVersionModel 
    {
        public Func<Task<string>> QueryEndpoint { get; set; }
    }
}