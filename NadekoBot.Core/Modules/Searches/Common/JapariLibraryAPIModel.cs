using System;
using System.Collections.Generic;
using System.Text;

namespace NadekoBot.Core.Modules.Searches.Common
{


    public class JapariLibraryAPIModel
    {
        public bool batchcomplete { get; set; }
        public Continue @continue { get; set; }
        public Query query { get; set; }
        public class Continue
        {
            public int gsroffset { get; set; }
            public string @continue { get; set; }
        }

        public class Revision
        {
            public int revid { get; set; }
            public int parentid { get; set; }
            public bool minor { get; set; }
            public string user { get; set; }
            public DateTime timestamp { get; set; }
            public string comment { get; set; }
        }

        public class Page
        {
            public int pageid { get; set; }
            public int ns { get; set; }
            public string title { get; set; }
            public int index { get; set; }
            public string contentmodel { get; set; }
            public string pagelanguage { get; set; }
            public string pagelanguagehtmlcode { get; set; }
            public string pagelanguagedir { get; set; }
            public DateTime touched { get; set; }
            public int lastrevid { get; set; }
            public int length { get; set; }
            public string fullurl { get; set; }
            public string editurl { get; set; }
            public string canonicalurl { get; set; }
            public List<Revision> revisions { get; set; }
        }

        public class Query
        {
            public List<Page> pages { get; set; }
        }
    }
}
