using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;

namespace KoPlayer.Playlists
{
    public class Song : IComparable
    {
        public const string LengthFormat = "%m\\:ss";

        #region Properties
        [System.ComponentModel.Browsable(false)]
        public string Path { get; set; }

        [System.ComponentModel.DisplayName("Title")]
        public string Title { get; set; }

        [System.ComponentModel.DisplayName("Artist")]
        public string Artist { get; set; }

        [System.ComponentModel.DisplayName("Album")]
        public string Album { get; set; }

        [System.ComponentModel.DisplayName("Genre")]
        public string Genre { get; set; }

        [System.ComponentModel.Browsable(false)]
        public int TrackNumber { get; set; }

        [System.ComponentModel.Browsable(false)]
        public int DiscNumber { get; set; }

        [System.ComponentModel.DisplayName("Rating")]
        [XmlIgnore]
        public string RatingString { get { return RatingIntToString(Rating); } }

        [System.ComponentModel.Browsable(false)]
        public int Rating { get; set; }

        [System.ComponentModel.DisplayName("Play Count")]
        public int PlayCount { get; set; }

        [XmlIgnore]
        [System.ComponentModel.DisplayName("Length")]
        public TimeSpan Length { get; set; }

        [System.ComponentModel.Browsable(false)]
        public int LengthMS {
            get { return (int)this.Length.TotalMilliseconds; }
            set { this.Length = new TimeSpan(0, 0, 0, 0, value); }
        }

        [XmlIgnore]
        [System.ComponentModel.Browsable(false)]
        public string LengthString { get { return this.Length.ToString(LengthFormat); } }

        [System.ComponentModel.DisplayName("Date Added")]
        public DateTime DateAdded { get; set; }

        [System.ComponentModel.DisplayName("Last Played")]
        public DateTime LastPlayed { get; set; }
        #endregion

        public Song()
        {
            Path = "empty";
            Title = "";
            Artist = "";
            Album = "";
            Genre = "";
            TrackNumber = -1;
            DiscNumber = -1;
            Rating = -1;
            PlayCount = -1;
            Length = TimeSpan.MinValue;
            DateAdded = DateTime.Now;
        }

        public Song(string path) : this()
        {
            this.Path = path;
            bool readResult = Read(path);
            if (readResult == false)
                throw new SongReadException();
        }

        /// <summary>
        /// Returns a string field
        /// </summary>
        /// <param name="name">Name of field</param>
        /// <returns></returns>
        public string this[string name]
        {
            get
            {
                switch (name.ToLower())
                {
                    case "title":
                        return Title;
                    case "artist":
                        return Artist;
                    case "album":
                        return Album;
                    case "genre":
                        return Genre;
                    case "rating":
                        return RatingString;
                    case "length":
                        return LengthString;
                    case "play count":
                        return PlayCount.ToString();
                    case "date added":
                        return DateAdded.ToShortTimeString();
                    case "last played":
                        return LastPlayed.ToShortDateString();
                    case "track number":
                        return TrackNumber.ToString();
                    case "disc number":
                        return DiscNumber.ToString();
                    default:
                        return null;
                }
            }
        }

