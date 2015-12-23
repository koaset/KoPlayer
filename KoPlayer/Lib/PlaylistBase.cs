using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KoPlayer.Lib
{
    public abstract class PlaylistBase
    {
        [System.ComponentModel.DisplayName("Name")]
        public virtual string Name { get; set; }

        [System.ComponentModel.Browsable(false)]
        public abstract string Path { get; }

        protected List<Song> songs;

        [System.ComponentModel.Browsable(false)]
        public int CurrentIndex { get; set; }
        [System.ComponentModel.Browsable(false)]
        public int NumSongs { get { return songs.Count; } }
        [System.ComponentModel.Browsable(false)]
        public SortOrder SortOrder { get { return sortOrder; } }
        [System.ComponentModel.Browsable(false)]
        public int SortColumnIndex { get; set; }

        protected SortOrder sortOrder = SortOrder.None;
        protected string sortField = "";
        protected List<Dictionary<string, List<Song>>> sortDictionaries;

        protected void ResetSortVariables()
        {
            this.sortOrder = System.Windows.Forms.SortOrder.None;
            this.SortColumnIndex = -1;
        }

        public virtual List<Song> GetSongs()
        {
            return songs;
        }

        public virtual Song GetNext()
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

        public virtual Song GetPrevious()
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

        public virtual Song GetRandom(bool weighRating)
        {
            return songs[Playlist.r.Next(0, songs.Count)];
        }

        public abstract void Add(string path);
        public abstract void Add(List<Song> song);
        public abstract void Remove(int indexes);
        public abstract void Remove(List<int> indexes);
        public abstract void Remove(List<Song> songs);
        public abstract void RemoveAll();
        public abstract void UpdateSongInfo(Song song);
        public abstract void Sort(int columnIndex, string field);
        public abstract void Save();
    }

    public class PlaylistException : Exception
    {
        public PlaylistException(string message)
            : base(message)
        { }
    }
}
