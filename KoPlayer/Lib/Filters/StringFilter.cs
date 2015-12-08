using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Lib.Filters
{
    public class StringFilter : Filter
    {
        private string field;
        private string searchTerm;

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

        public StringFilter(StringFilter filter)
            : this(filter.field, filter.searchTerm, filter.Contains)
        { }

        protected override bool Allowed(Song song)
        {
            return !(song[Field].ToLower().Contains(searchTerm) ^ Contains);
        }

        protected override void SaveData(System.IO.StreamWriter sw)
        {
            sw.WriteLine(Field);
            sw.WriteLine(SearchTerm);
            sw.WriteLine(Contains);
        }

        public StringFilter(System.IO.StreamReader sr)
            : this(sr.ReadLine(), sr.ReadLine(), bool.Parse(sr.ReadLine()))
        { }

        public override string ToString()
        {
            string field = char.ToUpper(Field[0]) + Field.Substring(1); ;

            string modifier = " contains: ";
            if (!Contains)
                modifier = " does not contain: ";

            return field + modifier + searchTerm;
        }
    }
}
