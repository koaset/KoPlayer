using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KoPlayer.Lib
{
    public class PlaylistFactory
    {
        public static Playlist MakePlaylist(string path, Library library, Settings settings)
        {
            try
            {
                using (var sr = new StreamReader(path))
                {
                    string type = sr.ReadLine();

                    switch (type)
                    {
                        case "KoPlayer.Lib.Playlist":
                            return new Playlist(sr, library);
                        case "KoPlayer.Lib.ShuffleQueue":
                            return new ShuffleQueue(sr, library, settings);
                        case "KoPlayer.Lib.RatingFilterPlaylist":
                            return new RatingFilterPlaylist(sr, library);
                        default:
                            return null;

                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
