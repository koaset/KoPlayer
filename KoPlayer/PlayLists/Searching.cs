using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoPlayer.PlayLists
{
    public static class Searching
    {
        /// <summary>
        /// Divides the search string into words and returns all keys where the field contains all words
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static List<string> SeparateWordAndSearch(string searchString,
            Dictionary<string, List<Song>> dictionary)
        {
            string[] searchTerms = searchString.Split(' ');
            List<List<string>> matchingKeyLists = new List<List<string>>();
            foreach (string term in searchTerms)
            {
                if (term.Length > 0)
                {
                    List<string> matchingKeyList = dictionary.Where(x => x.Key.ToLower().Contains(term)).Select(x => x.Key).ToList();
                    matchingKeyLists.Add(matchingKeyList);
                }
            }

            List<string> keysToAdd = new List<string>();

            if (matchingKeyLists.Count > 1)
            {
                List<string> comparer = matchingKeyLists[0];
                matchingKeyLists.Remove(comparer);

                foreach (string key in comparer)
                {
                    bool inAll = true;
                    foreach (List<string> otherList in matchingKeyLists)
                    {
                        if (!otherList.Contains(key))
                            inAll = false;
                    }
                    if (inAll)
                        keysToAdd.Add(key);
                }
            }
            else
                keysToAdd.AddRange(matchingKeyLists[0]);
            return keysToAdd;
        }

        /// <summary>
        /// Divides the search string into words and returns all keys contaning any word
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static List<string> SeparateWordOrSearch(string searchString,
            Dictionary<string, List<Song>> dictionary)
        {
            string[] searchTerms = searchString.Split(' ');
            List<List<string>> matchingKeyLists = new List<List<string>>();
            foreach (string term in searchTerms)
            {
                if (term.Length > 0)
                {
                    List<string> matchingKeyList = dictionary.Where(x => x.Key.ToLower().Contains(term)).Select(x => x.Key).ToList();
                    matchingKeyLists.Add(matchingKeyList);
                }
            }
            List<string> keysToAdd = new List<string>();

            keysToAdd.AddRange(matchingKeyLists[0]);
            matchingKeyLists.Remove(matchingKeyLists[0]);

            if (matchingKeyLists.Count > 0)
            {
                foreach (List<string> matchingKeyList in matchingKeyLists)
                    foreach (string key in matchingKeyList)
                        if (!keysToAdd.Contains(key))
                            keysToAdd.Add(key);
            }
            return keysToAdd;
        }

        /// <summary>
        /// Gets all keys containing the whole search string in one piece
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static List<string> WholeStringKeySearch(string searchString,
            Dictionary<string, List<Song>> dictionary)
        {
            return dictionary.Where(x => x.Key.ToLower().Contains(searchString)).Select(x => x.Key).ToList();
        }
    }
}
