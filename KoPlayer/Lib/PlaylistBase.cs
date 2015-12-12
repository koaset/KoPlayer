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
        public virtual string Name { get; set; }

        protected List<Song> songs;

        public int CurrentIndex { get; set; }
        public int NumSongs { get { return songs.Count; } }

        public SortOrder SortOrder { get { return sortOrder; } }
        public int SortColumnIndex { get; set; }

        protected SortOrder sortOrder = SortOrder.None;
        protected string sortField = "";

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

        public abstract string Path { get; }
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
