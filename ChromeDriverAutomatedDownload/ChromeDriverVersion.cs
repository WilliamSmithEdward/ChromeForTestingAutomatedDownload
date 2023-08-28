using System.Text.Json.Serialization;

namespace ChromeForTestingAutomatedDownload
{
    class ChromeDriverVersion
    {
        [JsonPropertyName("versions")]
        List<Version>? Versions { get; set; }
    }

    class Version
    {

    }
}
