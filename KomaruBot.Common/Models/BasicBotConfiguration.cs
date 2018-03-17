using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KomaruBot.Common.Models
{
    public class BasicBotConfiguration
    {
        public string queryPointsCommand { get; set; }

        public string helpCommand { get; set; }

        public bool botEnabled { get; set; }

        public ValidationResult validate()
        {
            if (string.IsNullOrWhiteSpace(queryPointsCommand))
            {
                return new ValidationResult("You must provide a Query Points Command");
            }

            if (!queryPointsCommand.StartsWith("!"))
            {
                return new ValidationResult("The Query Points Command must start with '!'");
            }

            if (queryPointsCommand.Length == 1)
            {
                return new ValidationResult("The Query Points Command must be longer. Make sure it does not collide with any other commands.");
            }

            if (!helpCommand.StartsWith("!"))
            {
                return new ValidationResult("The Query Points Command must start with '!'");
            }

            if (helpCommand.Length == 1)
            {
                return new ValidationResult("The Query Points Command must be longer. Make sure it does not collide with any other commands.");
            }

            return ValidationResult.Success;
        }
    }

}
