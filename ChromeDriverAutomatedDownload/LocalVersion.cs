namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// The version of the Chrome installed on this machine, as returned by <see cref="LocalVersionChecking.GetChromeVersion"/>.
    /// </summary>
    public class LocalVersion
    {
        /// <summary>The version as read, such as 120.0.6099.109. On Linux and macOS it can end with white space or a line break.</summary>
        public string VersionString { get; private set; }
        /// <summary>The number before the first dot of <see cref="VersionString"/>, or 0 if it is not a number.</summary>
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
