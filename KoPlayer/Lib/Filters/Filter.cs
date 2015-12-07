using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KoPlayer.Lib.Filters
{
    public abstract class Filter
    {
        public void ApplyTo(List<Song> songs)
        {
            songs.RemoveAll(s => !Allowed(s));
        }

        public void Save(StreamWriter sw)
        {
            sw.WriteLine(GetType());
            SaveData(sw);
        }

        protected abstract void SaveData(StreamWriter sw);
        protected abstract bool Allowed(Song song);
    }
}
