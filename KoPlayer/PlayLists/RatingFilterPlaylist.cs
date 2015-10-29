using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace KoPlayer.Playlists
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
        public override string Path { get { return @"Playlists\" + base.Name + ".fpl"; } }

        public RatingFilterPlaylist()
            : base() { }

        public RatingFilterPlaylist(Library library, string name, int ratingCutoff, bool andHigher)
            : this(library, name, new List<string>(), ratingCutoff, andHigher) { }
        
        public RatingFilterPlaylist(Library library, string name, List<string> songPaths, int ratingCutoff, bool includeHigher)
            : base(library, name, songPaths) 
        {
            this.library = library;
            this.allowedRating = ratingCutoff;
            this.IncludeHigher = includeHigher;
            UpdateSongPaths();
            base.outputSongs = base.GetAllSongs();
            Sorting.CreateSortDictionaries(outputSongs, this.sortDictionaries);
        }

        public override BindingList<Song> GetAllSongs()
        {
            UpdateSongPaths();
            base.outputSongs = base.GetAllSongs();
            //Sorting.CreateSortDictionaries(outputSongs, this.sortDictionaries);
            return base.outputSongs;
        }

        /// <summary>
        /// Update songpath list according to smart playlist settings
        /// </summary>
        private void UpdateSongPaths()
        {
            List<Song> songList = new List<Song>();
            Dictionary<string, List<Song>> ratingDictionary = library.GetSortDictionary("rating");
            foreach (string key in ratingDictionary.Keys)
            {
                if (key.ToString().Length == allowedRating)
                    songList.AddRange(ratingDictionary[key]);
                else if (IncludeHigher)
                    if (key.ToString().Length > allowedRating)
                        songList.AddRange(ratingDictionary[key]);
            }

            base.songPaths = songList.Select(x => x.Path).ToList();
        }

        public override void UpdateSongInfo(Song song)
        {
            //Remove from all dictionaries
            foreach (Dictionary<string, List<Song>> dictionary in this.sortDictionaries)
                foreach (List<Song> list in dictionary.Values)
                    if (list.Contains(song))
                        list.Remove(song);

            if ((song.Rating == allowedRating) || (IncludeHigher && (song.Rating > allowedRating)))
                Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
        }

        public override Song GetRandom()
        {
            return libraryDictionary[songPaths[Playlist.r.Next(0, songPaths.Count)]];
        }

        public override void Save()
        {
            try
            {
                Stream stream = File.Create(this.Path);
                XmlSerializer serializer = new XmlSerializer(typeof(RatingFilterPlaylist));
                serializer.Serialize(stream, this);
                stream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save play list exception: " + ex.ToString());
            }
        }

        /// <summary>
        /// Load playlist from file. Returns null if it fails:
        /// </summary>
        /// <param name="path"></param>
        /// <param name="library"></param>
        /// <returns></returns>
        public static new RatingFilterPlaylist Load(String path, Library library)
        {
            Stream stream = null;
            RatingFilterPlaylist loadedPlaylist = null;
            try
            {
                stream = File.OpenRead(path);
                XmlSerializer serializer = new XmlSerializer(typeof(RatingFilterPlaylist));
                loadedPlaylist = (RatingFilterPlaylist)serializer.Deserialize(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
            RatingFilterPlaylist pl = new RatingFilterPlaylist(library, loadedPlaylist.Name, 
                new List<string>(), loadedPlaylist.AllowedRating, loadedPlaylist.IncludeHigher);
            library.LibraryChanged += pl.library_LibraryChanged;
            return pl;
        }
    }
}
