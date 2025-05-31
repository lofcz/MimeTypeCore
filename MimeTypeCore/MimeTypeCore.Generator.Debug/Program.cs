namespace MimeTypeCore.Generator.Debug;

class Program
{
    static async Task Main(string[] args)
    {
        List<string> urls = await Scraper.Scrape();
        List<string> files = await Scraper.DownloadFiles(urls);

        int z = 0;
    }
}