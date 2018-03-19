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
            return $"ceres_{userID}".ToLower();
        }

        public string GetKey()
        {
            return $"ceres_{this.userID}".ToLower();
        }

        public Common.Models.CeresConfiguration ceresConfiguration { get; set; }

        public ValidationResult validate()
        {
            if (ceresConfiguration == null)
            {
                return new ValidationResult("There was an error saving the ceres configuration.");
            }

            if (ceresConfiguration.numberOfSecondsToGuess < 1)
            {
                return new ValidationResult("The number of seconds to guess is too low. ");
            }

            if (ceresConfiguration.magicTimes == null)
            {
                ceresConfiguration.magicTimes = new List<CeresConfiguration.MagicTime>();
            }

            if (ceresConfiguration.staticRewards == null)
            {
                ceresConfiguration.staticRewards = new List<CeresConfiguration.StaticReward>();
            }

            if (ceresConfiguration.closestRewards == null)
            {
                ceresConfiguration.closestRewards = new List<CeresConfiguration.ClosestReward>();
            }

            foreach (var time in ceresConfiguration.magicTimes)
            {
                if (time.ceresTime < 1)
                {
                    return new ValidationResult($"The Magic Times must be positive.");
                }

                if (time.ceresTime.ToString().Length != 4)
                {
                    return new ValidationResult($"The Magic Time {time.ceresTime} must have exactly 4 digits (for example, a time of 45:79 would be 4579");
                }


                if (time.pointsAwarded < 1)
                {
                    return new ValidationResult($"The amount of points awarded when a Magic Time of {time.ceresTime} cannot be less than 1");
                }
            }

            foreach (var reward in ceresConfiguration.staticRewards)
            {
                if (reward.hundrethsLeewayStart < 0)
                {
                    return new ValidationResult($"(Static Rewards) The leeway start for guesses between {reward.hundrethsLeewayStart} and {reward.hundrethsLeewayEnd} hundreths of a second cannot be less than 1");
                }

                if (reward.hundrethsLeewayEnd < 0)
                {
                    return new ValidationResult($"(Static Rewards) The leeway end for guesses between {reward.hundrethsLeewayStart} and {reward.hundrethsLeewayEnd} hundreths of a second cannot be less than 1");
                }

                if (reward.hundrethsLeewayEnd < reward.hundrethsLeewayStart)
                {
                    return new ValidationResult($"(Static Rewards) The leeway end for guesses between {reward.hundrethsLeewayStart} and {reward.hundrethsLeewayEnd} hundreths of a second cannot be less than the leeway start");
                }

                if (reward.pointsAwarded < 1)
                {
                    return new ValidationResult($"(Static Rewards) The amount of points awarded to guesses between {reward.hundrethsLeewayStart} and {reward.hundrethsLeewayEnd} hundreths of a second cannot be less than 1");
                }
            }

            foreach (var reward in ceresConfiguration.closestRewards)
            {
                if (reward.rankAwarded < 1)
                {
                    return new ValidationResult("The ranks for Closest Rewards must be greater than 0. 1 is 1st, 2 is 2nd, etc.");
                }

                if (reward.pointsAwarded < 1)
                {
                    return new ValidationResult("The amount of points awarded to rank " + reward.rankAwarded + " cannot be less than 1");
                }
            }


            

            return ValidationResult.Success;
        }
    }
}
