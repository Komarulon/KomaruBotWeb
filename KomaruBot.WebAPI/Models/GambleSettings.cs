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
            return $"gamble_{userID}".ToLower();
        }

        public string GetKey()
        {
            return $"gamble_{this.userID}".ToLower();
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

            if (gambleConfiguration.minBid < 0)
            {
                return new ValidationResult("The minimum bid must be 0 or greater");
            }

            if (gambleConfiguration.maxBid < 0)
            {
                return new ValidationResult("The maximum bid must be 0 or greater");
            }

            if (gambleConfiguration.minMinutesBetweenGambles < 0)
            {
                return new ValidationResult("The minimum minutes between gambles must be 0 or greater");
            }

            if (gambleConfiguration.maxBid < gambleConfiguration.minBid)
            {
                return new ValidationResult("The maximum bid can't be lower than the minimum bid");
            }

            if (string.IsNullOrWhiteSpace(gambleConfiguration.gambleCommand))
            {
                return new ValidationResult("You must set a gamble command");
            }

            if (!gambleConfiguration.gambleCommand.StartsWith("!"))
            {
                return new ValidationResult("The gamble command must start with '!'");
            }

            if (gambleConfiguration.gambleCommand.Length == 1)
            {
                return new ValidationResult("The gamble command must be longer. Make sure it does not collide with any other commands.");
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

            if (gambleConfiguration.rollResults.Count != onetohundred.Count())
            {
                return new ValidationResult("There cannot be more than " + onetohundred.Count() + " rolls");
            }

            return ValidationResult.Success;
        }
    }
}
