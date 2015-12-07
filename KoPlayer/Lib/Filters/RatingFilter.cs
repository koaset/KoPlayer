using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Lib.Filters
{
    public class RatingFilter : Filter
    {
        public static List<int> Above(int val, bool strict)
        {
            var ret = new List<int>();

            if (!strict)
                ret.Add(val);

            for (int i = val + 1; i <= 5; i++)
                ret.Add(i);

            return ret;
        }

        public static List<int> Below(int val, bool strict)
        {
            var ret = new List<int>();

            if (!strict)
                ret.Add(val);

            for (int i = val - 1; i >= 0; i--)
                ret.Add(i);

            return ret;
        }

        public static List<int> Single(int rating)
        {
            return new int[] { rating }.ToList();
        }

        public List<int> AllowedRatings { get; set; }

        public RatingFilter(List<int> allowedRatings)
            : base()
        {
            AllowedRatings = allowedRatings ?? new List<int>();
        }

        protected override bool Allowed(Song song)
        {
            return AllowedRatings.Contains(song.Rating);
        }

        protected override void SaveData(System.IO.StreamWriter sw)
        {
            for (int i = 0; i <= 5; i++)
            {
                if (AllowedRatings.Contains(i))
                    sw.Write("1");
                else
                    sw.Write("0");
            }
            sw.Write("\n");
        }

        public RatingFilter(System.IO.StreamReader sr)
            : this(ReadData(sr))
        { }

        private static List<int> ReadData(System.IO.StreamReader sr)
        {
            string data = sr.ReadLine();
            var ret = new List<int>();

            for (int i = 0; i <= 5; i++)
                if (data[i] == '1')
                    ret.Add(i);

            return ret;
        }
    }
}
