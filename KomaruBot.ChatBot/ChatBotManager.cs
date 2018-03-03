using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;
using Microsoft.Extensions.Logging;
using KomaruBot.Common;

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

        public void RegisterConnection(ClientConfiguration channel)
        {
            lock (chatBots)
            {
                ChatBotConnection bot;
                if (!chatBots.TryGetValue(channel.channelName, out bot))
                {
                    bot = new ChatBotConnection(this.logger, this.ChatBotTwitchUsername, this.ChatBotTwitchOauthToken, channel);
                    chatBots.Add(channel.channelName, bot);
                    this.logger.LogInformation("Started chatbot for " + channel.channelName);
                }
            }
        }
    }
}
