using KomaruBot.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Models
{
    public class AlternativeUserBotRequest
    {
        public static string GetKey(string botUserID)
        {
            return $"aubr_{botUserID}".ToLower();
        }

        public string GetKey()
        {
            return $"aubr_{this.requestedBotUsername}".ToLower();
        }

        public static string GetWildcardKey()
        {
            return $"aubr_*".ToLower();
        }

        public string requestedBotUsername { get; set; }

        public string requestingUsername { get; set; }

        public string requestingUserID { get; set; }


    }
}
