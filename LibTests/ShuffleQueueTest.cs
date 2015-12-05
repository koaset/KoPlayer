using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using KoPlayer.Lib;

namespace LibTests
{
    [TestClass]
    public class ShuffleQueueTest
    {
        [TestMethod]
        public void ShuffleQueueCreate()
        {
            try
            {
                var shuffleQueue = Create();
            }
            catch
            {
                Assert.Fail();
            }
        }

        private ShuffleQueue Create()
        {
            var lib = Library.Load();
            var set = new KoPlayer.Settings();
            return new ShuffleQueue(lib, set);
        }

        [TestMethod]
        public void ShuffleQueuePopulate()
        {
            var sq = Create();
            sq.Populate();

            int expectedNumSongs = sq.Settings.Shufflequeue_NumNext + 1;
            

            for (int i = 0; i < sq.Settings.Shufflequeue_NumPrevious + 5; i++)
            {
                Assert.AreEqual(expectedNumSongs, sq.NumSongs, "i: " + i);
                
                sq.GetNext();
                sq.Populate();

                if (i < sq.Settings.Shufflequeue_NumPrevious)
                    expectedNumSongs++;
            }
        }
    }
}
