using KomaruBot.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Models
{
    public class HypeCommandsSettings : BotSettingsBase
    {
        public static string GetKey(string userID)
        {
            return $"hype_{userID}";
        }

        public string GetKey()
        {
            return $"hype_{this.userID}";
        }

        public List<HypeCommand> hypeCommands { get; set; }
    }
}
