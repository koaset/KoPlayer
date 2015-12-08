using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Lib.Filters
{
    public class DateFilter : Filter
    {
        public TimeUnit Unit { get; private set; }
        public int NumUnits { get; private set; }

        private DateTime limit;

        public void SetLimit(TimeUnit unit, int numUnits)
        {
            Unit = unit;
            NumUnits = numUnits;
            CalculateLimit();
        }

        public DateFilter(TimeUnit unit, int numUnits)
            : base()
        {
            SetLimit(unit, numUnits);
        }

        public DateFilter(DateFilter filter)
            : this(filter.Unit, filter.NumUnits)
        { }

        private void CalculateLimit()
        {
            switch (Unit)
            {
                case TimeUnit.Day:
                    limit = DateTime.Now.AddDays(-NumUnits);
                    break;
                case TimeUnit.Week:
                    limit = DateTime.Now.AddDays(-NumUnits * 7);
                    break;
                case TimeUnit.Month:
                    limit = DateTime.Now.AddMonths(-NumUnits);
                    break;
                default:
                    limit = DateTime.Now;
                    break;
            }
        }

        protected override bool Allowed(Song song)
        {
            return limit <= song.DateAdded;
        }

        protected override void SaveData(System.IO.StreamWriter sw)
        {
            sw.WriteLine((int)Unit);
            sw.WriteLine(NumUnits);
        }

        public DateFilter(System.IO.StreamReader sr)
            : this((TimeUnit)int.Parse(sr.ReadLine()), int.Parse(sr.ReadLine()))
        { }

        public override string ToString()
        {
            string unit = "";

            if (Unit == TimeUnit.Day)
                unit = " days";
            else if (Unit == TimeUnit.Week)
                unit = " weeks";
            else if (Unit == TimeUnit.Month)
                unit = " months";

            return "Date added is in the last " + NumUnits + unit;
        }
    }

    public enum TimeUnit
    {
        Day,
        Week,
        Month,
    }
}
