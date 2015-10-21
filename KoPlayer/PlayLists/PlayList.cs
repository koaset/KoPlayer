using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.ComponentModel;

namespace KoPlayer.PlayLists
{

    public class PlayList : IPlayList
    {
        public static Random r = new Random();

        [XmlIgnore]
        public string Path { get { return @"Playlists\" + Name + ".xml"; } }
        public string Name { get; set; }
        public int SortColumnIndex { get; set; }

        private const string EXTENSION = ".mp3";

        [XmlIgnore]
        public int NumSongs { get { return songPaths.Count; } }

        public int CurrentIndex { get; set; }
        public List<string> songPaths;
        protected Dictionary<string, Song> libraryDictionary;

        public SortOrder SortOrder { get { return sortOrder; } }
        private SortOrder sortOrder;

        protected PlayList() { }

        public PlayList(Library library)
            : this(library, "PlayListName") { }

        public PlayList(Library library, string name)
            : this(library, name, new List<string>()) { }

        public PlayList(Library library, string name, List<string> songPaths)
        {
            this.libraryDictionary = library.Dictionary;
            library.LibraryChanged += library_LibraryChanged;
            this.Name = name;
            this.songPaths = songPaths;
        }

        void library_LibraryChanged(object sender, LibraryChangedEventArgs e)
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
            songPaths.Add(path);
        }

        public void Add(Song song)
        {
            if (song != null)
                songPaths.Add(song.Path);
        }

        public void Add(List<Song> songs)
        {
            foreach (Song s in songs)
                Add(s);
        }

        public void Insert(int index, string song)
        {
            if (index < songPaths.Count)
                songPaths.Insert(index, song);
            else
                songPaths.Add(song);
        }

        public void Insert(int index, Song song)
        {
            if (song != null)
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
            songPaths.RemoveAt(index);
        }

        public void Remove(string path)
        {
            throw new NotImplementedException();
        }

        public void Remove(Song song)
        {
            Remove(song.Path);
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

        public void AddFolder(string path)
        {
            try
            {
                string[] mp3Files = Directory.GetFiles(path, "*" + EXTENSION, SearchOption.AllDirectories);
                foreach (string fileName in mp3Files)
                    songPaths.Add(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Add folder to playlist exception: " + ex.ToString());
            }
        }

        public void RemoveFolder(string path)
        {
            throw new NotImplementedException();
        }

        public void Sort(int columnindex, string field)
        {
 
        }

        public BindingList<Song> GetSongs()
        {
            if (songPaths.Count == 0)
                return null;
            List<Song> songs = new List<Song>();
            foreach (string songPath in songPaths)
                if (libraryDictionary.ContainsKey(songPath))
                    songs.Add(libraryDictionary[songPath]);
            return new BindingList<Song>(songs);
        }

        public BindingList<Song> GetAllSongs()
        {
            return GetSongs();
        }

        public Song Get(string path)
        {
            throw new NotImplementedException();
        }

        public Song GetRandom()
        {
            return libraryDictionary[songPaths[PlayList.r.Next(0, songPaths.Count)]];
        }

        public void Save()
        {
            try
            {
                Stream stream = File.Create(this.Path);
                XmlSerializer serializer = new XmlSerializer(typeof(PlayList));
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
        public static PlayList Load(String path, Library library)
        {
            Stream stream = null;
            PlayList loadedPlayList = null;
            try
            {
                stream = File.OpenRead(path);
                XmlSerializer serializer = new XmlSerializer(typeof(PlayList));
                loadedPlayList = (PlayList)serializer.Deserialize(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
            loadedPlayList.libraryDictionary = library.Dictionary;
            library.LibraryChanged += loadedPlayList.library_LibraryChanged;
            return loadedPlayList;
        }
    }
}
