using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Lib.Filters
{
    public class RatingFilter : Filter
    {
        public int EdgeRating
        {
            get
            {
                return edgeRating;
            }
            set 
            {
                edgeRating = value;
                SetAllowedRatings();
            }
        }


        public bool AndAbove
        {
            get
            {
                return andAbove;
            }
            set
            {
                andAbove = value;
                SetAllowedRatings();
            }
        }

        public bool Inclusive
        {
            get
            {
                return inclusive;
            }
            set
            {
                inclusive = value;
                SetAllowedRatings();
            }
        }

        private List<int> allowedRatings;
        private int edgeRating;
        private bool andAbove;
        private bool inclusive;

        public void SetParams(int edgeRating, bool andAbove, bool inclusive)
        {
            this.edgeRating = edgeRating;
            this.andAbove = andAbove;
            this.inclusive = inclusive;
            SetAllowedRatings();
        }

        private void SetAllowedRatings()
        {
            allowedRatings.Clear();

            if (andAbove)
                allowedRatings = Above(edgeRating, !inclusive);
            else
                allowedRatings = Below(edgeRating, !inclusive);
        }

        public RatingFilter(int edgeRating, bool andAbove, bool inclusive)
            : base()
        {
            allowedRatings = new List<int>();
            SetParams(edgeRating, andAbove, inclusive);
        }

        public RatingFilter(System.IO.StreamReader sr)
            : this(int.Parse(sr.ReadLine()), bool.Parse(sr.ReadLine()), bool.Parse(sr.ReadLine()))
        { }

        public RatingFilter(RatingFilter filter)
            : this(filter.edgeRating, filter.andAbove, filter.inclusive)
        { }

        protected override bool Allowed(Song song)
        {
            return allowedRatings.Contains(song.Rating);
        }

        protected override void SaveData(System.IO.StreamWriter sw)
        {
            sw.WriteLine(edgeRating);
            sw.WriteLine(andAbove);
            sw.WriteLine(inclusive);
        }

        public override string ToString()
        {
            string ret = "Ratings: ";
            foreach (int r in allowedRatings)
                ret += r + " ";
            return ret;
        }

        private static List<int> Above(int val, bool strict)
        {
            var ret = new List<int>();

            if (!strict)
                ret.Add(val);

            for (int i = val + 1; i <= 5; i++)
                ret.Add(i);

            return ret;
        }

        private static List<int> Below(int val, bool strict)
        {
            var ret = new List<int>();

            if (!strict)
                ret.Add(val);

            for (int i = val - 1; i >= 0; i--)
                ret.Add(i);

            return ret;
        }

        private static List<int> Single(int rating)
        {
            return new int[] { rating }.ToList();
        }
    }
}
