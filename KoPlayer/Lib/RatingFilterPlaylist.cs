using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace KoPlayer.Lib
{
    public class RatingFilterPlaylist : Playlist
    {
        Library library;
        private int allowedRating = 5;

        public int AllowedRating { get { return this.allowedRating; } 
            set 
            {
                if (value < 0)
                    allowedRating = 0;
                else if (value > 5)
                    allowedRating = 5;
                else
                    allowedRating = value; 
            }
        }
        public bool IncludeHigher { get; set; }

        public RatingFilterPlaylist()
            : base() { }

        public RatingFilterPlaylist(Library library, string name, int ratingCutoff, bool andHigher)
            : this(library, name, new List<Song>(), ratingCutoff, andHigher) { }
        
        public RatingFilterPlaylist(Library library, string name, List<Song> songs, int ratingCutoff, bool includeHigher)
            : base(library, name, songs) 
        {
            this.library = library;
            this.allowedRating = ratingCutoff;
            this.IncludeHigher = includeHigher;
            UpdateSongs();
            Sorting.CreateSortDictionaries(songs, this.sortDictionaries);
        }

        public override List<Song> GetSongs()
        {
            return songs;
        }

        /// <summary>
        /// Update songpath list according to smart playlist settings
        /// </summary>
        public void UpdateSongs()
        {
            base.songs.Clear();
            library.ResetRatingDictionary();
            Dictionary<string, List<Song>> ratingDictionary = library.GetSortDictionary("rating");
            foreach (string key in ratingDictionary.Keys)
            {
                if (key.ToString().Length == allowedRating)
                    base.songs.AddRange(ratingDictionary[key]);
                else if (IncludeHigher)
                    if (key.ToString().Length > allowedRating)
                        base.songs.AddRange(ratingDictionary[key]);
            }
        }

        public override void UpdateSongInfo(Song song)
        {
            RemoveFromSortDictionaries(song);

            if ((song.Rating == allowedRating) || (IncludeHigher && (song.Rating > allowedRating)))
            {
                if (!songs.Contains(song))
                    Add(song);
                Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
            }
            else
            {
                Remove(song.Path);
                songs.Remove(song);
            }
        }

        public void ResetSortDictionaries()
        {
            base.songs =  base.GetSongs();
            Sorting.CreateSortDictionaries(songs, base.sortDictionaries);
        }

        public override Song GetRandom()
        {
            return songs[Playlist.r.Next(0, songs.Count)];
        }

        protected override void SaveHeader(StreamWriter sw)
        {
            base.SaveHeader(sw);
            sw.WriteLine(allowedRating);
            sw.WriteLine(IncludeHigher);
        }

        protected override void SaveSongs(StreamWriter sw)
        { }

        public RatingFilterPlaylist(StreamReader sr, Library library)
        {
            ReadHeader(sr);
            //Init(library);

            this.library = library;

            ResetSortVariables();
            songs = new List<Song>();
            this.sortDictionaries = new List<Dictionary<string, List<Song>>>();

            UpdateSongs();
            Sorting.CreateSortDictionaries(songs, this.sortDictionaries);
        }

        protected override void ReadHeader(StreamReader sr)
        {
            base.ReadHeader(sr);
            AllowedRating = int.Parse(sr.ReadLine());
            IncludeHigher = bool.Parse(sr.ReadLine());
        }
    }
}
