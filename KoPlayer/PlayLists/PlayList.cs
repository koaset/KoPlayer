using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KoPlayer.Playlists
{

    public class Playlist : IPlaylist
    {
        public static Random r = new Random();

        [XmlIgnore]
        public virtual string Path { get { return @"Playlists\" + Name + ".pl"; } }
        public string Name { get; set; }
        [XmlIgnore]
        public int SortColumnIndex { get; set; }
        [XmlIgnore]
        public int NumSongs { get { return songPaths.Count; } }
        public int CurrentIndex { get; set; }

        private const string EXTENSION = ".mp3";

        protected BindingList<Song> outputSongs;
        public List<string> songPaths;
        protected Dictionary<string, Song> libraryDictionary;

        public SortOrder SortOrder { get { return sortOrder; } }
        private SortOrder sortOrder;
        private string sortField = "";

        protected List<Dictionary<string, List<Song>>> sortDictionaries;

        protected Playlist() { }

        public Playlist(Library library)
            : this(library, "PlaylistName") { }

        public Playlist(Library library, string name)
            : this(library, name, new List<string>()) { }

        public Playlist(Library library, string name, List<string> songPaths)
        {
            this.libraryDictionary = library.PathDictionary;
            library.LibraryChanged += library_LibraryChanged;
            this.Name = name;
            this.songPaths = songPaths;
            ResetSortVariables();

            this.outputSongs = GetSongsFromLibrary();
            this.sortDictionaries = new List<Dictionary<string, List<Song>>>();
            Sorting.CreateSortDictionaries(this.outputSongs, this.sortDictionaries);
        }

        private BindingList<Song> GetSongsFromLibrary()
        {
            BindingList<Song> ret = new BindingList<Song>();
            if (songPaths.Count == 0)
                return ret;
            List<string> pathsToBeRemoved = new List<string>();
            foreach (string songPath in songPaths)
            {
                if (libraryDictionary.ContainsKey(songPath))
                    ret.Add(libraryDictionary[songPath]);
                else
                    pathsToBeRemoved.Add(songPath);
            }

            foreach (string path in pathsToBeRemoved)
                songPaths.Remove(path);

            return ret;
        }

        protected void library_LibraryChanged(object sender, LibraryChangedEventArgs e)
        {
            List<int> toBeRemoved = new List<int>();
            for (int i = 0; i < songPaths.Count; i++)
            {
                if (!libraryDictionary.ContainsKey(songPaths[i]))
                    toBeRemoved.Add(i);
            }
            toBeRemoved.Sort();
            toBeRemoved.Reverse();
            foreach (int i in toBeRemoved)
                this.Remove(i);
        }

        public void Add(string path)
        {
            if (libraryDictionary.Keys.Contains(path))
                Add(libraryDictionary[path]);
        }

        public void Add(Song song)
        {
            if (song != null)
            {
                songPaths.Add(song.Path);
                this.outputSongs.Add(libraryDictionary[song.Path]);
                Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
            }
        }

        public void Add(List<Song> songs)
        {
            foreach (Song s in songs)
                Add(s);
        }

        public void Insert(int index, string song)
        {
            if (index < songPaths.Count)
            {
                if (index < CurrentIndex)
                    CurrentIndex++;
                if (index == CurrentIndex)
                    songPaths.Insert(index + 1, song);
                else
                    songPaths.Insert(index, song);
            }
            else
                songPaths.Add(song);
        }

        public void Insert(int index, Song song)
        {
            this.Insert(index, song.Path);
        }

        public void Insert(int index, List<Song> songs)
        {
            foreach (Song s in songs)
                this.Insert(index, s);
        }

        public void Remove(int index)
        {
            if (CurrentIndex > index)
                CurrentIndex--;
            this.songPaths.RemoveAt(index);

            Sorting.RemoveSongFromSortDictionaries(this.outputSongs[index], this.sortDictionaries);
            if (index < outputSongs.Count)
                this.outputSongs.RemoveAt(index);
        }

        public void Remove(string path)
        {
            int index;
            while ((index = songPaths.IndexOf(path)) > -1)
            {
                if (CurrentIndex > index)
                    CurrentIndex--;
                songPaths.RemoveAt(index);
            }
        }

        public void Remove(List<int> indexes)
        {
            foreach (int i in indexes)
                Remove(i);
        }

        public void Remove(Song song)
        {
            Remove(song.Path);
        }

        public void Remove(List<Song> songs)
        {
            foreach (Song s in songs)
                Remove(s.Path);
        }

        public void RemoveAll()
        {
            this.songPaths = new List<string>();
            this.outputSongs = new BindingList<Song>();
            this.CurrentIndex = 0;
            Sorting.CreateSortDictionaries(this.outputSongs, this.sortDictionaries);
        }

        public Song GetNext()
        {
            if (songPaths.Count == 0)
                return null;
            if (CurrentIndex + 1 < songPaths.Count)
                return libraryDictionary[songPaths[++CurrentIndex]];
            else
            {
                CurrentIndex = 0;
                return libraryDictionary[songPaths[CurrentIndex]];
            }
        }

        public Song GetPrevious()
        {
            if (songPaths.Count == 0)
                return null;
            if (CurrentIndex > 0)
                return libraryDictionary[songPaths[--CurrentIndex]];
            else
            {
                CurrentIndex = songPaths.Count - 1;
                return libraryDictionary[songPaths[CurrentIndex]];
            }
        }

        public virtual void UpdateSongInfo(Song song)
        {
            RemoveFromSortDictionaries(song);
            Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
        }

        protected void RemoveFromSortDictionaries(Song song)
        {
            foreach (Dictionary<string, List<Song>> dictionary in this.sortDictionaries)
                foreach (List<Song> list in dictionary.Values)
                    if (list.Contains(song))
                        list.Remove(song);
        }

        public void Sort(int columnIndex, string field)
        {
            if (this.SortColumnIndex == columnIndex)
            {
                if (this.sortOrder == SortOrder.None || this.sortOrder == SortOrder.Descending)
                    this.sortOrder = SortOrder.Ascending;
                else
                    this.sortOrder = SortOrder.Descending;
            }
            else
                this.sortOrder = SortOrder.Ascending;
            this.SortColumnIndex = columnIndex;
            this.sortField = field;
            this.outputSongs = Sorting.Sort(field, this.sortOrder, this.sortDictionaries, this.outputSongs);
        }

        private void ResetSortVariables()
        {
            this.sortOrder = System.Windows.Forms.SortOrder.None;
            this.SortColumnIndex = -1;
        }

        public BindingList<Song> GetSongs()
        {
            return outputSongs;
        }

        public virtual BindingList<Song> GetAllSongs()
        {
            ResetSortVariables();
            this.outputSongs = GetSongsFromLibrary();
            return outputSongs;
        }

        public virtual Song GetRandom()
        {
            return libraryDictionary[songPaths[Playlist.r.Next(0, songPaths.Count)]];
        }

        public virtual void Save()
        {
            try
            {
                Stream stream = File.Create(this.Path);
                XmlSerializer serializer = new XmlSerializer(typeof(Playlist));
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
        public static Playlist Load(String path, Library library)
        {
            Stream stream = null;
            Playlist loadedPlaylist = null;
            try
            {
                stream = File.OpenRead(path);
                XmlSerializer serializer = new XmlSerializer(typeof(Playlist));
                loadedPlaylist = (Playlist)serializer.Deserialize(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
            Playlist pl = new Playlist(library, loadedPlaylist.Name, loadedPlaylist.songPaths);
            library.LibraryChanged += pl.library_LibraryChanged;
            pl.CurrentIndex = loadedPlaylist.CurrentIndex;

            List<string> toBeRemoved = new List<string>();
            foreach (string filePath in pl.songPaths)
                if (!library.PathDictionary.Keys.Contains(filePath))
                    toBeRemoved.Add(filePath);
            foreach (string filePath in toBeRemoved)
                pl.Remove(filePath);

            return pl;
        }

        public override string ToString()
        {
            return "Playlist: " + this.Name + ", " + this.NumSongs + " songs.";
        }
    }
}
