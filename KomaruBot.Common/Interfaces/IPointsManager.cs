using System;
using System.Collections.Generic;
using System.Text;

namespace KomaruBot.Common.Interfaces
{
    public interface IPointsManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="amount"></param>
        /// <param name="newAmount">The amount of points the user has after this transaction, if available</param>
        void GivePlayerPoints(string userName, long amount, out long? newAmount);

        long GetCurrentPlayerPoints(string userName);

        string CurrencyPlural { get; }
        string CurrencySingular { get; }

        bool CheckSettingsCorrect(string userID);
    }
}
