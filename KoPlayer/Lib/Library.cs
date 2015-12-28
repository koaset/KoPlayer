using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KoPlayer.Lib
{
    public delegate void ReportProgressEventHandler(object sender, ReportProgressEventArgs e);

    public class Library : PlaylistBase
    {
        public event ReportProgressEventHandler ReportProgress;
        public event EventHandler Changed;

        public override string Name { get { return "Library"; } }
        public override string Path { get { return PATH; } }
        public Dictionary<string, Song> PathDictionary { get { return pathDictionary; } }
        public List<Dictionary<string, List<Song>>> SortDictionaries { get { return base.sortDictionaries; } }
        
        public static string PATH = KoPlayer.Forms.MainForm.ApplicationPath + "\\Library.xml";
        public static string[] EXTENSIONS = {".mp3", ".m4a", ".wma", ".aac", ".flac"};

        private Dictionary<string, Song> pathDictionary;
        private List<Song> newSongs;
        private List<string> invalidSongPaths;

        private bool raiseLibraryChangedEvent = true;

        public Dictionary<string, List<Song>> GetSortDictionary(string field)
        {
            field = field.ToLower();
            if (Sorting.SortColumns.Contains(field))
                return sortDictionaries[Array.IndexOf(Sorting.SortColumns, field)];
            else
                throw new PlaylistException("Sortdictionary field does not exist");
        }

        public Library()
            : this(null) {}

        public Library(List<Song> songs)
        {
            if (songs != null)
                this.songs = songs;
            else
                this.songs = new List<Song>();

            this.pathDictionary = SongListToPathDictionary();
            this.CurrentIndex = 0;
            ResetSortVariables();

            sortDictionaries = new List<Dictionary<string, List<Song>>>();
            Sorting.CreateSortDictionaries(this.songs, this.sortDictionaries);
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

        public override void UpdateSongInfo(Song song)
        {
            //Remove from all dictionaries
            foreach (Dictionary<string, List<Song>> dictionary in this.sortDictionaries)
                foreach (List<Song> list in dictionary.Values)
                    if (list.Contains(song))
                        list.Remove(song);

            Sorting.AddSongToSortDictionaries(song, this.sortDictionaries);
        }

        private List<Song> GetSongsFromPathDictionary()
        {
            return pathDictionary.Values.ToList();
        }

        public override void Add(string path)
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
            Sorting.CreateSortDictionaries(this.songs, this.sortDictionaries);
        }

        public void ResetRatingDictionary()
        {
            int index = Array.IndexOf(Sorting.SortColumns, "rating");
            this.songs = GetSongsFromPathDictionary();
            this.sortDictionaries[index] = Sorting.CreateFieldDictionary(this.songs, "rating");
        }

        public void Add(Song song)
        {
            if (song != null)
                if (!pathDictionary.ContainsKey(song.Path))
                {
                    songs.Add(song);
                    this.pathDictionary.Add(song.Path, song);
                    Sorting.AddSongToSortDictionaries(song, sortDictionaries);
                    NotifyChanged();
                }
        }

        public override void Add(List<Song> songs)
        {
            foreach (Song s in songs)
                Add(s);
        }

        public override void Remove(int index)
        {
            Sorting.RemoveSongFromSortDictionaries(songs[index], sortDictionaries);
            this.pathDictionary.Remove(songs[index].Path);
            songs.Remove(songs[index]);
            NotifyChanged();
        }

        public override void Remove(List<int> indexes)
        {
            raiseLibraryChangedEvent = false;
            foreach (int i in indexes)
                Remove(i);
            raiseLibraryChangedEvent = true;
            NotifyChanged();
        }

        public void Remove(Song song)
        {
            Remove(song.Path);
        }

        public override void Remove(List<Song> songs)
        {
            foreach (Song s in songs)
                Remove(s.Path);
            NotifyChanged();
        }

        public void Remove(string path)
        {
            Song toBeRemoved = null;
            foreach (Song s in songs)
                if (s.Path == path)
                    toBeRemoved = s;
            Sorting.RemoveSongFromSortDictionaries(toBeRemoved, sortDictionaries);
            songs.Remove(toBeRemoved);
            this.pathDictionary.Remove(toBeRemoved.Path);
            this.NotifyChanged();
        }

        public override void RemoveAll()
        {
            this.songs = new List<Song>();
            this.pathDictionary.Clear();
            Sorting.CreateSortDictionaries(this.songs, this.sortDictionaries);
            this.NotifyChanged();
        }

        public void Add(List<string> paths)
        {
            for (int i = 0; i < paths.Count; i++)
                paths[i] = paths[i].ToLower();

            this.newSongs = new List<Song>();
            var addFilesWorker = new BackgroundWorker();
            addFilesWorker.WorkerSupportsCancellation = false;
            addFilesWorker.WorkerReportsProgress = true;
            addFilesWorker.DoWork += addFilesWorker_DoWork;
            addFilesWorker.ProgressChanged += addFilesWorker_ProgressChanged;
            addFilesWorker.RunWorkerCompleted += addFilesWorker_RunWorkerCompleted;
            addFilesWorker.RunWorkerAsync(paths);
            this.invalidSongPaths = new List<string>();
        }

        private void addFilesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
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

        private void addFilesWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnReportProgress(new ReportProgressEventArgs(e.ProgressPercentage));
        }

        protected virtual void OnReportProgress(ReportProgressEventArgs e)
        {
            if (ReportProgress != null)
                ReportProgress(this, e);
        }

        protected virtual void NotifyChanged()
        {
            if (raiseLibraryChangedEvent)
                if (Changed != null)
                    Changed(this, new EventArgs());
        }

        private void addFilesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<Song> newLibrary = new List<Song>(this.songs.ToList());
            newLibrary.AddRange(newSongs);
            this.songs = new List<Song>(newLibrary);

            foreach (Song s in newSongs)
                if (!pathDictionary.Keys.Contains(s.Path))
                    pathDictionary.Add(s.Path, s);

            Save();

            Sorting.CreateSortDictionaries(this.songs, this.sortDictionaries);

            NotifyChanged();

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
            return songs.Any(s => s.Path == path);
        }

        public SearchResult Search(string searchString)
        {
            var words = searchString.ToLower().Split(' ').ToList();

            foreach (string s in words)
                s.Trim();

            var result = new SearchResult(this);
            var resultSongs = result.GetSongs();

            // 4 first dictionaries contain title, artist, album and genre fields
            for (int i = 0; i < 4; i++)
            {
                var dictionary = sortDictionaries[i];

                var keysToAdd = AndSearch(dictionary, words);

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

        private List<string> AndSearch(Dictionary<string, List<Song>> dictionary, List<string> words)
        {
            var keysToAdd = dictionary.Keys.ToList();

            foreach (string word in words)
            {
                if (string.IsNullOrEmpty(word))
                    continue;

                keysToAdd.RemoveAll(k => !k.ToLower().Contains(word));
            }

            return keysToAdd;
        }

        private Dictionary<string, Song> SongListToPathDictionary()
        {
            return songs.ToDictionary(x => x.Path, x => x);
        }

        public override string ToString()
        {
            return this.Name + ", " + this.NumSongs + " songs.";
        }

        public override void Save()
        {
            using (var stream = File.Create(PATH))
            {
                var serializer = new XmlSerializer(typeof(List<Song>));
                serializer.Serialize(stream, pathDictionary.Values.ToList());
            }
        }

        /// <summary>
        /// Load playlist from file. Returns null if it fails:
        /// </summary>
        /// <param name="path"></param>
        /// <param name="library"></param>
        /// <returns></returns>
        public static Library Load()
        {
            List<Song> loadedLibrary = null;

            try
            {
                using (var stream = File.OpenRead(PATH))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Song>));
                    loadedLibrary = (List<Song>)serializer.Deserialize(stream);
                }
            }
            catch
            {
                return null;
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
}
