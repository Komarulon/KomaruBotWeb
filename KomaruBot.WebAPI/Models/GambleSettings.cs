using KomaruBot.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Models
{
    public class GambleSettings : BotSettingsBase
    {
        public static string GetKey(string userID)
        {
            return $"gamble_{userID}";
        }

        public string GetKey()
        {
            return $"gamble_{this.userID}";
        }

        public Common.Models.GambleConfiguration gambleConfiguration{ get; set; }

        public ValidationResult validate()
        {
            if (gambleConfiguration == null)
            {
                return new ValidationResult("There was an error saving the gamble configuration.");
            }

            if (gambleConfiguration.rollResults == null)
            {
                return new ValidationResult("There was an error saving the gamble configuration rolls.");
            }


            var onetohundred = Enumerable.Range(1, 100);
            foreach (var a in onetohundred)
            {
                var roll = gambleConfiguration.rollResults.Find(x => x.roll == a);
                if (roll == null)
                {
                    return new ValidationResult("There must be a result for rolling a " + a);
                }
                
                if (roll.multiplier < 0)
                {
                    return new ValidationResult("There result for rolling a " + a + " cannot be a negative multiplier (0 means lose all money, 0.5 means lose half, 2 means double)");
                }

            }

            return ValidationResult.Success;
        }
    }
}
