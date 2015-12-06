using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace KoPlayer.Lib
{
    public interface IPlaylist
    {
        event EventHandler Changed;

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
        List<Song> GetSongs();
        void Save();
    }

    public class PlaylistException : Exception
    {
        public PlaylistException(string message)
            : base(message)
        { }
    }
}

