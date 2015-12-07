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
    }
}
