using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

namespace KoPlayer.PlayLists
{
    public delegate void ReportProgressEventHandler(object sender, ReportProgressEventArgs e);
    public delegate void LibraryChangedEventHandler(object sender, LibraryChangedEventArgs e);

    public class Library : IPlayList
    {
        public event ReportProgressEventHandler ReportProgress;
        public event LibraryChangedEventHandler LibraryChanged;

        public string Name { get { return "Library"; } }
        public string Path { get { return PATH; } }
        public Dictionary<string, Song> Dictionary { get { return pathDictionary; } }

        private Dictionary<string, List<Song>> titleDictionary;
        private Dictionary<string, List<Song>> artistDictionary;
        private Dictionary<string, List<Song>> albumDictionary;
        private Dictionary<string, List<Song>> genreDictionary;
        private List<Song> searchResults;

        private const string PATH = "Library.xml";
        private const string EXTENSION = ".mp3";

        private BindingList<Song> songs;
        private Dictionary<string, Song> pathDictionary;

        public int NumSongs
        {
            get { return this.songs.Count; }
        }

        [XmlIgnore]
        public int CurrentIndex { get; set; }

        public Library()
            : this(null) {}

        public Library(List<Song> songs)
        {
            if (songs != null)
                this.songs = new BindingList<Song>(songs);
            else
                this.songs = new BindingList<Song>();
            this.pathDictionary = SongListToDictionary();
            this.CurrentIndex = 0;

            this.titleDictionary = CreateFieldDictionary(this.songs, "title");
            this.artistDictionary = CreateFieldDictionary(this.songs, "artist");
            this.albumDictionary = CreateFieldDictionary(this.songs, "album");
            this.genreDictionary = CreateFieldDictionary(this.songs, "genre");
        }

        public Song Get(string path)
        {
            throw new NotImplementedException();
        }

        public BindingList<Song> GetAll()
        {
            return songs;
        }

        public void Add(string path)
        {
            Add(new Song(path));
        }

        public void Add(Song song)
        {
            if (song != null)
                if (!pathDictionary.ContainsKey(song.Path))
                {
                    songs.Add(song);
                    this.pathDictionary.Add(song.Path, song);
                    AddSongToFieldDictionaries(song);
                    OnLibraryChanged(new LibraryChangedEventArgs());
                }
        }

        public void Add(List<Song> songs)
        {
            foreach (Song s in songs)
                Add(s);
        }

        public void Remove(int index)
        {
            RemoveSongFromFieldDictionaries(songs[index]);
            this.pathDictionary.Remove(songs[index].Path);
            songs.Remove(songs[index]);
            
            OnLibraryChanged(new LibraryChangedEventArgs());
        }

        public void Remove(Song song)
        {
            Remove(song.Path);
        }

        public void Remove(string path)
        {
            Song toBeRemoved = null;
            foreach (Song s in songs)
                if (s.Path == path)
                    toBeRemoved = s;
            RemoveSongFromFieldDictionaries(toBeRemoved);
            songs.Remove(toBeRemoved);
            this.pathDictionary.Remove(toBeRemoved.Path);
            OnLibraryChanged(new LibraryChangedEventArgs());
        }

        public Song GetNext()
        {
            if (songs.Count == 0)
                return null;
            if (CurrentIndex + 1 < songs.Count)
                return songs[++CurrentIndex];
            else
            {
                CurrentIndex = 0;
                return songs[CurrentIndex];
            }
        }

        public Song GetPrevious()
        {
            if (songs.Count == 0)
                return null;
            if (CurrentIndex > 0)
                return songs[--CurrentIndex];
            else
            {
                CurrentIndex = songs.Count - 1;
                return songs[CurrentIndex];
            }
        }

        public Song GetRandom()
        {
            return songs[PlayList.r.Next(0, songs.Count)];
        }


        public void AddFolder(string path)
        {
            songs.RaiseListChangedEvents = false;
            
            BackgroundWorker addFilesWorker = new BackgroundWorker();
            addFilesWorker.WorkerSupportsCancellation = false;
            addFilesWorker.WorkerReportsProgress = true;
            addFilesWorker.DoWork += addFilesWorker_DoWork;
            addFilesWorker.ProgressChanged += addFilesWorker_ProgressChanged;
            addFilesWorker.RunWorkerCompleted += addFilesWorker_RunWorkerCompleted;
            addFilesWorker.RunWorkerAsync(path);
        }

        void addFilesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string path = e.Argument as string;
            try
            {
                string[] mp3Files = Directory.GetFiles(path, "*" + EXTENSION, SearchOption.AllDirectories);
                int count = 0;
                foreach (string fileName in mp3Files)
                {
                    if (!Exists(fileName))
                    {
                        Song newSong = new Song(fileName);
                        this.songs.Add(newSong);
                        this.pathDictionary.Add(fileName, newSong);
                        if (count % 50 == 0)
                            worker.ReportProgress((int)(100 * count / mp3Files.Length));
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Add folder exception: " + ex.ToString());
            }
            worker.ReportProgress(100);
            songs.RaiseListChangedEvents = true;
        }

        void addFilesWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnReportProgress(new ReportProgressEventArgs(e.ProgressPercentage));
        }

