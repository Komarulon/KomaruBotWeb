using KomaruBot.Common.Interfaces;
using KomaruBot.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KomaruBot.ChatBot
{
    public class ClientConfiguration
    {
        public string channelName { get; set; }
        public List<HypeCommand> hypeCommands { get; set; }
        public GambleConfiguration gambleConfiguration { get; set; }
        public CeresConfiguration ceresConfiguration { get; set; }
        public IPointsManager pointsManager { get; set; }
    }
}