        private bool Read(string path)
        {
            try
            {
                TagLib.File track = TagLib.File.Create(path);

                Title = track.Tag.Title;
                if (Title == null)
                    Title = "";
                if (Title == "")
                    Title = System.IO.Path.GetFileNameWithoutExtension(this.Path);

                if (Artist != null && track.Tag.Performers.Length > 0)
                    Artist = track.Tag.Performers[0];
                else
                    Artist = "";

                Album = track.Tag.Album;
                if (Album == null)
                    Album = "";

                Genre = track.Tag.FirstGenre;
                if (Genre == null)
                    Genre = "";

                TrackNumber = (int)track.Tag.Track;
                DiscNumber = (int)track.Tag.Disc;
                Rating = 0;
                PlayCount = 0;
                Length = track.Properties.Duration;
                DateAdded = DateTime.Now;
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void SaveTags()
        {
            using (TagLib.File track = TagLib.File.Create(Path))
            {
                if (this.Title != null)
                    track.Tag.Title = this.Title;
                if (this.Artist != null)
                {
                    if (track.Tag.Performers.Length == 0)
                        track.Tag.Performers = new string[1] { this.Artist };
                    else
                        track.Tag.Performers[0] = this.Artist;
                    if (this.Artist == "")
                        track.Tag.Performers = new string[0];
                }
                if (this.Album != null)
                    track.Tag.Album = this.Album;
                if (this.Genre != null)
                {
                    if (track.Tag.Genres.Length == 0)
                        track.Tag.Genres = new string[1] { this.Genre };
                    else
                        track.Tag.Genres[0] = this.Genre;
                    if (this.Genre == "")
                        track.Tag.Genres = new string[0];
                }
                if (this.TrackNumber > 0)
                    track.Tag.Track = (uint)this.TrackNumber;
                if (this.DiscNumber > 0)
                    track.Tag.Disc = (uint)this.DiscNumber;
                try
                {
                    track.Save();
                }
                catch
                {
                    MessageBox.Show("Could not save tags: File being used by another process.");
                }
            }
        }

        public void Reload()
        {
            TagLib.File track = null;
            try
            {
                track = TagLib.File.Create(this.Path);
            }
            catch
            {
                throw new SongReloadException();
            }

            Title = track.Tag.Title;
            if (Title == null)
                Title = "";
            if (Title == "")
                Title = System.IO.Path.GetFileNameWithoutExtension(this.Path);
            

            if (Artist != null && track.Tag.Performers.Length > 0)
                Artist = track.Tag.Performers[0];
            else
                Artist = "";

            Album = track.Tag.Album;
            if (Album == null)
                Album = "";

            Genre = track.Tag.FirstGenre;
            if (Genre == null)
                Genre = "";

            TrackNumber = (int)track.Tag.Track;
            DiscNumber = (int)track.Tag.Disc;

            Length = track.Properties.Duration;

            track = null;
        }

        public System.Drawing.Image ReloadAndGetImage()
        {
            TagLib.File track = null;
            try
            {
                track = TagLib.File.Create(this.Path);
            }
            catch
            {
                throw new SongReloadException();
            }

            Title = track.Tag.Title;
            if (Title == null)
                Title = "";
            if (Title == "")
                Title = System.IO.Path.GetFileNameWithoutExtension(this.Path);


            if (Artist != null && track.Tag.Performers.Length > 0)
                Artist = track.Tag.Performers[0];
            else
                Artist = "";

            Album = track.Tag.Album;
            if (Album == null)
                Album = "";

            Genre = track.Tag.FirstGenre;
            if (Genre == null)
                Genre = "";

            TrackNumber = (int)track.Tag.Track;
            DiscNumber = (int)track.Tag.Disc;

            Length = track.Properties.Duration;

            if (track.Tag.Pictures.Length > 0)
            {
                MemoryStream ms = new MemoryStream(track.Tag.Pictures[0].Data.Data);
                return System.Drawing.Image.FromStream(ms);
            }
            return null;

        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            Song otherSong = obj as Song;
            if (otherSong != null)
            {
                if (this.Album.CompareTo(otherSong.Album) == 0)
                {
                    if (this.DiscNumber.CompareTo(otherSong.DiscNumber) != 0)
                        return this.DiscNumber.CompareTo(otherSong.DiscNumber);
                    else
                        return this.TrackNumber.CompareTo(otherSong.TrackNumber);
                }
                return this.Album.CompareTo(otherSong.Album);
            }
            else
                throw new ArgumentException("Object is not a Song");
        }

        public static int RatingStringToInt(string rating)
        {
            return rating.Length;
        }

        public static string RatingIntToString(int rating)
        {
            string ret = "";
            for (int i = 0; i < 5; i++)
                if (i < rating)
                    ret += "★";
            return ret;
        }

        private static System.Drawing.Image GetImage(Song song)
        {
            TagLib.File tagFile = TagLib.File.Create(song.Path);
            if (tagFile.Tag.Pictures.Length > 0)
            {
                MemoryStream ms = new MemoryStream(tagFile.Tag.Pictures[0].Data.Data);
                return System.Drawing.Image.FromStream(ms);
            }
            return null;
        }
    }

    public class SongReadException : Exception
    {
        public new string Message { get; set; }

        public SongReadException()
        {
            this.Message = "Could not read song tags";
        }
    }
}
