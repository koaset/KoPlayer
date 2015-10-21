using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

namespace KoPlayer.PlayLists
{
    public static class Sorting
    {
        public static string[] SortColumns { get { return sortColumns; } }
        private static readonly string[] sortColumns = { "title", "artist", 
                                                           "album", "genre", 
                                                           "rating", "play count", 
                                                           "length", "date added", 
                                                           "last played"};

        /// <summary>
        /// Creates a sorted BindingList from sort dictionaries according to the sort column and order and stores it in the last argument (ref).
        /// </summary>
        /// <param name="field"></param>
        /// <param name="sortOrder"></param>
        /// <param name="sortDictionaries"></param>
        /// <param name="sortList">ref. The sorted list is stored here</param>
        public static void Sort(string field, SortOrder sortOrder,
            List<Dictionary<string, List<Song>>> sortDictionaries, ref BindingList<Song> sortList)
        {
            Dictionary<string, List<Song>> sortDictionary = GetDictionary(field, sortDictionaries);
            if (sortDictionary == null)
                throw new PlayListException("Invalid field name");

            SortedDictionary<string, List<Song>> sortedDictionary;
            sortedDictionary = new SortedDictionary<string, List<Song>>(sortDictionary);

            IEnumerable<string> keys = sortedDictionary.Keys;
            if (sortOrder == SortOrder.Descending)
                keys = keys.Reverse();

            List<Song> songList = new List<Song>();

            foreach (string key in keys)
            {
                if (field.ToLower() == "album" || field.ToLower() == "artist")
                {
                    sortedDictionary[key].Sort(delegate(Song song1, Song song2)
                    {
                        return song1.CompareTo(song2);
                    });
                }
                songList.AddRange(sortedDictionary[key]);
            }
            sortList = new BindingList<Song>(songList);
        }

        /// <summary>
        /// Gets the dictionary of the corresponding sorting column
        /// </summary>
        /// <param name="field"></param>
        /// <param name="sortDictionaries"></param>
        /// <returns></returns>
        private static Dictionary<string, List<Song>> GetDictionary(string field,
            List<Dictionary<string, List<Song>>> sortDictionaries)
        {
            for (int i = 0; i < Sorting.SortColumns.Length; i++)
            {
                if (field.ToLower() == Sorting.SortColumns[i].ToLower())
                    return sortDictionaries[i];
            }
            return null;
        }

        /// <summary>
        /// Creates dictionaries used for sorting playlists. A dictionary is created for each unique value that can be sorted on.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="sortDictionaries"></param>
        public static void CreateSortDictionaries(BindingList<Song> sourceList,
            List<Dictionary<string, List<Song>>> sortDictionaries)
        {
            sortDictionaries.Clear();
            for (int i = 0; i < Sorting.SortColumns.Length; i++)
                sortDictionaries.Add(CreateFieldDictionary(sourceList, Sorting.SortColumns[i]));
        }

        /// <summary>
        /// Creates the dictionaries and adds all songs from the source list
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private static Dictionary<string, List<Song>> CreateFieldDictionary(BindingList<Song> sourceList, string field)
        {
            Dictionary<string, List<Song>> fieldDictionary = new Dictionary<string, List<Song>>();
            foreach (Song s in sourceList)
                AddToFieldDictionary(fieldDictionary, field, s);
            return fieldDictionary;
        }

        /// <summary>
        /// Adds a single song to one of the dictionaries.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="field"></param>
        /// <param name="song"></param>
        private static void AddToFieldDictionary(Dictionary<string, List<Song>> dictionary, string field, Song song)
        {
            if (song[field] != null && dictionary.ContainsKey(song[field]))
                dictionary[song[field]].Add(song);
            else
            {
                List<Song> newEntry = new List<Song>();
                newEntry.Add(song);
                if (song[field] != null)
                    dictionary.Add(song[field], newEntry);
            }
        }

        /// <summary>
        /// Adds a single song to the existing sortdictionaries
        /// </summary>
        /// <param name="song"></param>
        /// <param name="sortDictionaries"></param>
        public static void AddSongToSortDictionaries(Song song, List<Dictionary<string,
            List<Song>>> sortDictionaries)
        {
            for (int i = 0; i < Sorting.SortColumns.Length; i++)
                AddToFieldDictionary(sortDictionaries[i], Sorting.SortColumns[i], song);
        }

        public static void RemoveFromSortDictionary(Dictionary<string, List<Song>> dictionary, string field, Song song)
        {
            if (dictionary.ContainsKey(song[field]))
            {
                dictionary[song[field]].Remove(song);
                if (dictionary[song[field]].Count == 0)
                    dictionary.Remove(song[field]);
            }
        }

        public static void RemoveSongFromSortDictionaries(Song song,
            List<Dictionary<string, List<Song>>> sortDictionaries)
        {
            for (int i = 0; i < Sorting.SortColumns.Length; i++)
                RemoveFromSortDictionary(sortDictionaries[i], Sorting.SortColumns[i], song);
        }
    }
}
