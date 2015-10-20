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

        public static readonly string[] sortColumns = { "title", "artist", 
                                                           "album", "genre", 
                                                           "rating", "play count", 
                                                           "length", "date added", 
                                                           "last played"};
        private List<Dictionary<string, List<Song>>> sortDictionaries;

        private List<Song> searchResults;

        private const string PATH = "Library.xml";
        private const string EXTENSION = ".mp3";

        private List<Song> songs;
        private Dictionary<string, Song> pathDictionary;

        public SortOrder SortOrder { get { return sortOrder; } }
        private SortOrder sortOrder;

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
                this.songs = new List<Song>(songs);
            else
                this.songs = new List<Song>();
            this.pathDictionary = SongListToDictionary();
            this.CurrentIndex = 0;

            sortDictionaries = new List<Dictionary<string, List<Song>>>();

            CreateSortDictionaries();
        }

        private void CreateSortDictionaries()
        {
            for (int i = 0; i < sortColumns.Length; i++)
                sortDictionaries.Add(CreateFieldDictionary(this.songs, sortColumns[i]));
        }

        public Song Get(string path)
        {
            throw new NotImplementedException();
        }

        public void Sort(string field)
        {
            if (sortOrder == SortOrder.None || sortOrder == SortOrder.Descending)
                sortOrder = SortOrder.Ascending;
            else
                sortOrder = SortOrder.Descending;
            Sort(sortOrder, field);
        }

        private void Sort(SortOrder sortOrder, string field)
        {
            this.sortOrder = sortOrder;

            Dictionary<string, List<Song>> sortDictionary = GetDictionary(field);
            if (sortDictionary == null)
                throw new PlayListException("Invalid field name");

            SortedDictionary<string, List<Song>> sortedDictionary;
            sortedDictionary = new SortedDictionary<string, List<Song>>(sortDictionary);

            IEnumerable<string> keys = sortedDictionary.Keys;
            if (sortOrder == SortOrder.Descending)
                keys = keys.Reverse();
            
            List<Song> songList = new List<Song>();

            foreach (string key in keys)
            {
                if (field.ToLower() == "album" || field.ToLower() == "artist")
                {
                    sortedDictionary[key].Sort(delegate(Song song1, Song song2)
                    {
                        return song1.CompareTo(song2);
                    });
                }
                songList.AddRange(sortedDictionary[key]);
            }

            this.songs = songList;
        }

        private Dictionary<string, List<Song>> GetDictionary(string field)
        {
            for (int i = 0; i < sortColumns.Length; i++)
            {
                if (field.ToLower() == sortColumns[i].ToLower())
                    return sortDictionaries[i];
            }
            return null;
        }

        private Dictionary<string, List<Song>> GetPathListDictionary()
        {
            Dictionary<string, List<Song>> ret = new Dictionary<string, List<Song>>();
            foreach (string s in pathDictionary.Keys)
            {
                List<Song> list = new List<Song>();
                list.Add(pathDictionary[s]);
                ret.Add(s, list);
            }
            return ret;
        }

        public List<Song> GetAll()
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
            Save();
            CreateSortDictionaries();
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
        /// <param name="searchString"></param>
        /// <returns></returns>
        public BindingList<Song> Search(string searchString)
        {
            searchString = searchString.ToLower().Trim();

            this.searchResults = new List<Song>();
            for (int i = 0; i < 4; i++)
                AddUniqueSearchResults(searchString, sortDictionaries[i]);
            return new BindingList<Song>(searchResults);
        }

        /// <summary>
        /// Searches a dictionary for the searchterm and returns a list of not yet added songs
        /// </summary>
        /// <param name="keysToAdd"></param>
        /// <param name="dictionary"></param>
        /// <param name="addedSongs"></param>
        /// <returns></returns>
        private void AddUniqueSearchResults(string searchString,
            Dictionary<string, List<Song>> dictionary)
        {
            List<string> keysToAdd = WholeStringKeySearch(searchString, dictionary);

            foreach (string keyToAdd in keysToAdd)
            {
                List<Song> currentList = dictionary[keyToAdd];
                foreach (Song s in currentList)
                    if (!this.searchResults.Contains(s))
                        this.searchResults.Add(s);
            }
        }

        /// <summary>
        /// Divides the search string into words and returns all keys where the field contains all words
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private List<string> SeparateWordAndSearch(string searchString,
            Dictionary<string, List<Song>> dictionary)
        {
            string[] searchTerms = searchString.Split(' ');
            List<List<string>> matchingKeyLists = new List<List<string>>();
            foreach (string term in searchTerms)
            {
                if (term.Length > 0)
                {
                    List<string> matchingKeyList = dictionary.Where(x => x.Key.ToLower().Contains(term)).Select(x => x.Key).ToList();
                    matchingKeyLists.Add(matchingKeyList);
                }
            }

            List<string> keysToAdd = new List<string>();

            if (matchingKeyLists.Count > 1)
            {
                List<string> comparer = matchingKeyLists[0];
                matchingKeyLists.Remove(comparer);
                foreach (string key in comparer)
                {
                    bool inAll = true;
                    foreach (List<string> otherList in matchingKeyLists)
                    {
                        if (!otherList.Contains(key))
                            inAll = false;
                    }
                    if (inAll)
                        keysToAdd.Add(key);
                }
            }
            else
                keysToAdd.AddRange(matchingKeyLists[0]);
            return keysToAdd;
        }

        /// <summary>
        /// Divides the search string into words and returns all keys contaning any word
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private List<string> SeparateWordOrSearch(string searchString,
            Dictionary<string, List<Song>> dictionary)
        {
            string[] searchTerms = searchString.Split(' ');
            List<List<string>> matchingKeyLists = new List<List<string>>();
            foreach (string term in searchTerms)
            {
                if (term.Length > 0)
                {
                    List<string> matchingKeyList = dictionary.Where(x => x.Key.ToLower().Contains(term)).Select(x => x.Key).ToList();
                    matchingKeyLists.Add(matchingKeyList);
                }
            }
            List<string> keysToAdd = new List<string>();

            //Must contain any search term in any field
            
            keysToAdd.AddRange(matchingKeyLists[0]);
            matchingKeyLists.Remove(matchingKeyLists[0]);
            if (matchingKeyLists.Count > 0)
            {
                foreach (List<string> matchingKeyList in matchingKeyLists)
                    foreach (string key in matchingKeyList)
                        if (!keysToAdd.Contains(key))
                            keysToAdd.Add(key);
            }
            return keysToAdd;
        }

        /// <summary>
        /// Gets all keys containing the whole search string in one piece
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        private List<string> WholeStringKeySearch(string searchString,
            Dictionary<string, List<Song>> dictionary)
        {
            return dictionary.Where(x => x.Key.ToLower().Contains(searchString)).Select(x => x.Key).ToList();
        }

        private static Dictionary<string, List<Song>> CreateFieldDictionary(List<Song> songs, string field)
        {
            Dictionary<string, List<Song>> fieldDictionary = new Dictionary<string, List<Song>>();
            foreach (Song s in songs)
                AddToFieldDictionary(fieldDictionary, field, s);
            return fieldDictionary;
        }

        private static void AddToFieldDictionary(Dictionary<string, List<Song>> dictionary, string field, Song song)
        {
            if (song[field] != null && dictionary.ContainsKey(song[field]))
                dictionary[song[field]].Add(song);
            else
            {
                List<Song> newEntry = new List<Song>();
                newEntry.Add(song);
                if (song[field] != null)
                    dictionary.Add(song[field], newEntry);
            }
        }

        private void AddSongToFieldDictionaries(Song song)
        {
            for (int i = 0; i < sortColumns.Length; i++)
                AddToFieldDictionary(sortDictionaries[i], sortColumns[i], song);
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
            for (int i = 0; i < sortColumns.Length; i++)
                RemoveFromFieldDictionary(sortDictionaries[i], sortColumns[i], song);
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
