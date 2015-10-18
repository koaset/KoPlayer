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
        public Dictionary<string, Song> Dictionary { get { return libraryDictionary; } }

        private const string PATH = "Library.xml";
        private const string EXTENSION = ".mp3";

        private BindingList<Song> songs;
        private Dictionary<string, Song> libraryDictionary;

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
            this.libraryDictionary = SongListToDictionary();
            this.CurrentIndex = 0;
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
                if (!libraryDictionary.ContainsKey(song.Path))
                {
                    songs.Add(song);
                    this.libraryDictionary.Add(song.Path, song);
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
            this.libraryDictionary.Remove(songs[index].Path);
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
            songs.Remove(toBeRemoved);
            this.libraryDictionary.Remove(toBeRemoved.Path);
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
                        this.libraryDictionary.Add(fileName, newSong);
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
            songs.ResetBindings();
            Save();
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
