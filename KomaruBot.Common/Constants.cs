using System;
using System.Collections.Generic;
using System.Text;

namespace KomaruBot.Common
{
    public class Constants
    {
        public enum CommandType
        {
            Guess,
            EndCeres,
            CancelCeres,
            StartCeres,
            GetPoints,
            Leaderboard,
            Stats,
            Help,
            About,
            Gamble,
            Hype
        }

        public enum AccessLevel
        {
            Public = 0,
            Moderator = 1,
            Broadcaster = 2,
        }
    }
}
