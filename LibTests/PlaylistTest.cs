using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using KoPlayer.Lib;

namespace LibTests
{
    [TestClass]
    public class PlaylistTest
    {
        [TestMethod]
        public void PlaylistCreate()
        {
            var library = Library.Load();
            Playlist pl;
            try
            {
                pl = new Playlist(library, "TestPlaylist 1");
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void PlaylistSaveSingle()
        {
            var library = Library.Load();
            Playlist pl = new Playlist(library, "TestPlaylist 1");
            pl.Add(library.GetSongs());

            try
            {
                pl.Save();
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void PlaylistSaveMany()
        {
            var library = Library.Load();
            Playlist pl = new Playlist(library, "TestPlaylist 2");

            for (int i = 0; i < 10; i++)
                pl.Add(library.GetSongs());

            try
            {
                pl.Save();
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}
