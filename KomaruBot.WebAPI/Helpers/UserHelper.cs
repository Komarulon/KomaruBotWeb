using KomaruBot.Common.PointsManagers;
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

        public UserBotSettings EnsureDefaultUserSettings(string userID)
        {
            var key = UserBotSettings.GetKey(userID);
            var botSettings = this.redis.Get<UserBotSettings>(key);
            if (botSettings == null)
            {
                botSettings = new UserBotSettings
                {
                    userID = userID,
                    currencyPlural = "Coins",
                    currencySingular = "Coin",
                    streamElementsAccountID = null,
                    streamElementsJWTToken = null,
                    botEnabled = false,
                };
                this.redis.Set(key, botSettings);
            }

            return botSettings;
        }

        public HypeCommandsSettings EnsureDefaultHypeSettings(string userID)
        {
            var key = HypeCommandsSettings.GetKey(userID);
            var hypeCommandsSettings = this.redis.Get<HypeCommandsSettings>(key);
            if (hypeCommandsSettings == null)
            {
                hypeCommandsSettings = new HypeCommandsSettings
                {
                    userID = userID,
                    hypeCommands = new List<Common.Models.HypeCommand>
                    {
                        new Common.Models.HypeCommand
                        {
                            accessLevel = (int)KomaruBot.Common.Constants.AccessLevel.Public,
                            commandResponses = new List<string>
                            {
                                "D e e R F o r C e",
                            },
                            commandText = "!deerforce",
                            numberOfResponses = 1,
                            pointsCost = 0,
                            randomizeResponseOrders = false,
                        },
                    },
                };
                this.redis.Set(key, hypeCommandsSettings);
            }

            return hypeCommandsSettings;
        }

        public GambleSettings EnsureDefaultGambleSettings(string userID)
        {
            var key = GambleSettings.GetKey(userID);
            var gambleSettings = this.redis.Get<GambleSettings>(key);
            if (gambleSettings == null)
            {
                gambleSettings = new GambleSettings
                {
                    userID = userID,
                    gambleConfiguration = new Common.Models.GambleConfiguration
                    {
                        gambleEnabled = false,
                        minBid = 10,
                        maxBid = 1000,
                        minMinutesBetweenGambles = 15,
                        rollResults = new List<Common.Models.GambleRewards>
                        {
                            new Common.Models.GambleRewards { roll = 100, multiplier = 5m },
                            new Common.Models.GambleRewards { roll = 99, multiplier = 3m },
                            new Common.Models.GambleRewards { roll = 98, multiplier = 3m },
                            new Common.Models.GambleRewards { roll = 97, multiplier = 2.5m },
                            new Common.Models.GambleRewards { roll = 96, multiplier = 2.25m },
                            new Common.Models.GambleRewards { roll = 95, multiplier = 2m },
                            new Common.Models.GambleRewards { roll = 94, multiplier = 2m },
                            new Common.Models.GambleRewards { roll = 93, multiplier = 2m },
                            new Common.Models.GambleRewards { roll = 92, multiplier = 2m },
                            new Common.Models.GambleRewards { roll = 91, multiplier = 2m },
                            new Common.Models.GambleRewards { roll = 90, multiplier = 2m },
                            new Common.Models.GambleRewards { roll = 89, multiplier = 1.75m },
                            new Common.Models.GambleRewards { roll = 88, multiplier = 1.75m },
                            new Common.Models.GambleRewards { roll = 87, multiplier = 1.75m },
                            new Common.Models.GambleRewards { roll = 86, multiplier = 1.75m },
                            new Common.Models.GambleRewards { roll = 85, multiplier = 1.75m },
                            new Common.Models.GambleRewards { roll = 84, multiplier = 1.5m },
                            new Common.Models.GambleRewards { roll = 83, multiplier = 1.5m },
                            new Common.Models.GambleRewards { roll = 82, multiplier = 1.5m },
                            new Common.Models.GambleRewards { roll = 81, multiplier = 1.5m },
                            new Common.Models.GambleRewards { roll = 80, multiplier = 1.5m },
                            new Common.Models.GambleRewards { roll = 79, multiplier = 1.5m },
                            new Common.Models.GambleRewards { roll = 78, multiplier = 1.5m },
                            new Common.Models.GambleRewards { roll = 77, multiplier = 1.5m },
                            new Common.Models.GambleRewards { roll = 76, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 75, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 74, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 73, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 72, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 71, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 70, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 69, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 68, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 67, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 66, multiplier = 1.25m },
                            new Common.Models.GambleRewards { roll = 65, multiplier = 1m },
                            new Common.Models.GambleRewards { roll = 64, multiplier = 1m },
                            new Common.Models.GambleRewards { roll = 63, multiplier = 1m },
                            new Common.Models.GambleRewards { roll = 62, multiplier = 0.75m },
                            new Common.Models.GambleRewards { roll = 61, multiplier = 0.75m },
                            new Common.Models.GambleRewards { roll = 60, multiplier = 0.75m },
                            new Common.Models.GambleRewards { roll = 59, multiplier = 0.5m },
                            new Common.Models.GambleRewards { roll = 58, multiplier = 0.5m },
                            new Common.Models.GambleRewards { roll = 57, multiplier = 0.5m },
                            new Common.Models.GambleRewards { roll = 56, multiplier = 0.25m },
                            new Common.Models.GambleRewards { roll = 55, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 54, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 53, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 52, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 51, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 50, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 49, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 48, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 47, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 46, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 45, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 44, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 43, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 42, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 41, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 40, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 39, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 38, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 37, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 36, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 35, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 34, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 33, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 32, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 31, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 30, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 29, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 28, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 27, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 26, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 25, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 24, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 23, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 22, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 21, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 20, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 19, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 18, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 17, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 16, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 15, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 14, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 13, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 12, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 11, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 10, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 9, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 8, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 7, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 6, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 5, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 4, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 3, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 2, multiplier = 0m },
                            new Common.Models.GambleRewards { roll = 1, multiplier = 0m }
                        },
                    }
                };
                this.redis.Set(key, gambleSettings);
            }

            return gambleSettings;
        }

        public CeresSettings EnsureDefaultCeresSettings(string userID)
        {
            var key = CeresSettings.GetKey(userID);
            var ceresSettings = this.redis.Get<CeresSettings>(key);
            if (ceresSettings == null)
            {
                ceresSettings = new CeresSettings
                {
                    ceresConfiguration = new Common.Models.CeresConfiguration
                    {
                        ceresEnabled = false,
                    },
                    userID = userID,
                };
                this.redis.Set(key, ceresSettings);
            }

            return ceresSettings;
        }


        public void SaveSettings(UserBotSettings toSave) { this.redis.Set(toSave.GetKey(), toSave); }

        public void SaveSettings(HypeCommandsSettings toSave) { this.redis.Set(toSave.GetKey(), toSave); }

        public void SaveSettings(GambleSettings toSave) { this.redis.Set(toSave.GetKey(), toSave); }

        public void SaveSettings(CeresSettings toSave) { this.redis.Set(toSave.GetKey(), toSave); }

        public ChatBot.ClientConfiguration GetConfigurationForUser(string userID)
        {
            var botSettings = this.redis.Get<UserBotSettings>(UserBotSettings.GetKey(userID));
            return this.GetConfigurationForUser(botSettings);
        }

        public ChatBot.ClientConfiguration GetConfigurationForUser(UserBotSettings botSettings)
        {
            if (botSettings == null)
            {
                return null;
            }

            if (!botSettings.botEnabled)
            {
                return null;
            }

            var hypeCommandsSettings = this.EnsureDefaultHypeSettings(botSettings.userID);
            var gambleSettings = this.EnsureDefaultGambleSettings(botSettings.userID);
            var ceresSettings = this.EnsureDefaultCeresSettings(botSettings.userID);

            var pointsManager = new StreamElementsPointsManager(
                Startup._loggerFactory.CreateLogger("StreamElementsPointsManager"),
                botSettings.streamElementsJWTToken,
                botSettings.currencyPlural,
                botSettings.currencySingular,
                botSettings.streamElementsAccountID);

            var config = new ChatBot.ClientConfiguration
            {
                channelName = botSettings.userID,
                hypeCommands = hypeCommandsSettings.hypeCommands,
                gambleConfiguration = gambleSettings.gambleConfiguration,
                pointsManager = pointsManager,
                ceresConfiguration = ceresSettings.ceresConfiguration,
            };

            return config;
        }

        public List<ChatBot.ClientConfiguration> GetAllUserClientConfigurations()
        {
            var allKeys = this.redis.GetKeysByPattern_SLOW(UserBotSettings.GetWildcardKey());
            var res = new List<ChatBot.ClientConfiguration>();
            foreach (var k in allKeys)
            {
                var botSettings = this.redis.Get<UserBotSettings>(k);
                if (botSettings != null)
                {
                    var config = this.GetConfigurationForUser(botSettings);
                    if (config != null)
                    {
                        res.Add(config);
                    }
                }
                
            }

            return res;
        }
    }
}
