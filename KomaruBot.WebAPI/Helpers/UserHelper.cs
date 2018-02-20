using KomaruBot.DAL;
using KomaruBot.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Helpers
{
    public class UserHelper
    {
        private RedisContext redis { get; set; }
        public UserHelper(RedisContext redis)
        {
            this.redis = redis;
        }

        public void EnsureDefaultUserSettings(string userID, out UserBotSettings botSettings)
        {
            var key = UserBotSettings.GetKey(userID);
            botSettings = this.redis.Get<UserBotSettings>(key);
            if (botSettings == null)
            {
                botSettings = new UserBotSettings
                {
                    userID = userID,
                    currencyPlural = "Coins",
                    currencySingular = "Coin",
                    streamElementsAccountID = null,
                    streamElementsJWTToken = null,
                };
                this.redis.Set(key, botSettings);
            }
        }
        public void SaveSettings(UserBotSettings toSave)
        {
            this.redis.Set(toSave.GetKey(), toSave);
        }
    }
}
