using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KoPlayer.Lib;
using System.Collections.Generic;

namespace LibTests
{
    [TestClass]
    public class LibraryTest
    {
        [TestMethod]
        public void LibraryCreate()
        {
            int numSongsExpected = 0;
            string nameExpected = "Library";
            
            Library lib = new Library();

            Assert.AreEqual(lib.NumSongs, numSongsExpected);
            Assert.AreEqual(lib.Name, nameExpected);
        }

        [TestMethod]
        public void LibrarySave()
        {
            Library lib = new Library();
            lib.Add(new Song(@"Songs\empty10sec.mp3"));

            try
            {
                lib.Save();
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void LibraryLoad()
        {
            int numSongsExpected = 1;

            Library lib = null;

            try
            {
                lib = Library.Load();
            }
            catch
            {
                Assert.Fail();
            }

            var songList = lib.GetSongs();

            Assert.AreEqual(numSongsExpected, lib.NumSongs, numSongsExpected);
        }
    }
}
