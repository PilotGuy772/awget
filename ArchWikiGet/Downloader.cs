using System.Net;
using HtmlAgilityPack;

namespace ArchWikiGet;

public class Downloader
{
    //download exactly one page using HtmlAgilityPack
    private readonly string _url;
    private HtmlDocument? _savedDocument;

    public HtmlDocument SavedDocument
    {
        get
        {
            if (_savedDocument != null)
                return _savedDocument;
            throw new Exception("Cannot access the document before it is downloaded, diptard");
        }
        private set => _savedDocument = value;
    }

    public Downloader(string arg)
    {
        if (Program.DoTreatAsURL)
            _url = arg;
        else if (Program.DoTreatAsTitle)
            _url = "https://wiki.archlinux.org/title/" + arg.Replace(' ', '_').ToLower();
        else
            _url = "https://wiki.archlinux.org/title/" + arg;
        
    }

    public void DownloadPage()
    {
        using var client = new HttpClient();
        HttpResponseMessage response = client.GetAsync(_url).Result;

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException("The http request returned failure error code.", null, response.StatusCode);
        
        var document = new HtmlDocument();
        string content = response.Content.ReadAsStringAsync().Result;
        document.LoadHtml(content);
        SavedDocument = document;

    }

    public async Task DownloadPageAsync()
    {
        using var client = new HttpClient();
        HttpResponseMessage message = await client.GetAsync(_url);
        
        if (!message.IsSuccessStatusCode) throw new Exception(message.StatusCode.ToString());
        
        var document = new HtmlDocument();
        string content = await message.Content.ReadAsStringAsync();
        document.LoadHtml(content);
        SavedDocument = document;

    }
}