using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NadekoBot.Core.Modules.Searches.Common
{
    public class JapariLibraryParseAPIModel
    {
        public Parse parse { get; set; }
        public class Category
        {
            public string sortkey { get; set; }
            public string category { get; set; }
        }

        public class Properties
        {
            public string PFDefaultForm { get; set; }
            public string description { get; set; }
        }

        public class Parse
        {
            public string title { get; set; }
            public int pageid { get; set; }
            public List<Category> categories { get; set; }
            public List<string> images { get; set; }
            public string displaytitle { get; set; }
            public Properties properties { get; set; }
        }
    }
}
