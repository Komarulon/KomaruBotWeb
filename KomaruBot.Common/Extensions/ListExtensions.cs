using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KomaruBot.Common.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Ranks a list (so if there are ties, they get the same rank)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to sort</param>
        /// <param name="getScore">A function to get the score being used in the rank</param>
        /// <param name="setRankAndZeroBasedListIndex">An action to set the new rank, and the zero-based list index</param>
        public static void Rank<T>(this IList<T> list, Func<T, decimal> getScore, Action<T, int, int> setRankAndZeroBasedListIndex)
        {
            if (list == null || !list.Any())
            {
                return;
            }

            IList<T> orderedList;

            orderedList = list.OrderByDescending(x => getScore(x)).ToList();

            int currentRank = 0;
            int currentNumPlayers = 0;
            decimal lastScore = decimal.MinValue;
            for (int i = 0, max = orderedList.Count; i < max; i++)
            {
                var item = orderedList[i];

                currentNumPlayers++;

                var myScore = getScore(item);
                if (myScore != lastScore)
                {
                    currentRank = currentNumPlayers;
                    lastScore = myScore;
                }

                setRankAndZeroBasedListIndex(item, currentRank, i);
            }
        }
    }
}
