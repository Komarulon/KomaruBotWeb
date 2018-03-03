using KomaruBot.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Models
{
    public class CeresSettings : BotSettingsBase
    {
        public static string GetKey(string userID)
        {
            return $"ceres_{userID}";
        }

        public string GetKey()
        {
            return $"ceres_{this.userID}";
        }

        public Common.Models.CeresConfiguration ceresConfiguration { get; set; }

        public ValidationResult validate()
        {
            return ValidationResult.Success;
        }
    }
}
