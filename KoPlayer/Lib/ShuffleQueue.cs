using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace KoPlayer.Lib
{
    public class ShuffleQueue : Playlist
    {
        [XmlIgnore]
        public IPlaylist Source { get; set; }
        [XmlIgnore]
        public Settings Settings { get; set; }
        private Library library;

        public ShuffleQueue()
            : base()
        { }

        public ShuffleQueue(Library library, Settings settings, List<string> songPaths) : base (library, "Shuffle Queue", songPaths)
        {
            Settings = settings;
            this.library = library;
        }

        public ShuffleQueue(Library library, Settings settings)
            : base(library, "Shuffle Queue")
        {
            Settings = settings;
            this.library = library;
        }

        public void Populate()
        {
            if (Source == null)
            {
                Source = library;
                Settings.Shufflequeue_SourcePlaylistName = library.Name;
            }

            while (CurrentIndex > Settings.Shufflequeue_NumPrevious)
                Remove(0);

            if (Source.NumSongs > 0)
                while (NumSongs - CurrentIndex < Settings.Shufflequeue_NumNext + 1)
                    Add(Source.GetRandom());
        }

        public override void Save()
        {
            try
            {
                using (var stream = File.Create(Path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ShuffleQueue));
                    serializer.Serialize(stream, this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save play list exception: " + ex.ToString());
            }
        }

        /// <summary>
        /// Load shufflequeue from file. Returns null if it fails
        /// </summary>
        /// <param name="path"></param>
        /// <param name="library"></param>
        /// <returns></returns>
        public static ShuffleQueue Load(String path, Library library, Settings settings)
        {
            Stream stream = null;
            Playlist loadedPlaylist = null;
            try
            {
                stream = File.OpenRead(path);
                XmlSerializer serializer = new XmlSerializer(typeof(ShuffleQueue));
                loadedPlaylist = (ShuffleQueue)serializer.Deserialize(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
            ShuffleQueue pl = new ShuffleQueue(library, settings, loadedPlaylist.songPaths);
            library.Changed += pl.library_LibraryChanged;
            pl.CurrentIndex = loadedPlaylist.CurrentIndex;

            List<string> toBeRemoved = new List<string>();
            foreach (string filePath in pl.songPaths)
                if (!library.PathDictionary.Keys.Contains(filePath))
                    toBeRemoved.Add(filePath);
            foreach (string filePath in toBeRemoved)
                pl.Remove(filePath);

            return pl;
        }
    }
}
