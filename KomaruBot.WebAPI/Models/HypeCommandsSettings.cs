using KomaruBot.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public ValidationResult validate()
        {
            if (hypeCommands == null)
            {
                hypeCommands = new List<HypeCommand>();
            }
            foreach (var a in hypeCommands)
            {
                if (string.IsNullOrEmpty(a.commandText))
                {
                    return new ValidationResult("All Hype Commands must have command text");
                }

                if (!a.commandText.StartsWith("!"))
                {
                    return new ValidationResult("All Hype Commands must start with '!'");
                }

                if (a.commandText.Length == 1)
                {
                    return new ValidationResult("The Command " + a.commandText + " must be longer. Make sure it does not collide with any other commands.");
                }

                if (a.pointsCost < 0)
                {
                    return new ValidationResult("The command " + a.commandText + " cannot cost negative points");
                }

                if (a.commandResponses.Count < 1)
                {
                    return new ValidationResult("The command " + a.commandText + " needs at least one response");
                }

                if (a.numberOfResponses < 1)
                {
                    return new ValidationResult("The Number of Responses for command " + a.commandText + " cannot be less than 1");
                }

                if (!Enum.IsDefined(typeof(Common.Constants.AccessLevel), a.accessLevel))
                {
                    return new ValidationResult("The access level for command " + a.commandText + " must be 0, 1, or 2 (Public, Moderator, or Broadcaster)");
                }
            }

            return ValidationResult.Success;
        }
    }
}
