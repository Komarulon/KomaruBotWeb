using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KomaruBot.Common.Models
{
    public class BotAccountConfiguration
    {
        public string botIsForAccount { get; set; }

        public string username { get; set; }

        public string accessToken { get; set; }

        public static string GetKey(string username)
        {
            return $"custombot_{username}".ToLower();
        }

        public string GetKey()
        {
            return $"custombot_{this.botIsForAccount}".ToLower();
        }

        public static string GetWildcardKey()
        {
            return $"custombot_*".ToLower();
        }
    }

}
