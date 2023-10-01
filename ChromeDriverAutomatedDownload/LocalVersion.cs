namespace ChromeForTestingAutomatedDownload
{
    public class LocalVersion
    {
        public string VersionString { get; private set; }
        public int MajorReleaseNumber
        {
            get
            {
                var majorRelease = int.TryParse(VersionString.Split(".")[0], out int _majorRelease) ? _majorRelease : 0;
                return majorRelease;
            }
        }

        internal LocalVersion(string versionString)
        {
            VersionString = versionString;
        }
    }
}
