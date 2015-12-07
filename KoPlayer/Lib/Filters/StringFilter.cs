using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Lib.Filters
{
    public class StringFilter : Filter
    {
        private string searchTerm = null;
        private string field = "title";

        public bool Contains { get; set; }

        public string SearchTerm
        {
            get 
            {
                return searchTerm;
            }
            set
            {
                if (value.Length > 0)
                    searchTerm = value.ToLower();
            }
        }

        public string Field
        {
            get
            {
                return field;
            }
            set
            {
                switch (value)
                {
                    case "title":
                    case "artist":
                    case "album":
                    case "genre":
                        field = value;
                        break;
                    default:
                        field = null;
                        break;
                }
            }
        }

        public StringFilter(string field, string searchTerm, bool contains)
        {
            Field = field;
            SearchTerm = searchTerm;
            Contains = contains;
        }

        protected override bool Allowed(Song song)
        {
            return !(song[Field].Contains(searchTerm) ^ Contains);
        }
    }
}
