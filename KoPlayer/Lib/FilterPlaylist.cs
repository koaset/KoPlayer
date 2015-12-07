using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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

        protected override void SaveHeader(StreamWriter sw)
        {
            base.SaveHeader(sw);
            foreach (var filter in Filters)
                filter.Save(sw);
        }

        protected override void SaveSongs(StreamWriter sw)
        { }

        public FilterPlaylist(StreamReader sr, Library library)
        {
            this.library = library;
            Filters = new List<Filter>();

            ReadHeader(sr);

            ResetSortVariables();
            songs = new List<Song>();
            this.sortDictionaries = new List<Dictionary<string, List<Song>>>();

            FilterLibrary();
            Sorting.CreateSortDictionaries(songs, this.sortDictionaries);
        }

        protected override void ReadHeader(StreamReader sr)
        {
            base.ReadHeader(sr);

            string filterType;
            while ((filterType = sr.ReadLine()) != null)
            {
                Filter filter = null;

                switch (filterType)
                {
                    case "KoPlayer.Lib.Filters.StringFilter":
                        filter = new StringFilter(sr);
                        break;
                    case "KoPlayer.Lib.Filters.RatingFilter":
                        filter = new RatingFilter(sr);
                        break;
                    case "KoPlayer.Lib.Filters.DateFilter":
                        filter = new DateFilter(sr);
                        break;
                    default:
                        throw new FileLoadException();
                }

                Filters.Add(filter);
            }
        }
    }
}
