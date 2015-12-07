using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KoPlayer.Lib;

namespace LibTests
{
    [TestClass]
    public class SongTest
    {
        [TestMethod]
        public void SongLoad()
        {
            try
            {
                Song s = new Song(@"Songs\empty10sec.mp3");
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void SongTag()
        {
            string titleExpected = "empty10sec - 1";
            string artistExpected = "artist";
            string albumExpected = "album";
            string genreExpected = "noise";
            int trackNumberExpected = 1;
            int discNumberExpected = 1;

            Song s = new Song(@"Songs\empty10sec.mp3");

            Assert.AreEqual(s.Title, titleExpected);
            Assert.AreEqual(s.Artist, artistExpected);
            Assert.AreEqual(s.Album, albumExpected);
            Assert.AreEqual(s.Genre, genreExpected);
            Assert.AreEqual(s.TrackNumber, trackNumberExpected);
            Assert.AreEqual(s.DiscNumber, discNumberExpected);
        }

        [TestMethod]
        public void SongReload()
        {
            Song s = null;
            try
            {
                s = new Song(@"Songs\empty10sec.mp3");
            }
            catch
            {
                Assert.Fail("Load fail");
            }

            try
            {
                s.ReloadTags();
            }
            catch
            {
                Assert.Fail("Reload fail");
            }
        }
    }
}
