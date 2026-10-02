using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    /// <summary>
    /// One version of a model that implements <see cref="IDownload"/>.
    /// </summary>
    public interface IVersionObject
    {
        /// <summary>The channel or, in the milestone models, the milestone number.</summary>
        public string Channel { get; set; }

        /// <summary>The full version, such as 120.0.6099.109.</summary>
        public string Version { get; set; }

        /// <summary>The Chromium revision.</summary>
        public string Revision { get; set; }

        /// <summary>The download URLs per binary and platform.</summary>
        public DownloadMetaData Downloads { get; set; }
    }
}
