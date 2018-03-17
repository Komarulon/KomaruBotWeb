using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;
using Microsoft.Extensions.Logging;
using KomaruBot.Common;
using KomaruBot.Common.Models;
using KomaruBot.Common.Interfaces;

namespace KomaruBot.ChatBot
{
    public class ChatBotManager
    {

        private string ChatBotTwitchUsername { get; set; }
        private string ChatBotTwitchOauthToken { get; set; }
        private ILogger logger;
        public ChatBotManager(
            ILogger logger,
            string chatBotTwitchUsername,
            string chatBotTwitchOauthToken)
        {
            this.logger = logger;
            this.ChatBotTwitchUsername = chatBotTwitchUsername;
            this.ChatBotTwitchOauthToken = chatBotTwitchOauthToken;
        }

        private Dictionary<string, ChatBotConnection> chatBots = new Dictionary<string, ChatBotConnection>();

        /// <summary>
        /// Periodically clean up extra memory objects from the bot
        /// </summary>
        public void PeriodicCleanup()
        {
            lock (chatBots)
            {
                foreach (var a in chatBots)
                {
                    a.Value.cleanupTimeouts();
                }
            }
        }


        public void UnregisterConnection(string channelName)
        {
            lock (chatBots)
            {
                ChatBotConnection bot;
                if (chatBots.TryGetValue(channelName, out bot))
                {
                    bot.disconnect();
                    chatBots.Remove(channelName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>If a new bot was created or not</returns>
        public bool RegisterConnection(ClientConfiguration channel)
        {
            lock (chatBots)
            {
                ChatBotConnection bot;
                if (!chatBots.TryGetValue(channel.channelName, out bot))
                {
                    bot = new ChatBotConnection(this.logger, this.ChatBotTwitchUsername, this.ChatBotTwitchOauthToken, channel);
                    chatBots.Add(channel.channelName, bot);
                    this.logger.LogInformation("Started chatbot for " + channel.channelName);
                    return true;
                }
            }
            return false;
        }

        // TODO: if settings are the same, don't update!

        public void UpdateConnection(
            string channelName, 
            IPointsManager pointsManager, 
            BasicBotConfiguration basicConfig)
        {
            lock (chatBots)
            {
                ChatBotConnection bot;
                if (chatBots.TryGetValue(channelName, out bot))
                {
                    bot.ConfigurePointsManager(basicConfig, pointsManager);
                }
            }
        }

        public void UpdateConnection(string channelName, List<HypeCommand> hypeCommands)
        {
            lock (chatBots)
            {
                ChatBotConnection bot;
                if (chatBots.TryGetValue(channelName, out bot))
                {
                    bot.ConfigureHype(hypeCommands);   
                }
            }
        }

        public void UpdateConnection(string channelName, GambleConfiguration gambleSettings)
        {
            lock (chatBots)
            {
                ChatBotConnection bot;
                if (chatBots.TryGetValue(channelName, out bot))
                {
                    bot.ConfigureGamble(gambleSettings);
                }
            }
        }

        public void UpdateConnection(string channelName, CeresConfiguration ceresSettings)
        {
            lock (chatBots)
            {
                ChatBotConnection bot;
                if (chatBots.TryGetValue(channelName, out bot))
                {
                    bot.ConfigureCeres(ceresSettings);
                }
            }
        }
    }
}
