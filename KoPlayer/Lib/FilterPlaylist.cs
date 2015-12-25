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
        public List<Filter> Filters { get; set; }

        private Library library;

        public FilterPlaylist(Library library, string name)
            : this(library, name, null)
        {
            this.library = library;
            Filters = new List<Filter>();
        }

        public FilterPlaylist(Library library, string name, List<Filter> filters)
            : base(library, name)
        {
            this.library = library;

            Filters = filters ?? new List<Filter>();

            FilterLibrary();
        }

        public void FilterLibrary()
        {
            songs.Clear();
            songs.AddRange(library.GetSongs());

            foreach (var filter in Filters)
            {
                if (songs.Count == 0)
                    break;
                filter.ApplyTo(songs);
            }
            Sorting.CreateSortDictionaries(songs, this.sortDictionaries);
        }

        public override void UpdateSongInfo(Song song)
        {
            RemoveFromSortDictionaries(song);

            bool allowed = Filters.All(f => f.Allowed(song));

            if (!allowed)
                songs.Remove(song);
            else
            {
                if (!songs.Contains(song))
                    songs.Add(song);
                Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
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
                    case "StringFilter":
                        filter = new StringFilter(sr);
                        break;
                    case "RatingFilter":
                        filter = new RatingFilter(sr);
                        break;
                    case "DateFilter":
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
