using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace KoPlayer.Lib
{
    public class ShuffleQueue : Playlist
    {
        [XmlIgnore]
        public PlaylistBase Source { get; set; }
        [XmlIgnore]
        public Settings Settings { get; set; }
        private Library library;

        public ShuffleQueue()
            : base()
        { }

        public ShuffleQueue(Library library, Settings settings, List<Song> songs) : base (library, "Shuffle Queue", songs)
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

            if (Source.NumSongs <= 0)
                return;

            while (NumSongs - CurrentIndex < Settings.Shufflequeue_NumNext + 1)
                Add(Source.GetRandom(Settings.Shufflequeue_WeighRating));
        }

        public override void Insert(int index, List<Song> songs)
        {
            if (index < CurrentIndex)
                CurrentIndex += songs.Count;

            base.Insert(index, songs);
        }

        public ShuffleQueue(StreamReader sr, Library library, Settings settings)
            : base(sr, library)
        {
            this.library = library;
            this.Settings = settings;
        }
    }
}
