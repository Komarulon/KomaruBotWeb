using KomaruBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchLib.Models.Client;

namespace KomaruBot.ChatBot
{
    public class Command
    {
        


        public Constants.CommandType commandType { get; private set; }
        public string commandText { get; private set; }
        public Action<ChatMessage, Command> onRun { get; private set; }
        public Constants.AccessLevel requiredAccessLevel { get; private set; }
        public bool requiredRoundStarted { get; private set; }
        public bool requiredRoundNotStarted { get; private set; }
        public Command(
            Constants.CommandType commandType,
            Action<ChatMessage, Command> onRun,
            Constants.AccessLevel requiredAccessLevel,
            bool requiredRoundStarted,
            bool requiredRoundNotStarted,
            string commandText)
        {
            this.commandType = commandType;
            this.onRun = onRun;
            this.requiredAccessLevel = requiredAccessLevel;
            this.requiredRoundStarted = requiredRoundStarted;
            this.requiredRoundNotStarted = requiredRoundNotStarted;
            this.commandText = commandText;
        }
    }
}
