using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;

internal class Program
{
    async static Task Main(string[] args)
    {
        await DownloadBrowser().ConfigureAwait(false);
    }

    private static async Task DownloadBrowser()
    {
        BrowserFetcher? browserFetcher;

        browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = Path.GetTempPath(),
            Browser = SupportedBrowser.Chromium
        });
        try
        {
            await browserFetcher.DownloadAsync(BrowserTag.Latest).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Browser download failed: {exception.Message}");
            throw;
        }

        browserFetcher.Dispose();

        Console.WriteLine("Browser download was successful");
    }
}

