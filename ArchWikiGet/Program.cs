/*
 * Main behavior:
 * There will be one class for each major step in the process or each major extension to the functionality.
 * For example, there must be
 */


namespace ArchWikiGet;

class Program
{
    //environment variables for command-line args, along with their default values.
    //all booleans start with `Do...`
    //controlled by -p or --raw
    private static bool DoSanitize = true; //whether to sanitize the page. -p or --raw reverses this.
    //controlled by -f or --force
    private static bool DoForce = false; //whether to process any page that is received
    //controlled by -t or --title
    private static bool DoTreatAsTitle = false; //whether to process the argument as an article title rather than a page slug
    //controlled by -s or --search
    private static bool DoSearchMode = false; //whether to process the argument as a search query and just return search results
    //controlled by -d or --database
    private static bool DoIndexInDatabase = false; //whether to add the downloaded page to the database
    //controlled by -r or --recursive
    private static bool DoRecursion = false; //whether to recursively download page dependencies
    //controlled by the value that follows the -r flag
    private static int  RecursionLayers = 0; //how many layers to get page links. 0 indicates that all deps should be downloaded.
    //controlled by -o or --output
    private static bool DoWriteToFile = false; //whether to write to a file
    //controlled by the value that follows -o
    private static string FilePath = "./output"; //where to write the downloaded page, if applicable.
    //controlled by the flag -l or --little
    private static bool DoTinyMode = false; //whether to be extremely strict with memory (i.e. keep only ~five pages in memory at a time)
    //controlled by the only plain argument that doesn't follow a flag
    private static List<string> Arguments = new(); //which wiki page(s) to download
    //controlled by -h or --help
    private static bool DoHelpMode = false; //if true, print help page and exit
    //controlled by -u or --url
    private static bool DoTreatAsURL = false; //whether to treat args as full URLs
    //controlled by -m or --markdown
    private static bool DoMarkdown = false; //whether to output as markdown instead of HTML.
    
    
    public static void Main(string[] args)
    {
        //step zero: process flags
        ProcessFlags(args);
        //step one: handle behavior changing arguments.
        //This includes: help, search, recursive
        if (DoHelpMode)
        {
            PrintHelp();
            Environment.Exit(0);
        }
    }

    private static void ProcessFlags(string[] args)
    {
        //iterate through args
        for (int i = 0; i < args.Length; i++)
        {
            //check if the arg is a flag
            if (args[i].StartsWith("-"))
            {
                //if it's a long flag 
                if (args[i][1].Equals('-'))
                {
                    //now go thru the switch/case
                    switch (args[i])
                    {
                        case "--raw":
                            DoSanitize = false;
                            break;
                        case "--force":
                            DoForce = true;
                            break;
                        case "--title":
                            DoTreatAsTitle = true;
                            break;
                        case "--search":
                            DoSearchMode = true;
                            break;
                        case "--database":
                            DoIndexInDatabase = true;
                            break;
                        case "--recursive":
                            DoRecursion = true;
                            i++;
                            RecursionLayers = Convert.ToInt32(args[i]);
                            //skip the next arg bcz it's already been processed
                            break;
                        case "--output":
                            DoWriteToFile = true;
                            i++;
                            FilePath = args[i];
                            break;
                        case "--little":
                            DoTinyMode = true;
                            break;
                        case "--help":
                            DoHelpMode = true;
                            break;
                        case "--url":
                            DoTreatAsURL = true;
                            break;
                        case "--markdown":
                            DoMarkdown = true;
                            break;
                        default:
                            throw new Exception("unrecognized flag: " + args[i]);
                        
                    }

                    continue;
                }
                
                //otherwise we have short flags. These can be grouped,
                //so there will be multiple mods in one 'arg.'
                //flags that require values must be one per group, 
                //otherwise throw an error.
                var tooManyValues = false;
                foreach (char flag in args[i])
                {
                    switch (flag)
                    {
                        case 'p':
                            DoSanitize = false;
                            break;
                        case 'f':
                            DoForce = true;
                            break;
                        case 't':
                            DoTreatAsTitle = true;
                            break;
                        case 's':
                            DoSearchMode = true;
                            break;
                        case 'd':
                            DoIndexInDatabase = true;
                            break;
                        case 'r':
                            if (tooManyValues)
                            {
                                throw new Exception("Only one value-bound argument per argument group.");
                            }

                            tooManyValues = true;
                            DoRecursion = true;
                            RecursionLayers = Convert.ToInt32(args[i + 1]);
                            break;
                        case 'o':
                            if (tooManyValues)
                            {
                                throw new Exception("Only one value-bound argument per argument group.");
                            }

                            tooManyValues = true;
                            DoWriteToFile = true;
                            FilePath = args[i + 1];
                            break;
                        case 'l':
                            DoTinyMode = true;
                            break;
                        case 'h':
                            DoHelpMode = true;
                            break;
                        case 'u':
                            DoTreatAsURL = true;
                            break;
                        case 'm':
                            DoMarkdown = true;
                            break;
                        case '-':
                            break;
                        default:
                            throw new Exception("unrecognized flag: -" + flag);
                            
                            
                    }

                    if (tooManyValues)
                        i++; //skip the next arg bcz it's already processed
                }
            }
        }
    }

