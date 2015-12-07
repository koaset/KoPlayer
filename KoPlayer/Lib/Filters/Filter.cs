using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Lib.Filters
{
    public abstract class Filter
    {
        public void ApplyTo(List<Song> songs)
        {
            songs.RemoveAll(s => !Allowed(s));
        }

        protected abstract bool Allowed(Song song);
    }
}
