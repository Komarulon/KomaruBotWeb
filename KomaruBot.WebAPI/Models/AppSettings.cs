using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Models
{
    public class AppSettings
    {
        public string ClientUrl { get; set; }
        public string ClientAuthRedirectRoute { get; set; }
        public string TwitchClientID { get; set; }
        public string TwitchClientSecret { get; set; }
        public string TwitchScopes { get; set; }
        public string TwitchOauthEndpoint { get; set; }
        public string RedisConnectionString { get; set; }

        public string ChatBotTwitchUsername { get; set; }
        public string ChatBotTwitchOauthToken { get; set; }
    }

    
}