        protected virtual void OnReportProgress(ReportProgressEventArgs e)
        {
            if (ReportProgress != null)
                ReportProgress(this, e);
        }

        protected virtual void OnLibraryChanged(LibraryChangedEventArgs e)
        {
            if (LibraryChanged != null)
                LibraryChanged(this, e);
        }

        void addFilesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.songs.ResetBindings();
            Save();
            this.titleDictionary = CreateFieldDictionary(this.songs, "title");
            this.artistDictionary = CreateFieldDictionary(this.songs, "artist");
            this.albumDictionary = CreateFieldDictionary(this.songs, "album");
            this.genreDictionary = CreateFieldDictionary(this.songs, "genre");
            OnLibraryChanged(new LibraryChangedEventArgs());
        }

        private bool Exists(string path)
        {
            foreach (Song s in songs)
                if (s.Path == path)
                    return true;
            return false;
        }

        public void RemoveFolder(string path)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            Stream stream = File.Create(PATH);
            XmlSerializer serializer = new XmlSerializer(typeof(List<Song>));
            serializer.Serialize(stream, songs.ToList());
            stream.Close();
        }

        /// <summary>
        /// Load playlist from file. Returns null if it fails:
        /// </summary>
        /// <param name="path"></param>
        /// <param name="library"></param>
        /// <returns></returns>
        public static Library Load()
        {
            Stream stream = null;
            List<Song> loadedLibrary = null;
            try
            {
                stream = File.OpenRead(PATH);
                XmlSerializer serializer = new XmlSerializer(typeof(List<Song>));
                loadedLibrary = (List<Song>)serializer.Deserialize(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
            return new Library(loadedLibrary);
        }

        /// <summary>
        /// Searches for string in all string fields and returns matches in a list
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public BindingList<Song> Search(string searchTerm)
        {
            searchTerm = searchTerm.ToLower().Trim();

            this.searchResults = new List<Song>();
            AddUniqueSearchResults(searchTerm, titleDictionary);
            AddUniqueSearchResults(searchTerm, artistDictionary);
            AddUniqueSearchResults(searchTerm, albumDictionary);
            AddUniqueSearchResults(searchTerm, genreDictionary);
            return new BindingList<Song>(searchResults);
        }

        /// <summary>
        /// Searches a dictionary for the searchterm and returns a list of not yet added songs
        /// </summary>
        /// <param name="keysToAdd"></param>
        /// <param name="dictionary"></param>
        /// <param name="addedSongs"></param>
        /// <returns></returns>
        private void AddUniqueSearchResults(string searchTerm,
            Dictionary<string, List<Song>> dictionary)
        {
            List<string> keysToAdd = dictionary.Where(x => x.Key.ToLower().Contains(searchTerm)).Select(x => x.Key).ToList();

            foreach (string keyToAdd in keysToAdd)
            {
                List<Song> currentList = dictionary[keyToAdd];
                foreach (Song s in currentList)
                    if (!this.searchResults.Contains(s))
                        this.searchResults.Add(s);
            }
        }

        private static Dictionary<string, List<Song>> CreateFieldDictionary(BindingList<Song> songs, string field)
        {
            Dictionary<string, List<Song>> fieldDictionary = new Dictionary<string, List<Song>>();
            foreach (Song s in songs)
                AddToFieldDictionary(fieldDictionary, field, s);
            return fieldDictionary;
        }

        private static void AddToFieldDictionary(Dictionary<string, List<Song>> dictionary, string field, Song song)
        {
            if (dictionary.ContainsKey(song[field]))
                dictionary[song[field]].Add(song);
            else
            {
                List<Song> newEntry = new List<Song>();
                newEntry.Add(song);
                dictionary.Add(song[field], newEntry);
            }
        }

        private void AddSongToFieldDictionaries(Song song)
        {
            AddToFieldDictionary(this.titleDictionary, "title", song);
            AddToFieldDictionary(this.artistDictionary, "artist", song);
            AddToFieldDictionary(this.albumDictionary, "album", song);
            AddToFieldDictionary(this.genreDictionary, "genre", song);
        }

        private static void RemoveFromFieldDictionary(Dictionary<string, List<Song>> dictionary, string field, Song song)
        {
            if (dictionary.ContainsKey(song[field]))
            {
                dictionary[song[field]].Remove(song);
                if (dictionary[song[field]].Count == 0)
                    dictionary.Remove(song[field]);
            }
        }

        private void RemoveSongFromFieldDictionaries(Song song)
        {
            RemoveFromFieldDictionary(this.titleDictionary, "title", song);
            RemoveFromFieldDictionary(this.artistDictionary, "artist", song);
            RemoveFromFieldDictionary(this.albumDictionary, "album", song);
            RemoveFromFieldDictionary(this.genreDictionary, "genre", song);
        }

        private Dictionary<string, Song> SongListToDictionary()
        {
            return songs.ToDictionary(x => x.Path, x => x);
        }
    }

    public class ReportProgressEventArgs : EventArgs
    {
        public int ProgressPercentage { get; set; }
        public ReportProgressEventArgs(int progressPercentage)
            : base()
        {
            this.ProgressPercentage = progressPercentage;
        }
    }

    public class LibraryChangedEventArgs : EventArgs
    {
        public LibraryChangedEventArgs()
            : base() { }
    }
}
