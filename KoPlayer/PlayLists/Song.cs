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
        [System.ComponentModel.DisplayName("Length")]
        public string Length { get; set; }
        [System.ComponentModel.DisplayName("Date Added")]
        public DateTime DateAdded { get; set; }
        [System.ComponentModel.DisplayName("Last Played")]
        public DateTime LastPlayed { get; set; }

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
            Length = "0:0";
            DateAdded = DateTime.Now;
        }

        public Song(string path) : this()
        {
            this.Path = path;
            this.Read(path); ;
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
                        return Length;
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

            Length = DurationFromTimeSpanToString(track.Properties.Duration);
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

        private void Read(string path)
        {
            try
            {
                TagLib.File track = TagLib.File.Create(path);

                Title = track.Tag.Title;
                if (Title == null)
                    Title = "";

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
                Length = DurationFromTimeSpanToString(track.Properties.Duration);
                DateAdded = DateTime.Now;
            }
            catch (Exception e)
            {
                MessageBox.Show("Song read exception: " + e.ToString());
            }
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
                track.Save();
            }
        }

        public static string DurationFromTimeSpanToString(TimeSpan duration)
        {
            string ret = "";
            if (duration.Hours > 0)
                ret += duration.Hours + ":";
            ret += duration.Minutes + ":";
            if (duration.Seconds < 10)
                ret += "0";
            ret += duration.Seconds;
            return ret;
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

        public static System.Drawing.Image GetImage(Song song)
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
}
