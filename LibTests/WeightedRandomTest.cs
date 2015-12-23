using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KoPlayer.Lib;
using System.Collections.Generic;
using System.Linq;

namespace LibTests
{
    [TestClass]
    public class WeightedRandomTest
    {
        [TestMethod]
        public void GetRandomWeighRating()
        {
            var lib = SetupLibrary();
            var songList = lib.GetSongs();

            var ratingDicts = lib.SortDictionaries[Array.IndexOf(Sorting.SortColumns, "rating")];


            foreach (var key in ratingDicts.Keys)
                Assert.AreEqual(1, ratingDicts[key].Count);

            int numSongs = 5000;

            // ratingRelation = 1 -  ((firstRating + 1) / (secondRating + 1))

            TestRelation(numSongs, 0.5f, lib, 1, 3);

            songList[0].Rating = 2;
            lib.UpdateSongInfo(songList[0]);

            TestRelation(numSongs, 0.6f, lib, 1, 2);
        }

        private void TestRelation(int numSongs, float ratingRelation, PlaylistBase randomSource,
            int firstRating, int secondRating)
        {
            var list = new List<Song>();

            float lower = ratingRelation - 0.20f;
            float upper = ratingRelation + 0.20f;

            for (int run = 0; run < 100; run++)
            {
                for (int i = 0; i < numSongs; i++)
                    list.Add(randomSource.GetRandom(true));

                int c1 = list.Where(s => s.Rating == firstRating).Count();
                int c2 = list.Where(s => s.Rating == secondRating).Count();

                if (!(c1 > c2 * lower && c1 < c2 * upper))
                    Assert.Fail();

                list.Clear();
            }
        }

        private Library SetupLibrary()
        {
            var lib = new Library();
            AddSongsToLibrary(lib);
            return lib;
        }

        private void AddSongsToLibrary(Library lib)
        {
            var song = new Song(@"Songs\empty10sec.mp3");
            song.Rating = 3;
            lib.Add(song);

            song = new Song(@"Songs\empty10sec2.mp3");
            song.Rating = 1;
            lib.Add(song);
        }
    }
}
