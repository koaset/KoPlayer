﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KoPlayer.Lib;
using KoPlayer.Lib.Filters;

namespace LibTests
{
    [TestClass]
    public class FilterPlaylistTest
    {
        [TestMethod]
        public void FilterPlaylistCreate()
        {
            var lib = CreateLibrary();

            FilterPlaylist fpl;

            try
            {
                fpl = new FilterPlaylist(lib, "fpl 1");
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void FilterPlaylistAddFilter()
        {
            var lib = CreateLibrary();

            var fpl = new FilterPlaylist(lib, "fpl 1");

            var filter = new StringFilter("title", "1", true);

            try
            {
                fpl.Filters.Add(filter);
            }
            catch
            {
                Assert.Fail();
            }

            Assert.AreEqual(1, fpl.Filters.Count);
        }

        [TestMethod]
        public void FilterPlaylistApplyTitleFilter()
        {
            var lib = CreateLibrary();

            var fpl = new FilterPlaylist(lib, "fpl 1");

            fpl.Filters.Add(new StringFilter("title", "2", true));

            fpl.FilterLibrary();
            
            Assert.AreEqual(1, fpl.NumSongs);

            fpl.Filters.Add(new StringFilter("title", "2", false));

            fpl.FilterLibrary();

            Assert.AreEqual(0, fpl.NumSongs);
        }

        [TestMethod]
        public void FilterPlaylistApplyGenreFilter()
        {
            var lib = CreateLibrary();

            var fpl = new FilterPlaylist(lib, "fpl 1");

            var filter = new StringFilter("genre", "NoIsE", true);
            fpl.Filters.Add(filter);
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);

            filter.Contains = false;
            fpl.FilterLibrary();
            Assert.AreEqual(1, fpl.NumSongs);
        }

        [TestMethod]
        public void FilterPlaylistApplyRatingFilter()
        {
            var lib = CreateLibrary();

            var fpl = new FilterPlaylist(lib, "fpl 1");

            var filter = new RatingFilter(RatingFilter.Above(3, true));
            fpl.Filters.Add(filter);
            fpl.FilterLibrary();
            Assert.AreEqual(1, fpl.NumSongs);

            filter.AllowedRatings = RatingFilter.Above(3, false);
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);

            filter.AllowedRatings = RatingFilter.Below(3, false);
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);

            filter.AllowedRatings = RatingFilter.Below(3, true);
            fpl.FilterLibrary();
            Assert.AreEqual(1, fpl.NumSongs);
        }

        [TestMethod]
        public void FilterPlaylistApplyCombinationFilter()
        {
            var lib = CreateLibrary();

            var fpl = new FilterPlaylist(lib, "fpl 1");

            fpl.Filters.Add(new StringFilter("genre", "noise", true));
            fpl.Filters.Add(new RatingFilter(RatingFilter.Below(3, false)));
            fpl.FilterLibrary();

            Assert.AreEqual(2, fpl.NumSongs);
        }

        [TestMethod]
        public void FilterPlaylistApplyDateFilter()
        {
            var lib = CreateLibrary();

            var fpl = new FilterPlaylist(lib, "fpl 1");

            var filter = new DateFilter(TimeUnit.Day, 2);
            fpl.Filters.Add(filter);
            
            fpl.FilterLibrary();
            Assert.AreEqual(1, fpl.NumSongs);

            filter.SetLimit(TimeUnit.Day, 7);
            fpl.FilterLibrary();
            Assert.AreEqual(1, fpl.NumSongs);

            filter.SetLimit(TimeUnit.Day, 13);
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);

            filter.SetLimit(TimeUnit.Week, 2);
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);

            filter.SetLimit(TimeUnit.Month, 2);
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);

            filter.SetLimit(TimeUnit.Month, 4);
            fpl.FilterLibrary();
            Assert.AreEqual(3, fpl.NumSongs);

            filter.SetLimit(TimeUnit.Month, 34);
            fpl.FilterLibrary();
            Assert.AreEqual(3, fpl.NumSongs);
        }

        private Library CreateLibrary()
        {
            Library lib = new Library();

            var song = new Song(@"Songs\empty10sec.mp3");
            song.Rating = 3;
            song.DateAdded = DateTime.Now.AddDays(-1);
            lib.Add(song);

            song = new Song(@"Songs\empty10sec2.mp3");
            song.DateAdded = DateTime.Now.AddDays(-8);
            lib.Add(song);

            song = new Song(@"Songs\empty10sec3.mp3");
            song.DateAdded = DateTime.Now.AddMonths(-3);
            song.Rating = 5;
            lib.Add(song);

            return lib;
        }
    }
}