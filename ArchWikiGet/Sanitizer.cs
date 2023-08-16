using HtmlAgilityPack;

namespace ArchWikiGet;

public class Sanitizer
{
    //sanitizes exactly one wiki page
    private HtmlDocument _document;
    private HtmlDocument? _result;

    public HtmlDocument Result
    {
        get
        {
            if (_result != null)
            {
                return _result;
            }

            throw new NullReferenceException("the result may not be accessed before the sanitization process is completed.");
        }
        private set => _result = value;
    }

    public Sanitizer(HtmlDocument document)
    {
        _document = document;
    }

    public async Task SanitizeAsync()
    {
        if (!Program.DoSanitize)
            return;

        //first step, remove the header.
        //Inspection of the wiki pages' HTML reveals that this is entirely encapsulated by the element with the ID `archnavbar`
        Remove("archnavbar");
        
        //next, remove the search header
        //the rest of the content is stored in the div under the class named `mw-page-content`
        //there are three elements below this. One is a span under the ID `mw-top-page`, one is a 'jump to navigation' link
        //   and the last contains the actual page content. All three get to stay.
        //In the main body element, the search header is stored under a div with the class name `mw-header.` No ID, unfortunately.
        //  The hamburger menu has an ID of `mw-sidebar-button`
        //  When the hamburger menu is out, a hidden checkbox element IDd `mw-sidebar-checkbox` is revealed
        //  The search bar has an ID of `p-search`
        //  The account button has an ID of `p-vector-menu-user-overflow`
        //  The triple dot menu has an ID of `p-personal
        Remove("mw-sidebar-button");
        Remove("p-search");
        Remove("p-vector-menu-user-overflow");
        Remove("p-personal");
        Remove("mw-sidebar-checkbox");
        
        //next, remove the left sidebar
        //The content of the left nav menu is normally hidden, but can be brought out with the hamburger menu button.
        //This content is stored in an element with a height of zero that is brought to a normal size when the hamburger menu is opened.
        //  This element is under the ID `mw-navigation`.
        //The table of contents is stored under a nav element with the id `mw-panel-toc`
        Remove("mw-panel-toc");
        Remove("mw-navigation");
        //some residual (useless) elements remain
        Remove("mw-sidebar-checkbox");
        Remove("vector-toc-collapsed-checkbox");
        
        
        
        return;

        void Remove(string id) => _document.GetElementbyId(id).Remove();
    }
}