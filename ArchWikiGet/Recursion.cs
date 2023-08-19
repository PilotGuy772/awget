using HtmlAgilityPack;

namespace ArchWikiGet;

public static class Recursion
{
    // Recursively get all links from a page
    // This class handles everything related to recursion.
    
    private static List<string> _visited = new();
    
    
    
    /*
     * Process for recursion:
     * 1. Get all links from a page that lead to other Arch Wiki pages
     * 2. Filter the pages that have already been visited
     * 3. Add the remaining pages to the visited list
     * 4. Initialize a new Downloader for each page and download it; await the result
     * 5. Repeat the process for each page
     * 6. Initialize a new Sanitizer for each page and sanitize it; await the result
     * 7. Repeat the process for each page
     * 8. Save the sanitized pages to the disk
     * 9. For each sanitized page, initialize a new recursion process in a background task.
     *      -> note: adding to the database is not implemented yet
     */

    public static async Task RecurseAsync(string documentString, int depth)
    {
        if (depth == Program.RecursionLayers) return; // make sure we aren't at the bottom of the recursion
        
        // turn the string document into an HtmlDocument
        var document = new HtmlDocument();
        document.LoadHtml(documentString);
        // 1. Get all links from a page that lead to other Arch Wiki pages
        var links = document.DocumentNode
            .SelectNodes("//a[@href]")
            .Select(node => node.Attributes["href"].Value)
            .Where(link => link.StartsWith("/title/"))
            .ToList();
        
        // 2. Filter the pages that have already been visited
        links = links.Where(link => !_visited.Contains(link)).ToList();
        
        // also, for links that include an anchor, remove the anchor
        links = links.Select(link => link.Contains('#') ? link[..link.IndexOf("#", StringComparison.Ordinal)] : link).ToList();
        
        //finally, remove links that reference pages that are already on the disk in the target directory
        links = Program.DoWriteToFile 
            ? links.Where(link => !File.Exists(Program.FilePath + "/" + link.Replace("/title/", "") + (Program.DoMarkdown ? ".md" : ".html"))).ToList() 
            : links.Where(link => !File.Exists("./" + link.Replace("/title/", "") + (Program.DoMarkdown ? ".md" : ".html"))).ToList();
        
        // 3. Add the remaining pages to the visited list
        _visited.AddRange(links);
        
        // 4. Initialize a new Downloader for each page and download it; await the result
        var downloaders = links.Select(link => new Downloader(link.Replace("/title/", ""))).ToList();
        
        foreach (Downloader downloader in downloaders)
        {
            await downloader.DownloadPageAsync();
        }
        
        // 5. Repeat the process for each page
        
        // 6. Initialize a new Sanitizer for each page and sanitize it; await the result
        var sanitizers = downloaders.Select(downloader => new Sanitizer(downloader.SavedDocument)).ToList();
        
        foreach (Sanitizer sanitizer in sanitizers)
        {
            await sanitizer.SanitizeAsync();
        }
        
        // 7. Repeat the process for each page
        
        // 8. Save the sanitized pages to the disk
        // for now, there will unfortunately be no saving to database. We will have to wait for the database to be implemented.
        
        //if the directory is specified, save the files to the directory
        if (Program.DoWriteToFile)
        {
            for (var i = 0; i < links.Count; i++)
            {
                Saver saver = new(sanitizers[i].Result);
                await saver.SaveAsync(Program.FilePath + "/" + links[i].Replace("/title/", "") + (Program.DoMarkdown ? ".md" : ".html"));
            }
        }
        else
        {
            //if the directory is not specified, save the files to the current directory
            for (var i = 0; i < links.Count; i++)
            {
                Saver saver = new(sanitizers[i].Result);
                await saver.SaveAsync( "./" + links[i].Replace("/title/", "") + (Program.DoMarkdown ? ".md" : ".html"));
            }
        }
        
        //list to store background tasks
        List<Task> tasks = new();
        
        // 9. For each sanitized page, initialize a new recursion process in a background task.
        for (var j = 0; j < links.Count; j++)
        {
            Console.WriteLine($"Dowloading {links[j]}...");
            tasks.Add(RecurseAsync(sanitizers[j].Result, depth + 1));
        }
        
        //wait for all background tasks to finish
        await Task.WhenAll(tasks);
        
        //all done
    }
}