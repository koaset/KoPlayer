using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KoPlayer.Playlists
{
    public delegate void ReportProgressEventHandler(object sender, ReportProgressEventArgs e);
    public delegate void LibraryChangedEventHandler(object sender, LibraryChangedEventArgs e);

    public class Library : IPlaylist
    {
        public event ReportProgressEventHandler ReportProgress;
        public event LibraryChangedEventHandler LibraryChanged;

        public string Name { get { return "Library"; } }
        public string Path { get { return PATH; } }
        public int SortColumnIndex { get; set; }
        public Dictionary<string, Song> PathDictionary { get { return pathDictionary; } }
        public List<Dictionary<string, List<Song>>> SortDictionaries { get { return this.sortDictionaries; } }
        public SortOrder SortOrder { get { return sortOrder; } }

        public static string PATH = KoPlayer.Forms.MainForm.ApplicationPath + "\\Library.xml";
        public static string[] EXTENSIONS = {".mp3", ".m4a", ".wma", ".aac", ".flac"};

        private BindingList<Song> outputSongs;
        private Dictionary<string, Song> pathDictionary;
        private List<Dictionary<string, List<Song>>> sortDictionaries;
        private List<Song> newSongs;
        private List<string> invalidSongPaths;

        private SortOrder sortOrder = SortOrder.None;
        private string sortField = "";
        private bool raiseLibraryChangedEvent = true;

        public Dictionary<string, List<Song>> GetSortDictionary(string field)
        {
            field = field.ToLower();
            if (Sorting.SortColumns.Contains(field))
                return sortDictionaries[Array.IndexOf(Sorting.SortColumns, field)];
            else
                throw new PlaylistException("Sortdictionary field does not exist");
        }

        public int NumSongs
        {
            get { return this.pathDictionary.Count; }
        }

        [XmlIgnore]
        public int CurrentIndex { get; set; }

        public Library()
            : this(null) {}

        public Library(List<Song> songs)
        {
            if (songs != null)
                this.outputSongs = new BindingList<Song>(songs);
            else
                this.outputSongs = new BindingList<Song>();

            this.pathDictionary = SongListToPathDictionary();
            this.CurrentIndex = 0;
            ResetSortVariables();

            sortDictionaries = new List<Dictionary<string, List<Song>>>();
            Sorting.CreateSortDictionaries(this.outputSongs, this.sortDictionaries);
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
            //this.outputSongs = Sorting.SortBindingList(this.outputSongs, this.sortOrder, field);

            this.outputSongs = Sorting.Sort(field, this.sortOrder, this.sortDictionaries, this.outputSongs);
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

        public void UpdateSongInfo(Song song)
        {
            //Remove from all dictionaries
            foreach (Dictionary<string, List<Song>> dictionary in this.sortDictionaries)
                foreach (List<Song> list in dictionary.Values)
                    if (list.Contains(song))
                        list.Remove(song);

            Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
        }

        public BindingList<Song> GetSongs()
        {
            return outputSongs;
        }

        public BindingList<Song> GetAllSongs()
        {
            //ResetSortVariables();
            this.outputSongs = GetSongsFromPathDictionary();
            return GetSongs();
        }

        private BindingList<Song> GetSongsFromPathDictionary()
        {
            return new BindingList<Song>(pathDictionary.Values.ToList());
        }

        private void ResetSortVariables()
        {
            this.sortOrder = System.Windows.Forms.SortOrder.None;
            this.SortColumnIndex = -1;
        }

        public void Add(string path)
        {
            Song newSong = null;
            try
            {
                newSong = new Song(path);
            }
            catch
            {
                MessageBox.Show("File read error");
            }
            if (newSong != null)
                Add(newSong);   
        }

        public void ResetSearchDictionaries()
        {
            Sorting.CreateSortDictionaries(this.outputSongs, this.sortDictionaries);
        }

        public void ResetRatingDictionary()
        {
            int index = Array.IndexOf(Sorting.SortColumns, "rating");
            this.outputSongs = GetSongsFromPathDictionary();
            this.sortDictionaries[index] = Sorting.CreateFieldDictionary(this.outputSongs, "rating");
        }

        public void Add(Song song)
        {
            if (song != null)
                if (!pathDictionary.ContainsKey(song.Path))
                {
                    outputSongs.Add(song);
                    this.pathDictionary.Add(song.Path, song);
                    Sorting.AddSongToSortDictionaries(song, sortDictionaries);
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
            Sorting.RemoveSongFromSortDictionaries(outputSongs[index], sortDictionaries);
            this.pathDictionary.Remove(outputSongs[index].Path);
            outputSongs.Remove(outputSongs[index]);
            OnLibraryChanged(new LibraryChangedEventArgs());
        }

        public void Remove(List<int> indexes)
        {
            raiseLibraryChangedEvent = false;
            foreach (int i in indexes)
                Remove(i);
            raiseLibraryChangedEvent = true;
            OnLibraryChanged(new LibraryChangedEventArgs());
        }

        public void Remove(Song song)
        {
            Remove(song.Path);
        }

        public void Remove(List<Song> songs)
        {
            this.outputSongs.RaiseListChangedEvents = false;
            foreach (Song s in songs)
                Remove(s.Path);
            this.outputSongs.RaiseListChangedEvents = true;
            OnLibraryChanged(new LibraryChangedEventArgs());
        }

        public void Remove(string path)
        {
            Song toBeRemoved = null;
            foreach (Song s in outputSongs)
                if (s.Path == path)
                    toBeRemoved = s;
            Sorting.RemoveSongFromSortDictionaries(toBeRemoved, sortDictionaries);
            outputSongs.Remove(toBeRemoved);
            this.pathDictionary.Remove(toBeRemoved.Path);
            this.OnLibraryChanged(new LibraryChangedEventArgs());
        }

        public void RemoveAll()
        {
            this.outputSongs = new BindingList<Song>();
            this.pathDictionary.Clear();
            Sorting.CreateSortDictionaries(this.outputSongs, this.sortDictionaries);
            this.OnLibraryChanged(new LibraryChangedEventArgs());
        }

        public Song GetNext()
        {
            if (outputSongs.Count == 0)
                return null;
            if (CurrentIndex + 1 < outputSongs.Count)
                return outputSongs[++CurrentIndex];
            else
            {
                CurrentIndex = 0;
                return outputSongs[CurrentIndex];
            }
        }

        public Song GetPrevious()
        {
            if (outputSongs.Count == 0)
                return null;
            if (CurrentIndex > 0)
                return outputSongs[--CurrentIndex];
            else
            {
                CurrentIndex = outputSongs.Count - 1;
                return outputSongs[CurrentIndex];
            }
        }

        public Song GetRandom()
        {
            return pathDictionary.Values.ToList()[Playlist.r.Next(0, pathDictionary.Count)];
        }

        public void Add(List<string> paths)
        {
            for (int i = 0; i < paths.Count; i++)
                paths[i] = paths[i].ToLower();

            this.newSongs = new List<Song>();
            BackgroundWorker addFilesWorker = new BackgroundWorker();
            addFilesWorker.WorkerSupportsCancellation = false;
            addFilesWorker.WorkerReportsProgress = true;
            addFilesWorker.DoWork += addFilesWorker_DoWork;
            addFilesWorker.ProgressChanged += addFilesWorker_ProgressChanged;
            addFilesWorker.RunWorkerCompleted += addFilesWorker_RunWorkerCompleted;
            addFilesWorker.RunWorkerAsync(paths);
            this.outputSongs.RaiseListChangedEvents = false;
            this.invalidSongPaths = new List<string>();
        }

        void addFilesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            List<string> musicFiles = e.Argument as List<string>;
            int count = 0;
            foreach (string fileName in musicFiles)
            {
                if (!Exists(fileName))
                {
                    Song newSong;
                    try
                    {
                        newSong = new Song(fileName);
                    }
                    catch
                    {
                        newSong = null;
                        this.invalidSongPaths.Add(fileName);
                    }

                    if (newSong != null)
                    {
                        this.newSongs.Add(newSong);
                        if (count % 50 == 0)
                            worker.ReportProgress((int)(100 * count / musicFiles.Count));
                    }
                }
                count++;
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
            this.outputSongs.ResetBindings();
            if (raiseLibraryChangedEvent)
                if (LibraryChanged != null)
                    LibraryChanged(this, e);
        }

        void addFilesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<Song> newLibrary = new List<Song>(this.outputSongs.ToList());
            newLibrary.AddRange(newSongs);
            this.outputSongs = new BindingList<Song>(newLibrary);

            foreach (Song s in newSongs)
                if (!pathDictionary.Keys.Contains(s.Path))
                    pathDictionary.Add(s.Path, s);

            this.outputSongs.RaiseListChangedEvents = true;
            Save();

            Sorting.CreateSortDictionaries(this.outputSongs, this.sortDictionaries);

            this.outputSongs.ResetBindings();
            OnLibraryChanged(new LibraryChangedEventArgs());

            if (invalidSongPaths.Count > 0)
            {
                int numSongs = invalidSongPaths.Count + newSongs.Count;
                string errorMessage = "An error occured when adding " + invalidSongPaths.Count + 
                    " out of " + numSongs + " music files found.";
                if (invalidSongPaths.Count < 10)
                {
                    errorMessage += "\nWas unable to add:";
                    foreach (string path in invalidSongPaths)
                        errorMessage += "\n" + path;
                }
                MessageBox.Show(errorMessage);
            }
            invalidSongPaths = null;
        }

        private bool Exists(string path)
        {
            foreach (Song s in outputSongs)
                if (s.Path == path)
                    return true;
            return false;
        }

        public SearchResult Search(string searchString)
        {
            ResetSortVariables();
            searchString = searchString.ToLower().Trim();

            var result = new SearchResult(this);
            result.Name = "Search Results";
            var resultSongs = new List<Song>();

            // 4 first dictionaryies contain title artist album genre fields
            for (int i = 0; i < 4; i++)
            {
                var dictionary = sortDictionaries[i];

                List<string> keysToAdd = Searching.WholeStringKeySearch(searchString, dictionary);

                foreach (string keyToAdd in keysToAdd)
                {
                    List<Song> currentList = dictionary[keyToAdd];
                    foreach (Song s in currentList)
                        if (!resultSongs.Contains(s))
                            result.Add(s);
                }
            }

            return result;
        }

        private Dictionary<string, Song> SongListToPathDictionary()
        {
            return outputSongs.ToDictionary(x => x.Path, x => x);
        }

        public override string ToString()
        {
            return this.Name + ", " + this.NumSongs + " songs.";
        }

        public void Save()
        {
            Stream stream = File.Create(PATH);
            XmlSerializer serializer = new XmlSerializer(typeof(List<Song>));
            serializer.Serialize(stream, pathDictionary.Values.ToList());
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
