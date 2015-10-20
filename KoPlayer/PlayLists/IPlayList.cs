using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Windows.Forms;

namespace KoPlayer.PlayLists
{
    public interface IPlayList
    {
        string Name { get; }
        string Path { get; }
        int NumSongs {get;}
        int CurrentIndex { get; set; }
        void Add(string path);
        void Add(Song song);
        void Add(List<Song> song);
        void Remove(int index);
        void Remove(string path);
        void Remove(Song song);
        Song GetNext();
        Song GetPrevious();
        Song GetRandom();
        void AddFolder(string path);
        void RemoveFolder(string path);
        void Sort(string field);
        SortOrder SortOrder { get; }
        BindingList<Song> GetAll();
        void Save();
    }

    public class PlayListException : Exception
    {
        public PlayListException(string message)
            : base(message)
        { }
    }
}

