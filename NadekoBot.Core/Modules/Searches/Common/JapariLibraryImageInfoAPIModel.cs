using System;
using System.Collections.Generic;
using System.Text;

namespace NadekoBot.Core.Modules.Searches.Common
{


    public class JapariLibraryImageInfoAPIModel
    {
        public Continue @continue { get; set; }
        public Query query { get; set; }
        public class Continue
        {
            public DateTime iistart { get; set; }
            public string @continue { get; set; }
        }

        public class Imageinfo
        {
            public DateTime timestamp { get; set; }
            public string user { get; set; }
            public string url { get; set; }
            public string descriptionurl { get; set; }
            public string descriptionshorturl { get; set; }
        }

        public class Page
        {
            public int pageid { get; set; }
            public int ns { get; set; }
            public string title { get; set; }
            public string imagerepository { get; set; }
            public List<Imageinfo> imageinfo { get; set; }
        }

        public class Query
        {
            public List<Page> pages { get; set; }
        }




    }
}
