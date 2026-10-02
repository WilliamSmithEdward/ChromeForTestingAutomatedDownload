using System.IO.Compression;
using System.Net;
using System.Text;

namespace ChromeForTestingAutomatedDownload.Tests;

/// <summary>
/// Answers requests from a fixed table and refuses any other, so no test reaches the network.
/// </summary>
internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpResponseMessage>> _responses = new(StringComparer.Ordinal);

    public List<Uri> Requests { get; } = new();

    public FakeHttpHandler Serve(string url, byte[] body, HttpStatusCode status = HttpStatusCode.OK)
    {
        _responses[url] = () => new HttpResponseMessage(status) { Content = new ByteArrayContent(body) };
        return this;
    }

    public FakeHttpHandler Serve(string url, string body) => Serve(url, Encoding.UTF8.GetBytes(body));

    public FakeHttpHandler ServeFixture(string fileName) =>
        Serve(Fixtures.EndpointBase + fileName, Fixtures.Read(fileName));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri ?? throw new InvalidOperationException("request without a URI");
        Requests.Add(uri);
        if (!_responses.TryGetValue(uri.AbsoluteUri, out var response))
        {
            throw new InvalidOperationException($"The test made an unexpected request to {uri}.");
        }
        return Task.FromResult(response());
    }

    public HttpClient Client() => new(this, disposeHandler: false);
}

internal static class Fixtures
{
    public const string EndpointBase = "https://googlechromelabs.github.io/chrome-for-testing/";

    public const string Milestones = "latest-versions-per-milestone-with-downloads.json";

    public static string Read(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    public static string DriverUrl(string version, string platform) =>
        $"https://storage.googleapis.com/chrome-for-testing-public/{version}/{platform}/chromedriver-{platform}.zip";

    /// <summary>A ZIP archive holding the given entries, each with the given text.</summary>
    public static byte[] Zip(params (string Name, string Content)[] entries)
    {
        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                using var writer = new StreamWriter(archive.CreateEntry(name).Open());
                writer.Write(content);
            }
        }
        return buffer.ToArray();
    }
}

/// <summary>
/// A folder of its own under the system temp folder for one test, deleted afterwards.
/// </summary>
internal sealed class TempFolder : IDisposable
{
    public TempFolder()
    {
        Root = Path.Combine(Path.GetTempPath(), "cft-tests", Guid.NewGuid().ToString("N"));
        Download = Path.Combine(Root, "download");
        Directory.CreateDirectory(Download);
    }

    /// <summary>The parent of <see cref="Download"/>, so a test can see writes outside the download folder.</summary>
    public string Root { get; }

    /// <summary>The download folder.</summary>
    public string Download { get; }

    public string[] Files() =>
        Directory.GetFiles(Root, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(Root, f).Replace('\\', '/'))
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

    public void Dispose()
    {
        try { Directory.Delete(Root, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
