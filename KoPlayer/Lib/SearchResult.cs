using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Lib
{
    public class SearchResult : Playlist
    {
        private Library library;

        public SearchResult(Library library)
            : base(library, "Search Results")
        {
            this.library = library;
        }

        public override void Remove(List<int> indexes)
        {
            foreach (int i in indexes)
                library.Remove(songs[i]);
        }

        public override void RemoveAll()
        {
            songs.Clear();
        }

        public override void Save()
        {
            library.Save();
        }
    }
}
