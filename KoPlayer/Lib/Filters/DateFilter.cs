﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.Lib.Filters
{
    public class DateFilter : Filter
    {
        public int NumUnits { get; private set; }
        public TimeUnit Unit { get; private set; }

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
    }

    public enum TimeUnit
    {
        Day,
        Week,
        Month,
    }
}