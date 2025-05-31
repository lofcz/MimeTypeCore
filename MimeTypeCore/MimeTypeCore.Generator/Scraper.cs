using System.Collections.Concurrent;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Flurl.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MimeTypeCore.Generator;

public static class Scraper
{
    public static async Task<List<string>> Scrape()
    {
        IConfiguration config = Configuration.Default.WithDefaultLoader();
        IBrowsingContext context = BrowsingContext.New(config);
        IDocument document = await context.OpenAsync("https://www.iana.org/assignments/media-types/media-types.xhtml");
        
        IHtmlCollection<IElement> csvLinks = document.QuerySelectorAll("a[href$='.csv']");
        List<string> ret = [];
        
        foreach (IElement element in csvLinks)
        {
            IHtmlAnchorElement link = (IHtmlAnchorElement)element;
            ret.Add(link.Href);
        }

        return ret;
    }

    public static async Task<List<string>> DownloadFiles(List<string> urls)
    {
        DirectoryInfo dir = Directory.CreateDirectory("sources");

        foreach (FileInfo file in dir.GetFiles())
        {
            file.Delete();
        }

        foreach (DirectoryInfo subDirectory in dir.GetDirectories())
        {
            subDirectory.Delete(true);
        }

        ConcurrentDictionary<string, string> dict = [];
        
        await Parallel.ForEachAsync(urls, async (url, token) =>
        {
            string ext = Path.GetFileName(url);
            await url.WithHeaders(new
            {
                User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36"
            }).DownloadFileAsync(dir.FullName, ext, cancellationToken: token);

            dict.TryAdd(Path.Join(dir.FullName, ext), string.Empty);
        });

        return dict.Select(x => x.Key).ToList();
    }
}