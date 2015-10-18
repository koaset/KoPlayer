using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KoPlayer.PlayLists
{
    public class Song
    {
        [System.ComponentModel.DisplayName("Path")]
        public string Path { get; set; }
        [System.ComponentModel.DisplayName("Title")]
        public string Title { get; set; }
        [System.ComponentModel.DisplayName("Artist")]
        public string Artist { get; set; }
        [System.ComponentModel.DisplayName("Album")]
        public string Album { get; set; }
        [System.ComponentModel.DisplayName("Genre")]
        public string Genre { get; set; }
        [System.ComponentModel.DisplayName("#")]
        public int TrackNumber { get; set; }
        [System.ComponentModel.DisplayName("Rating")]
        public int Rating { get; set; }
        [System.ComponentModel.DisplayName("Play Count")]
        public int PlayCount { get; set; }
        [System.ComponentModel.DisplayName("Length")]
        public string Length { get; set; }
        [System.ComponentModel.DisplayName("DateAdded")]
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

        private void Read(string path)
        {
            try
            {
                TagLib.File track = TagLib.File.Create(path);
                Title = track.Tag.Title;
                if (track.Tag.Performers.Length > 0)
                    Artist = track.Tag.Performers[0];
                else
                    Artist = "";
                Album = track.Tag.Album;
                Genre = track.Tag.FirstGenre;
                TrackNumber = (int)track.Tag.Track;
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

        /*public DataGridViewRow GetGridViewItem(DataGridView gridView)
        {
            DataGridViewRow ret = (DataGridViewRow)gridView.Rows[0].Clone();

            ret.Tag = Path;

            foreach (DataGridViewColumn column in gridView.Columns)
            {
                //DataGridViewCell cell = new DataGridViewCell();
                switch (column.HeaderText)
                {
                    case "Title":
                        ret.Cells[0].Value = Title;
                        break;
                    case "Length":
                        ret.Cells[1].Value = Length;
                        break;
                    case "Artist":
                        ret.Cells[2].Value = Artist;
                        break;
                    case "Album":
                        ret.Cells[3].Value = Album;
                        break;
                    case "#":
                        if (TrackNumber == 0)
                            ret.Cells[4].Value = "";
                        else
                            ret.Cells[4].Value = TrackNumber.ToString();
                        break;
                    case "Genre":
                        ret.Cells[5].Value = Genre;
                        break;
                }
            }
            return ret;
        }*/

        private static string DurationFromTimeSpanToString(TimeSpan duration)
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
    }
}
