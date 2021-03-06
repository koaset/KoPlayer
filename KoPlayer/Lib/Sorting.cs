﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System;

namespace KoPlayer.Lib
{
    public static class Sorting
    {
        /// <summary>
        /// Holds the names of the columns where sorting is avaliable
        /// </summary>
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
        public static List<Song> Sort(string field, SortOrder sortOrder,
            List<Dictionary<string, List<Song>>> sortDictionaries, List<Song> sortList)
        {
            field = field.ToLower();

            if (field == "length")
                return SortLength(sortOrder, sortList);
            else if (field == "date added")
                return SortDate(sortOrder, sortList);

            //Get the dictionary for the corresponding column
            Dictionary<string, List<Song>> sortDictionary = GetDictionary(field, sortDictionaries);

            if (sortDictionary == null)
                throw new PlaylistException("Invalid field name");

            //Create a dictionary sorted on they keys
            var sortedDictionary = new SortedDictionary<string, List<Song>>(sortDictionary);

            //Get list of keys and reverse order if needed
            IEnumerable<string> keys = sortedDictionary.Keys;
            if (sortOrder == SortOrder.Descending)
                keys = keys.Reverse();

            //Create a new list to add sorted songs to
            List<Song> songList = new List<Song>();
            foreach (string key in keys)
            {
                // Sorts on track and disc number before sorting if album or artist sorting is chosen
                if (field == "album" || field == "artist")
                    sortedDictionary[key].Sort((s1,s2) => s1.CompareTo(s2));

                // Adds all songs for the current key to the output list
                songList.AddRange(sortedDictionary[key]);
            }

            //Sets output. This is a reference
            return songList;
        }

        public static List<Song> SortLength(SortOrder sortOrder, List<Song> sortList)
        {
            var sortedList = sortList.ToList();

            sortedList.Sort((s1, s2) => s1.Length.CompareTo(s2.Length));

            if (sortOrder == SortOrder.Descending)
                sortedList.Reverse();

            return sortedList;
        }

        public static List<Song> SortDate(SortOrder sortOrder, List<Song> sortList)
        {
            var sortedList = sortList.ToList();

            sortedList.Sort((s1, s2) => s1.DateAdded.CompareTo(s2.DateAdded));

            if (sortOrder == SortOrder.Descending)
                sortedList.Reverse();

            return sortedList;
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
                if (field.ToLower() == Sorting.SortColumns[i].ToLower())
                    return sortDictionaries[i];
            return null;
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
                AddSongToFieldDictionary(sortDictionaries[i], Sorting.SortColumns[i], song);
        }

        /// <summary>
        /// Rem
        /// </summary>
        /// <param name="song"></param>
        /// <param name="sortDictionaries"></param>
        public static void RemoveSongFromSortDictionaries(Song song,
            List<Dictionary<string, List<Song>>> sortDictionaries)
        {
            for (int i = 0; i < Sorting.SortColumns.Length; i++)
                RemoveSongFromFieldDictionary(sortDictionaries[i], Sorting.SortColumns[i], song);
        }

        /// <summary>
        /// Creates dictionaries used for sorting playlists. A dictionary is created for each unique value that can be sorted on.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="sortDictionaries"></param>
        public static void CreateSortDictionaries(List<Song> sourceList,
            List<Dictionary<string, List<Song>>> sortDictionaries)
        {
            sortDictionaries.Clear();
            for (int i = 0; i < Sorting.SortColumns.Length; i++)
                sortDictionaries.Add(CreateFieldDictionary(sourceList, Sorting.SortColumns[i]));

            if (sourceList.Count == 2)
            {
                int i = 2;
                i++;
            }

        }

        /// <summary>
        /// Creates the dictionaries and adds all songs from the source list
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Song>> CreateFieldDictionary(List<Song> sourceList, string field)
        {
            var fieldDictionary = new Dictionary<string, List<Song>>();
            foreach (Song s in sourceList)
                AddSongToFieldDictionary(fieldDictionary, field, s);
            return fieldDictionary;
        }

        /// <summary>
        /// Adds a single song to one of the dictionaries.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="field"></param>
        /// <param name="song"></param>
        private static void AddSongToFieldDictionary(Dictionary<string, List<Song>> dictionary, string field, Song song)
        {
            if (song[field] != null && dictionary.ContainsKey(song[field]))
                dictionary[song[field]].Add(song);
            else
            {
                var newEntry = new List<Song>();
                newEntry.Add(song);
                if (song[field] != null)
                    dictionary.Add(song[field], newEntry);
            }
        }

        /// <summary>
        /// Removes one song from a dictionary
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="field"></param>
        /// <param name="song"></param>
        private static void RemoveSongFromFieldDictionary(Dictionary<string, List<Song>> dictionary, string field, Song song)
        {
            if (dictionary.ContainsKey(song[field]))
            {
                dictionary[song[field]].Remove(song);
                if (dictionary[song[field]].Count == 0)
                    dictionary.Remove(song[field]);
            }
        }
    }
}
