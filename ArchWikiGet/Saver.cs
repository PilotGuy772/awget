using HtmlAgilityPack;

namespace ArchWikiGet;

public class Saver
{
    private readonly string _document;
    
    public Saver(string document)
    {
        _document = document;
    }


    public async Task SaveAsync(string path)
    {
        //first, check if the file already exists
        if (!CheckForFile(path)) return;
        
        //make sure the directories exist along the way
        // Extract the directory path from the file path
        string directoryPath = Path.GetDirectoryName(path) ?? "";

        // Create any missing directories in the path
        Directory.CreateDirectory(directoryPath);
        
        //if the file doesn't exist, create it
        FileStream stream = File.Create(path);
        stream.Close();
        
        //then, write the document to the file
        await File.WriteAllTextAsync(path, _document);
        
        //all done
    }

    //check if the file already exists and ask the user if they want to overwrite it
    //true means you are clear to write to the file, false means you are not
    private static bool CheckForFile(string path)
    {
        if (!File.Exists(path)) return true;
        Console.Write($"File \"{path}\" already exists. Overwrite? [y/N] ");
        string input = Console.ReadLine() ?? "";
        return input.ToLower() == "y" || input.ToLower() == "yes";
    }
}