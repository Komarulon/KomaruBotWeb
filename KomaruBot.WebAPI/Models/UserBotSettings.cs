using KomaruBot.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public string streamElementsJWTToken { get; set; }

        public string streamElementsAccountID { get; set; }

        public string currencySingular { get; set; }

        public string currencyPlural { get; set; }

        public BasicBotConfiguration basicBotConfiguration { get; set; }

        public ValidationResult validate()
        {
            if (basicBotConfiguration == null)
            {
                return new ValidationResult("There was an error saving the bot configuration.");
            }

            if (string.IsNullOrWhiteSpace(streamElementsJWTToken))
            {
                return new ValidationResult("You must provide a StreamElements JWT Token");
            }

            if (string.IsNullOrWhiteSpace(streamElementsAccountID))
            {
                return new ValidationResult("You must provide a StreamElements Account ID");
            }

            if (string.IsNullOrWhiteSpace(currencySingular))
            {
                return new ValidationResult("You must provide a singular name for your currency");
            }

            if (string.IsNullOrWhiteSpace(currencyPlural))
            {
                return new ValidationResult("You must provide a plural name for your currency");
            }

            var basicValidation = basicBotConfiguration.validate();
            if (basicValidation != null && basicValidation != ValidationResult.Success)
            {
                return basicValidation;
            }

            return ValidationResult.Success;
        }

    }
}