    private static void PrintHelp()
    {
        //print help page and exit
        const string helpPage = "awget - Download pages from the Arch Wiki.\n" +
                          "Usage:\n" +
                          "   awget [options] <page-slugs>\n" +
                          "   awget [options] -r <layers> <page-slugs>\n" +
                          "   awget [options] -o <file> <page-slugs>\n" +
                          "   awget [options] -t <page-titles>\n" +
                          "\n" +
                          "Options:\n" +
                          "   -d  --database  Register the downloaded pages in the wikidb database and copy\n" +
                          "                   downloaded files to the database directory. Configure with wikidb.\n" +
                          "   -f  --force     Download and process the requested page no matter what is received.\n" +
                          "   -l  --little    Attempts to use as little memory as possible when downloading and processing\n" +
                          "                   pages. Useful with -r on memory limited systems.\n" +
                          "   -m  --markdown  Output markdown instead of HTML.\n" +
                          "   -o  --output    Send the output to the requested file instead of stdout.\n" +
                          "   -p  --raw       Skip the HTML sanitization step.\n" +
                          "   -r  --recursive Recursively download all pages that are directly linked to by downloaded\n" +
                          "                   pages. Specify the layers of depth to search. A depth argument below 1\n" +
                          "                   indicates that all pages linked to should be downloaded until there are\n" +
                          "                   no more to download.\n" +
                          "                         -> This may use a fair amount of disk space and memory. If memory\n" +
                          "                            is extremely tight, consider adding -l. Using -l will significantly\n" +
                          "                            slow down the process.\n" +
                          "                         -> By default, all downloaded pages are placed in their own files in the\n" +
                          "                            current directory. Optionally, specify the directory to place files in\n" +
                          "                            with -o, or download the files to the wiki database with -d.\n" +
                          "                         -> Duplicates are not downloaded. A list of pages that have already\n" +
                          "                            been downloaded is stored in memory. If -l is specified, the \n" +
                          "                            destination directory is checked for duplicates instead.\n" +
                          "   -s  --search    Use the Arch Wiki's search function to search for pages by\n" +
                          "                   keyword. This returns a summary of results, but does not download pages.\n" +
                          "   -t  --title     Treat input arguments as page titles instead of page slugs.\n" +
                          "   -u  --url       Treat input arguments as regular page URLs instead of page slugs or titles.\n" +
                          "\n" +
                          "Description:\n" +
                          "   This command-line tool downloads pages from the Arch Wiki, sanitizes them to remove elements\n" +
                          "   such as the header, footer, and sidebar, and prints the resulting HTML to stdout. The above\n" +
                          "   options modify and extend this behavior. This utility also comes with a partner utility, wikidb,\n" +
                          "   which indexes local wiki pages from the Arch Wiki or any other source. This software is licensed\n" +
                          "   under the MIT license. You may view the source code for this software (and for wikidb) as well as\n" +
                          "   the full license over at https://github.com/PilotGuy772/awget .\n" +
                          "Version: dev-prealpha-v0.0.1";
        
        Console.WriteLine(helpPage);
    }
}