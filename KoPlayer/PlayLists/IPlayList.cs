using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Windows.Forms;

namespace KoPlayer.Playlists
{
    public interface IPlaylist
    {
        string Name { get; }
        string Path { get; }
        int NumSongs {get;}
        int CurrentIndex { get; set; }
        int SortColumnIndex { get; set; }
        void Add(string path);
        void Add(List<Song> song);
        void Remove(int indexes);
        void Remove(List<int> indexes);
        void Remove(List<Song> songs);
        void RemoveAll();
        Song GetNext();
        Song GetPrevious();
        Song GetRandom();
        void UpdateSongInfo(Song song);
        void Sort(int columnIndex, string field);
        SortOrder SortOrder { get; }
        BindingList<Song> GetSongs();
        BindingList<Song> GetAllSongs();
        void Save();
    }

    public class PlaylistException : Exception
    {
        public PlaylistException(string message)
            : base(message)
        { }
    }

    public class SongReloadException : Exception
    {
        string message;
        public SongReloadException()
            : base()
        { this.message = "Song load failed: File not found"; }

        public override string ToString()
        {
            return this.message;
        }
    }
}

