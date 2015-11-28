using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Playlists
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
                library.Remove(songPaths[i]);
        }

        public override void RemoveAll()
        {
            var paths = new List<string>(songPaths);
            foreach (string s in paths)
                library.Remove(s);
        }

        public override void Save()
        {
            library.Save();
        }
    }
}
