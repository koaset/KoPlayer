using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KoPlayer.Lib
{

    public class Playlist : PlaylistBase
    {
        public override string Path { get { return @"Playlists\" + Name + ".pl"; } }

        private const string EXTENSION = ".mp3";

        protected Dictionary<string, Song> libraryDictionary;

        protected Playlist() { }

        public Playlist(Library library)
            : this(library, "PlaylistName") { }

        public Playlist(Library library, string name)
            : this(library, name, new List<Song>()) { }

        public Playlist(Library library, string name, List<Song> songs)
        {
            this.Name = name;
            this.songs = songs;
            
            Init(library);
        }

        protected void Init(Library library)
        {
            this.libraryDictionary = library.PathDictionary;
            library.Changed += library_LibraryChanged;

            ResetSortVariables();

            this.sortDictionaries = new List<Dictionary<string, List<Song>>>();
            Sorting.CreateSortDictionaries(this.songs, this.sortDictionaries);
        }

        protected List<Song> GetSongsFromLibrary()
        {
            
            var ret = new List<Song>();
            if (songs.Count == 0)
                return ret;
            var songsToBeRemoved = new List<Song>();
            foreach (Song s in songs)
            {
                if (libraryDictionary.ContainsKey(s.Path))
                    ret.Add(libraryDictionary[s.Path]);
                else
                    songsToBeRemoved.Add(s);
            }

            foreach (Song s in songsToBeRemoved)
                songs.Remove(s);

            return ret;
        }

        protected void library_LibraryChanged(object sender, EventArgs e)
        {
            List<int> toBeRemoved = new List<int>();
            for (int i = 0; i < songs.Count; i++)
            {
                if (!libraryDictionary.ContainsKey(songs[i].Path))
                    toBeRemoved.Add(i);
            }
            toBeRemoved.Sort();
            toBeRemoved.Reverse();
            foreach (int i in toBeRemoved)
                this.Remove(i);
        }

        public override void Add(string path)
        {
            if (!libraryDictionary.Keys.Contains(path))
                return;

            Add(libraryDictionary[path]);
        }

        public void Add(Song song)
        {
            if (song == null)
                return;

            songs.Add(song);
            Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
        }

        public override void Add(List<Song> songs)
        {
            foreach (Song s in songs)
                Add(s);
        }

        public virtual void Insert(int index, List<Song> songs)
        {
            if (index < this.songs.Count)
                this.songs.InsertRange(index, songs);
            else
                this.songs.InsertRange(this.songs.Count - 1, songs);
        }

        public override void Remove(int index)
        {
            if (CurrentIndex > index)
                CurrentIndex--;

            Sorting.RemoveSongFromSortDictionaries(this.songs[index], this.sortDictionaries);
            this.songs.RemoveAt(index);
        }

        public void Remove(string path)
        {
            Song song;
            while ((song = songs.Find(s => s.Path == path)) != null)
            {
                int index = songs.IndexOf(song);
                if (CurrentIndex > index)
                    CurrentIndex--;
                songs.RemoveAt(index);
            }
        }

        public override void Remove(List<int> indexes)
        {
            foreach (int i in indexes)
                Remove(i);
        }

        public void Remove(Song song)
        {
            Remove(song.Path);
        }

        public override void Remove(List<Song> songs)
        {
            foreach (Song s in songs)
                Remove(s.Path);
        }

        public override void RemoveAll()
        {
            songs.Clear();
            this.CurrentIndex = 0;
            Sorting.CreateSortDictionaries(this.songs, this.sortDictionaries);
        }

        public override void UpdateSongInfo(Song song)
        {
            if (!songs.Contains(song))
                return;

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

        public override void Sort(int columnIndex, string field)
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
            this.songs = Sorting.Sort(field, this.sortOrder, this.sortDictionaries, this.songs);
        }

        public override void Save()
        {
            try
            {
                using (var sw = new StreamWriter(Path))
                {
                    sw.WriteLine(GetType().Name);
                    SaveHeader(sw);
                    SaveSongs(sw);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex);
                MessageBox.Show("Save play list exception: " + ex.ToString());
            }
        }

        protected virtual void SaveSongs(StreamWriter sw)
        {
            foreach (string path in songs.Select(s => s.Path))
                sw.WriteLine(path);
        }

        protected virtual void SaveHeader(StreamWriter sw)
        {
            sw.WriteLine(Name);
            sw.WriteLine(CurrentIndex);
        }

        public Playlist(StreamReader sr, Library library)
        {
            ReadHeader(sr);

            var paths = new List<string>();
            string current;
            while ((current = sr.ReadLine()) != null)
                paths.Add(current);

            songs = new List<Song>();
            foreach (string path in paths)
                songs.Add(library.PathDictionary[path]);

            Init(library);
        }

        protected virtual void ReadHeader(StreamReader sr)
        {
            Name = sr.ReadLine();
            CurrentIndex = int.Parse(sr.ReadLine());
        }

        public override string ToString()
        {
            return "Playlist: " + this.Name + ", " + this.NumSongs + " songs.";
        }
    }
}
