using System;
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

            var filter = new RatingFilter(3, true, false);
            fpl.Filters.Add(filter);
            fpl.FilterLibrary();
            Assert.AreEqual(1, fpl.NumSongs);

            filter.SetParams(3, true, true);
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);

            filter.SetParams(3, false, true);
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);

            filter.SetParams(3, false, false);
            fpl.FilterLibrary();
            Assert.AreEqual(1, fpl.NumSongs);
        }

        [TestMethod]
        public void FilterPlaylistApplyCombinationFilter()
        {
            var lib = CreateLibrary();

            var fpl = new FilterPlaylist(lib, "fpl 1");

            fpl.Filters.Add(new StringFilter("genre", "noise", true));
            fpl.Filters.Add(new RatingFilter(3, false, true));
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

        [TestMethod]
        public void FilterPlaylistSave()
        {
            var lib = CreateLibrary();

            var fpl = new FilterPlaylist(lib, "fpl 1");

            fpl.Filters.Add(new StringFilter("genre", "NoIsE", true));
            fpl.Filters.Add(new DateFilter(TimeUnit.Day, 10));
            fpl.Filters.Add(new RatingFilter(4, true, true));
            fpl.FilterLibrary();

            try
            {
                fpl.Save();
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void FilterPlaylistLoad()
        {
            var lib = CreateLibrary();
            var set = new KoPlayer.Settings();
            var loaded = PlaylistFactory.MakePlaylist(@"Playlists/fpl 3.pl", lib, set) as FilterPlaylist;

            Assert.AreEqual(0, loaded.NumSongs);

            loaded.Filters.RemoveAt(2);

            loaded.FilterLibrary();
            Assert.AreEqual(2, loaded.NumSongs);
        }

        [TestMethod]
        public void FilterPlaylistLibraryChange()
        {
            var lib = new Library();

            var fpl = new FilterPlaylist(lib, "fpl 4");

            fpl.Filters.Add(new StringFilter("genre", "noise", true));

            fpl.FilterLibrary();

            Assert.AreEqual(0, fpl.NumSongs);

            AddSongsToLibrary(lib);
            lib.Changed += (s, e) => { };
            fpl.FilterLibrary();
            Assert.AreEqual(2, fpl.NumSongs);
        }

        public static Library CreateLibrary()
        {
            Library lib = new Library();

            AddSongsToLibrary(lib);

            return lib;
        }

        private static void AddSongsToLibrary(Library lib)
        {
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
        }
    }
}
