using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KoPlayer.Lib.Filters;

namespace KoPlayer.Lib
{
    public class FilterPlaylist : Playlist
    {
        public List<Filter> Filters { get; private set; }

        private Library library;

        public FilterPlaylist(Library library, string name)
            : base(library, name)
        {
            this.library = library;
            Filters = new List<Filter>();
        }

        public void FilterLibrary()
        {
            songs.Clear();
            songs.AddRange(library.GetSongs());

            foreach (var filter in Filters)
            {
                filter.ApplyTo(songs);
                if (songs.Count == 0)
                    break;
            }
        }
    }
}
