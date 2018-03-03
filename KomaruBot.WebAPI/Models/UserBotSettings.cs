using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Models
{
    public class UserBotSettings : BotSettingsBase
    {
        public static string GetKey(string userID)
        {
            return $"bot_{userID}";
        }

        public string GetKey()
        {
            return $"bot_{this.userID}";
        }

        public static string GetWildcardKey()
        {
            return $"bot_*";
        }


        //public string TwitchAPIKey { get; set; }

        public string streamElementsJWTToken { get; set; }

        public string streamElementsAccountID { get; set; }

        public string currencySingular { get; set; }

        public string currencyPlural { get; set; }

        public bool botEnabled { get; set; }
    }
}
