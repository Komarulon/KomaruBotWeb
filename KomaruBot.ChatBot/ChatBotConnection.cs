using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;
using Microsoft.Extensions.Logging;
using KomaruBot.Common;
using System.Linq;
using KomaruBot.ChatBot.Ceres;
using KomaruBot.Common.Models;
using KomaruBot.Common.Interfaces;

namespace KomaruBot.ChatBot
{
    public class ChatBotConnection
    {

        private ILogger logger;
        private string ChatBotTwitchUsername;
        private string ChatBotTwitchOauthToken;
        private ClientConfiguration channelDetails;
        private ITwitchClient twitchClient;

        private CeresGuessingGame ceres;

        private static int gambleTimeoutSecondsDefault = 30;
        private static int numberOfMessagesAllowed = 16;
        private static int secondsThrottled = 15;
        private static Random rnd = new Random();

        public ChatBotConnection(
            ILogger logger,
            string chatBotTwitchUsername,
            string chatBotTwitchOauthToken,
            ClientConfiguration channelDetails)
        {
            this.logger = logger;
            this.ChatBotTwitchUsername = chatBotTwitchUsername;
            this.ChatBotTwitchOauthToken = chatBotTwitchOauthToken;
            this.channelDetails = channelDetails;

            lock (this.commands)
            {
                this.connect();
                this.ConfigureCeres(this.channelDetails.ceresConfiguration);
                this.ConfigureHype(this.channelDetails.hypeCommands);
                this.ConfigureGamble(this.channelDetails.gambleConfiguration);
                this.ConfigurePointsManager(this.channelDetails.basicConfiguration, this.channelDetails.pointsManager);
            }
        }

        public void ConfigurePointsManager(BasicBotConfiguration basicBotConfigurationCommand, IPointsManager newSettings)
        {
            lock (this.commands)
            {
                this.channelDetails.pointsManager = newSettings;

                this.commands.RemoveAll(x => x.commandType == Constants.CommandType.GetPoints ||
                                             x.commandType == Constants.CommandType.Help);

                this.commands.Add(new Command(Constants.CommandType.GetPoints, (message, cmd) =>
                    {
                        var points = this.channelDetails.pointsManager.GetCurrentPlayerPoints(message.Username);
                        var curString = (points == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural);
                        this.sendMessage($"{message.Username} has {points} {curString}");
                    },
                    Constants.AccessLevel.Public,
                    false,
                    false,
                    basicBotConfigurationCommand.queryPointsCommand,
                    false,
                    null,
                    30
                ));

                this.commands.Add(new Command(Constants.CommandType.Help, (message, cmd) =>
                    {
                        this.sendMessage(this._helpString);
                    },
                    Constants.AccessLevel.Public,
                    false,
                    false,
                    basicBotConfigurationCommand.helpCommand,
                    false,
                    20,
                    null
                ));

                updateHelpString();
            }
        }

        
        private object _helpLock = new object();
        private string _helpString;
        private void updateHelpString()
        {
            lock (_helpLock)
            {
                var strs = new List<string>();
                var cmd = this.commands.Find(x => x.commandType == Constants.CommandType.GetPoints);
                if (cmd != null)
                {
                    strs.Add($"\"{cmd.commandText}\" - get your current {this.channelDetails.pointsManager.CurrencyPlural}");
                }

                if (this.ceres != null)
                {
                    cmd = this.commands.Find(x => x.commandType == Constants.CommandType.Guess);
                    if (cmd != null)
                    {
                        strs.Add($"\"{cmd.commandText} [guess (4 digits)]\" - during a live ceres round, enter a guess to win {this.channelDetails.pointsManager.CurrencyPlural}");
                    }
                }

                if (this.channelDetails.gambleConfiguration != null && this.channelDetails.gambleConfiguration.gambleEnabled)
                {
                    cmd = this.commands.Find(x => x.commandType == Constants.CommandType.Gamble);
                    if (cmd != null)
                    {
                        var str = $"\"{cmd.commandText} [points]\" - gamble between {this.channelDetails.gambleConfiguration.minBid} " +
                                  $"and {this.channelDetails.gambleConfiguration.maxBid} {this.channelDetails.pointsManager.CurrencyPlural}. ";

                        if (this.channelDetails.gambleConfiguration.minMinutesBetweenGambles == 0)
                        {
                            str += $"{gambleTimeoutSecondsDefault} sec timeout.";
                        }
                        else
                        {
                            str += $"{this.channelDetails.gambleConfiguration.minMinutesBetweenGambles} min timeout.";
                        }
                            

                        strs.Add(str);
                    }
                }

                if (this.channelDetails.hypeCommands.Any())
                {
                    foreach (var a in this.channelDetails.hypeCommands)
                    {
                        var str = "";
                        str += a.commandText;
                        if (a.pointsCost > 0)
                        {
                            //var curString = (a.pointsCost == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural);
                            str += " - Costs " + a.pointsCost;// + " " + curString + ". ";
                        }
                        strs.Add(str);
                    }
                }

                this._helpString = string.Join(" | ", strs.ToArray());
            }
        }

        public void ConfigureCeres(CeresConfiguration newSettings)
        {
            lock (this.commands)
            {
                this.channelDetails.ceresConfiguration = newSettings;

                // Complete reset:
                this.commands.RemoveAll(x => x.commandType == Constants.CommandType.StartCeres ||
                    x.commandType == Constants.CommandType.StartCeres ||
                    x.commandType == Constants.CommandType.EndCeres ||
                    x.commandType == Constants.CommandType.CancelCeres ||
                    x.commandType == Constants.CommandType.Guess);

                if (this.channelDetails.ceresConfiguration != null && this.channelDetails.ceresConfiguration.ceresEnabled)
                {
                    // only use the one ceres object. If the user turns it off and then on again we want to keep this round going
                    if (this.ceres == null)
                    {
                        this.ceres = new CeresGuessingGame(
                            logger,
                            channelDetails.channelName,
                            newSettings.numberOfSecondsToGuess,
                            () =>
                            {
                                // On begin guessing
                                lock (this.commands)
                                {
                                    this.sendMessage($"Ceres round started. Type {(this.commands.FirstOrDefault(x => x.commandType == Constants.CommandType.Guess).commandText)} xxxx to register your Ceres time guess. You have {this.channelDetails.ceresConfiguration.numberOfSecondsToGuess} seconds to place your guess.");
                                }
                            },

                            () =>
                            {
                                // On round canceled
                                this.sendMessage("Ceres round cancelled.");
                            },

                            (endTime, guesses) =>
                            {
                                // On round finished

                                var staticWinners = new List<Tuple<CeresGuess, int, string>>();
                                foreach (var a in guesses)
                                {
                                    string closeness;
                                    var points = this.channelDetails.ceresConfiguration.GetStaticPointsAwarded(endTime, a.guess, out closeness);
                                    if (points != null && points > 0)
                                    {
                                        staticWinners.Add(new Tuple<CeresGuess, int, string>(a, points.Value, closeness));
                                    }
                                }

                                var anyWinners = staticWinners.Any();
                                var rankWinners = new List<Tuple<CeresGuess, int, string>>();
                                if (this.channelDetails.ceresConfiguration.closestRewards.Any())
                                {
                                    foreach (var a in this.channelDetails.ceresConfiguration.closestRewards)
                                    {
                                        if (anyWinners && !a.awardEvenIfOtherWinners) { continue; }

                                        var newWinners = guesses.Where(x => x.rank == a.rankAwarded).ToList();
                                        foreach (var g in newWinners)
                                        {
                                            var str = g.rank.ToString();
                                            switch (g.rank)
                                            {
                                                case 1:
                                                    str += "st";
                                                    break;
                                                case 2:
                                                    str += "nd";
                                                    break;
                                                case 3:
                                                    str += "rd";
                                                    break;
                                                default:
                                                    str += "th";
                                                    break;
                                            }


                                            rankWinners.Add(new Tuple<CeresGuess, int, string>(g, a.pointsAwarded, str));
                                        }
                                    }
                                }

                                var anybodyWon = false;
                                var messages = new List<string>();

                                foreach (var a in this.channelDetails.ceresConfiguration.magicTimes)
                                {
                                    if (a.ceresTime == endTime)
                                    {
                                        var curString = (a.pointsAwarded == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural);

                                        var timeStr = endTime.ToString().Insert(2, ".");

                                        if (!guesses.Any())
                                        {
                                            messages.Add($"Nobody guessed. You all missed out on {a.pointsAwarded} {curString} from a Ceres time of {timeStr}! :(");
                                            break;
                                        }

                                        if (guesses.Count > 4)
                                        {
                                            messages.Add($"Awarding {a.pointsAwarded} {curString} to {guesses.Count} beautiful people for a Ceres time of {timeStr}!");
                                        }
                                        else
                                        {
                                            messages.Add($"Awarding {a.pointsAwarded} {curString} to {(string.Join(", ", guesses.Select(x => x.userID).ToArray()))} for a Ceres time of {timeStr}!");
                                        }

                                        foreach (var g in guesses)
                                        {
                                            anybodyWon = true;
                                            this.channelDetails.pointsManager.GivePlayerPoints(g.userID, a.pointsAwarded, out long? newPoints);
                                        }

                                        break;
                                    }
                                }

                                foreach (var a in staticWinners)
                                {
                                    anybodyWon = true;

                                    long? newPoints;

                                    this.channelDetails.pointsManager.GivePlayerPoints(a.Item1.userID, a.Item2, out newPoints);

                                    var curString = (a.Item2 == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural);

                                    messages.Add(
                                        $"{a.Item1.userID} guessed {a.Item3}, and wins {a.Item2} {curString}{(newPoints.HasValue ? ($" ({newPoints} total)") : "")}!"
                                    );
                                }

                                foreach (var a in rankWinners)
                                {
                                    anybodyWon = true;

                                    long? newPoints;

                                    this.channelDetails.pointsManager.GivePlayerPoints(a.Item1.userID, a.Item2, out newPoints);

                                    var curString = (a.Item2 == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural);

                                    messages.Add(
                                        $"{a.Item1.userID} came in {a.Item3}, and wins {a.Item2} {curString}{(newPoints.HasValue ? ($" ({newPoints} total)") : "")}!"
                                    );
                                }

                                if (!anybodyWon)
                                {
                                    messages.Add("Ceres round ended. Nobody won. :(");
                                }


                                this.SendMessagesTogether(messages);
                            },

                            () =>
                            {
                            // On guessing time finished
                            this.sendMessage($"Ceres time guessing has ended. Good luck!");
                            });
                    }

                    this.commands.Add(new Command(
                        Constants.CommandType.StartCeres,
                        new Action<ChatMessage, Command>((message, command) =>
                        {
                            this.ceres.beginGuessing();
                        }),
                        Constants.AccessLevel.Moderator,
                        false,
                        true,
                        "!startceres",
                        false,
                        5,
                        null
                    ));

                    this.commands.Add(new Command(
                        Constants.CommandType.EndCeres,
                        new Action<ChatMessage, Command>((message, command) =>
                        {
                            string endtime = new string(message.Message.Where(Char.IsDigit).ToArray()); // linq magic to extract any leading/trailing chars

                        if (endtime.Length != 4)
                            {
                                this.logger.LogWarning("Invalid endtime (" + endtime + ")", true);
                                return;
                            }

                            var time = int.Parse(endtime);

                            this.ceres.completeGuessingGame(time);
                        }),
                        Constants.AccessLevel.Moderator,
                        true,
                        false,
                        "!endceres",
                        true, // does have a param
                        5,
                        null
                    ));

                    this.commands.Add(new Command(
                        Constants.CommandType.CancelCeres,
                        new Action<ChatMessage, Command>((message, command) =>
                        {
                            this.ceres.cancelGuessing();
                        }),
                        Constants.AccessLevel.Moderator,
                        true,
                        false,
                        "!cancelceres",
                        false,
                        5,
                        null
                    ));


                    this.commands.Add(new Command(
                        Constants.CommandType.Guess,
                        new Action<ChatMessage, Command>((message, command) =>
                        {
                            string guess = new string(message.Message.Where(Char.IsDigit).ToArray()); // linq magic to extract any leading/trailing chars

                            if (guess.Length != 4)
                            {
                                lock (this.commands)
                                {
                                    sendMessage($"I'm not sure what guess you meant, @{message.Username} . Please enter a new guess with {(this.commands.FirstOrDefault(x => x.commandType == Constants.CommandType.Guess).commandText)} xxxx");
                                }
                                return;
                            }

                            var time = int.Parse(guess);

                            this.ceres.makeGuess(message.Username, time);
                        }),
                        Constants.AccessLevel.Public,
                        true,
                        false,
                        "!guess",
                        true, // does have parameters
                        null,
                        null
                    ));
                }
                else if (this.ceres != null)
                {
                    this.ceres.shutdown();
                    this.ceres = null;
                }

                updateHelpString();
            }
        }

        public void ConfigureGamble(GambleConfiguration newSettings)
        {
            lock (this.commands)
            {
                this.channelDetails.gambleConfiguration = newSettings;

                // complete reset:
                this.commands.RemoveAll(x => x.commandType == Constants.CommandType.Gamble);

                if (this.channelDetails.gambleConfiguration != null && this.channelDetails.gambleConfiguration.gambleEnabled)
                {
                    this.commands.Add(new Command(
                        Constants.CommandType.Gamble,
                        new Action<ChatMessage, Command>((message, command) =>
                        {
                            this.Gamble(message);
                        }),
                        Constants.AccessLevel.Public,
                        false,
                        false,
                        this.channelDetails.gambleConfiguration.gambleCommand,
                        true, // has parameters
                        null,
                        3 // timeout is managed in the command, so use a really low one here to prevent spamming
                    ));
                }

                updateHelpString();
            }
        }

        public void ConfigureHype(List<HypeCommand> newSettings)
        {
            lock (this.commands)
            {
                if (newSettings == null) { newSettings = new List<HypeCommand>(); }

                this.channelDetails.hypeCommands = newSettings;

                // Complete reset:

                this.commands.RemoveAll(x => x.commandType == Constants.CommandType.Hype);

                foreach (var cmd in this.channelDetails.hypeCommands)
                {
                    if (!cmd.enabled) { continue; }

                    this.commands.Add(new Command(
                        Constants.CommandType.Hype,
                        new Action<ChatMessage, Command>((message, command) =>
                        {
                            var hCommand = this.channelDetails.hypeCommands.Find(x => x.commandText == command.commandText);

                            if (hCommand.pointsCost != 0)
                            {
                                var curPoints = this.channelDetails.pointsManager.GetCurrentPlayerPoints(message.Username);
                                if (curPoints < hCommand.pointsCost)
                                {
                                    sendMessage($"You have only {curPoints} {(curPoints == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural)}, @{message.Username} . {hCommand.pointsCost} {(hCommand.pointsCost == 1 ? (this.channelDetails.pointsManager.CurrencySingular + " is") : (this.channelDetails.pointsManager.CurrencyPlural + " are"))} required for that command.");
                                    return;
                                }
                            }

                            var res = new List<string>();

                            var timesToRepeat = hCommand.numberOfResponses;
                            var availableTextStrings = hCommand.commandResponses.Select(x => x).ToList(); // make a copy of the text strings

                        while (timesToRepeat > 0 && availableTextStrings.Any())
                            {
                                timesToRepeat--;
                                var idx = 0;
                                if (hCommand.randomizeResponseOrders)
                                {
                                    idx = rnd.Next(availableTextStrings.Count);
                                }

                                var text = availableTextStrings[idx];
                                availableTextStrings.RemoveAt(idx);
                                res.Add(text.message);
                            }

                            foreach (var a in res)
                            {
                                this.sendMessage(a);
                            }

                        // Deduct after, in case of exception
                        if (hCommand.pointsCost != 0)
                            {
                                long? newPoints;
                                this.channelDetails.pointsManager.GivePlayerPoints(message.Username, (-1 * hCommand.pointsCost), out newPoints);
                            }
                        }),
                        (Constants.AccessLevel)cmd.accessLevel,
                        false,
                        false,
                        cmd.commandText,
                        false,
                        null,
                        30));
                }

                updateHelpString();
            }
        }

        private List<Command> commands = new List<Command>();
        private Command GetCommand(string messageText)
        {
            lock (commands)
            {

                foreach (var command in commands)
                {
                    if (command.hasParameters)
                    {
                        if (command.commandText != null && messageText.ToLower().StartsWith(command.commandText.ToLower()))
                        {
                            return command;
                        }
                    }
                    else
                    {
                        if (command.commandText != null && messageText.ToLower().Trim() == command.commandText.ToLower().Trim())
                        {
                            return command;
                        }
                    }
                }

                if (this.channelDetails.ceresConfiguration != null && this.channelDetails.ceresConfiguration.ceresEnabled)
                {
                    // special command for !xxxx guesses
                    if (messageText.Length == 5 && messageText.StartsWith("!"))
                    {
                        var str = messageText.Remove(0, 1);
                        if (str.Length == 4 && !str.Any(x => !Char.IsDigit(x)))
                        {
                            var guessCmd = commands.Find(x => x.commandType == Constants.CommandType.Guess);
                            return guessCmd;
                        }
                    }
                }

                return null;
            }
        }

        public void connect()
        {

            ConnectionCredentials credentials = new ConnectionCredentials(this.ChatBotTwitchUsername, this.ChatBotTwitchOauthToken);
            twitchClient = new TwitchClient(credentials, this.channelDetails.channelName, '!', '!', false, null, true);
            twitchClient.Logging = false;
            twitchClient.ChatThrottler = new TwitchLib.Services.MessageThrottler(twitchClient, numberOfMessagesAllowed, TimeSpan.FromSeconds(secondsThrottled));
            twitchClient.OnMessageReceived += new EventHandler<OnMessageReceivedArgs>(globalChatMessageReceived);
            //cl.OnWhisperReceived += new EventHandler<OnWhisperReceivedArgs>(komarusSecretCommand);
            twitchClient.OnConnected += new EventHandler<OnConnectedArgs>(onConnected);
            twitchClient.OnConnectionError += Cl_OnConnectionError;
            twitchClient.OnIncorrectLogin += Cl_OnIncorrectLogin;
            twitchClient.OnDisconnected += Cl_OnDisconnected;
            this.logger.LogInformation($"Connecting to {this.channelDetails.channelName}...");
            twitchClient.Connect();
            twitchClient.ChatThrottler.StartQueue();
        }

        public void disconnect()
        {
            this.doNotReconnect = true;
            this.ceres?.shutdown();
            this.ceres = null;
            this.twitchClient.Disconnect();
            this.twitchClient = null;
        }

        private int reconnectTryCt = 0;
        private int maxRetryCt = 10;
        private void Cl_OnDisconnected(object sender, OnDisconnectedArgs e)
        {

            try
            {
                reconnectTryCt++;
                if (reconnectTryCt >= maxRetryCt)
                {
                    doNotReconnect = true;
                }

                if (doNotReconnect)
                {
                    this.logger.LogWarning("Chat Disconnected. Not attempting to reconnect.", true);
                }
                else
                {
                    new Thread(() =>
                    {
                        try
                        {
                            this.logger.LogWarning($"Chat Disconnected. Trying to reconnect (attempt {reconnectTryCt}) in 15 seconds... ");
                            Thread.Sleep(15000);
                            this.logger.LogWarning($"Attempt {reconnectTryCt} Connecting...");
                            connect();
                        }
                        catch (Exception exc)
                        {
                            this.logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name} (ReconnectThread Exception)"));
                        }
                    }).Start();
                }
            }
            catch (Exception exc)
            {
                this.logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
            }
        }

        private bool doNotReconnect = false;
        private void Cl_OnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
            try
            {
                doNotReconnect = true;
                this.logger.LogWarning($"Incorrect Login Exception Occurred.");
            }
            catch (Exception exc)
            {
                this.logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
            }
        }

        private void Cl_OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            try
            {
                this.logger.LogError("An Exception Occurred Connecting!  Perhaps restarting could help? Error username: {e.BotUsername} | Error message: {e.Error}");
            }
            catch (Exception exc)
            {
                this.logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
            }
        }


        private void onConnected(object sender, OnConnectedArgs e)
        {
            try
            {
                this.twitchClient.JoinChannel(this.channelDetails.channelName);
                reconnectTryCt = 0;

                this.logger.LogInformation("Connected! Channel: #" + channelDetails.channelName);
            }
            catch (Exception exc)
            {
                this.logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
            }
        }

        private void globalChatMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            try
            {
                var command = GetCommand(e.ChatMessage.Message);

                if ((command == null) ||
                    (this.ceres != null && command.requiredRoundStarted && !this.ceres.RoundRunning) ||
                    (this.ceres != null && command.requiredRoundNotStarted && this.ceres.RoundRunning)
                    )
                {
                    return;
                }

                var callerAccessLevel = Constants.AccessLevel.Public;
                if (e.ChatMessage.IsModerator)
                {
                    callerAccessLevel = Constants.AccessLevel.Moderator;
                }
                if (e.ChatMessage.IsBroadcaster)
                {
                    callerAccessLevel = Constants.AccessLevel.Broadcaster;
                }

                if (command.requiredAccessLevel > callerAccessLevel)
                {
                    return;
                }

                if (command.globalTimeout.HasValue)
                {
                    var key = command.commandText + "_" + command.commandType.ToString();

                    lock (globalTimeouts)
                    {
                        DateTime allowedAgain;
                        if (globalTimeouts.TryGetValue(key, out allowedAgain))
                        {
                            if (DateTime.Now < allowedAgain)
                            {
                                // We can't use this command yet because the currenttime is less than the allowed time
                                return;
                            }

                            globalTimeouts.Remove(key);
                        }
                        globalTimeouts.Add(key, DateTime.Now.AddSeconds(command.globalTimeout.Value));
                    }
                }

                if (command.userTimeout.HasValue)
                {
                    var key = command.commandText + "_" + command.commandType.ToString() + "_" + e.ChatMessage.Username;

                    lock (userTimeouts)
                    {
                        DateTime allowedAgain;
                        if (userTimeouts.TryGetValue(key, out allowedAgain))
                        {
                            if (DateTime.Now < allowedAgain)
                            {
                                // We can't use this command yet because the currenttime is less than the allowed time
                                return;
                            }

                            userTimeouts.Remove(key);
                        }
                        userTimeouts.Add(key, DateTime.Now.AddSeconds(command.userTimeout.Value));
                    }
                }

                command.onRun(e.ChatMessage, command);
            }
            catch (Exception exc)
            {
                this.logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
            }
        }

        private Dictionary<string, DateTime> globalTimeouts = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> userTimeouts = new Dictionary<string, DateTime>();

        public void cleanupTimeouts()
        {
            lock (globalTimeouts)
            {
                var toRemove = new List<string>();
                foreach (var a in globalTimeouts)
                {
                    if (a.Value < DateTime.Now)
                    {
                        toRemove.Add(a.Key);
                    }
                }

                foreach (var a in toRemove)
                {
                    globalTimeouts.Remove(a);
                }
            }

            lock (userTimeouts)
            {
                var toRemove = new List<string>();
                foreach (var a in userTimeouts)
                {
                    if (a.Value < DateTime.Now)
                    {
                        toRemove.Add(a.Key);
                    }
                }

                foreach (var a in toRemove)
                {
                    userTimeouts.Remove(a);
                }
            }
        }


        private static object sendLock = new object();
        public void sendMessage(string message)
        {
            lock (sendLock)
            {
                twitchClient.SendMessage(this.channelDetails.channelName, message);
            }
        }

        private void SendMessagesTogether(List<string> messages)
        {
            var msg = "";
            int maxMsgLength = 450;
            foreach (var message in messages)
            {
                if ((msg + message).Length > maxMsgLength)
                {
                    sendMessage(msg);
                    msg = message + " ";
                    Thread.Sleep(500);
                }
                else
                {
                    msg += message + " ";
                }
            }

            sendMessage(msg);
        }

        private Dictionary<string, DateTime> userLastGambles = new Dictionary<string, DateTime>();
        public void Gamble(ChatMessage c)
        {
            if (this.channelDetails.gambleConfiguration == null || this.channelDetails.gambleConfiguration.gambleEnabled == false)
            {
                return;
            }

            string gambleAmountStr = new string(c.Message.Where(Char.IsDigit).ToArray());

            lock (userLastGambles)
            {
                DateTime lastGamble;
                if (userLastGambles.TryGetValue(c.Username, out lastGamble))
                {
                    DateTime lastGambleMustBeBeforeThisDate;
                    if (this.channelDetails.gambleConfiguration.minMinutesBetweenGambles == 0)
                    {
                        lastGambleMustBeBeforeThisDate = DateTime.Now.AddSeconds(-1 * gambleTimeoutSecondsDefault);
                    }
                    else
                    {
                        lastGambleMustBeBeforeThisDate = DateTime.Now.AddMinutes(this.channelDetails.gambleConfiguration.minMinutesBetweenGambles * -1);
                    }


                    if (lastGamble > lastGambleMustBeBeforeThisDate)
                    {
                        var timespan = (lastGamble - lastGambleMustBeBeforeThisDate);

                        var timeString = timespan.Minutes + " minutes";
                        if (timespan.Minutes == 1) { timeString = timespan.Minutes + " minute"; }

                        if (timespan.Minutes < 1)
                        {
                            timeString = timespan.Seconds + " seconds";
                            if (timespan.Seconds == 1) { timeString = timespan.Seconds + " second"; }
                        }

                        var timeoutStr = $"{this.channelDetails.gambleConfiguration.minMinutesBetweenGambles} {(this.channelDetails.gambleConfiguration.minMinutesBetweenGambles == 1 ? "minute" : "minutes")}";
                        if (this.channelDetails.gambleConfiguration.minMinutesBetweenGambles == 0)
                        {
                            timeoutStr = $"{gambleTimeoutSecondsDefault} seconds";
                        }
                        sendMessage($"@{c.Username}, you can only gamble once every {timeoutStr}. Please wait another {timeString}.");
                        return;
                    }
                }
            }


            int gambleAmount;
            if (!int.TryParse(gambleAmountStr, out gambleAmount))
            {
                lock (this.commands)
                {
                    sendMessage($"I'm not sure what guess you meant, @{c.Username} . Please enter a new gamble with {(this.commands.Find(x => x.commandType == Constants.CommandType.Gamble).commandText)} [amount]");
                }
                return;
            }

            if (gambleAmount < this.channelDetails.gambleConfiguration.minBid)
            {
                sendMessage($"@{c.Username}, You must gamble at least {this.channelDetails.gambleConfiguration.minBid} {this.channelDetails.pointsManager.CurrencySingular}");
                return;
            }

            if (gambleAmount > this.channelDetails.gambleConfiguration.maxBid)
            {
                sendMessage($"@{c.Username}, You cannot gamble more than {this.channelDetails.gambleConfiguration.maxBid} {this.channelDetails.pointsManager.CurrencyPlural}");
                return;
            }

            if (gambleAmount <= 0)
            {
                sendMessage($"@{c.Username}, You cannot gamble less than 1 {this.channelDetails.pointsManager.CurrencySingular}");
                return;
            }

            var curPoints = this.channelDetails.pointsManager.GetCurrentPlayerPoints(c.Username);
            if (curPoints < gambleAmount)
            {
                sendMessage($"You have only {curPoints} {(curPoints == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural)}, @{c.Username} .");
                return;
            }

            lock (userLastGambles)
            {
                if (userLastGambles.ContainsKey(c.Username))
                {
                    userLastGambles[c.Username] = DateTime.Now;
                }
                else
                {
                    userLastGambles.Add(c.Username, DateTime.Now);
                }
            }

            var roll = rnd.Next(1, 101);
            var multiplier = this.channelDetails.gambleConfiguration.rollResults.FirstOrDefault(x => x.roll == roll);

            if (multiplier == null || multiplier.multiplier == 1)
            {
                sendMessage($"{c.Username} rolled {roll}. No {this.channelDetails.pointsManager.CurrencyPlural} won or lost.");
                return;
            }
            else if (multiplier.multiplier < 1)
            {
                var amountLost = ((int)Math.Round((1 - multiplier.multiplier) * gambleAmount));

                long? newPoints;
                this.channelDetails.pointsManager.GivePlayerPoints(c.Username, (amountLost * -1), out newPoints);

                sendMessage($"{c.Username} rolled {roll} and lost {amountLost} {(amountLost == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural)}{(newPoints.HasValue ? ($" ({newPoints} total)") : "")}.");
                return;
            }
            else if (multiplier.multiplier > 1)
            {
                var amountGained = ((int)Math.Round(multiplier.multiplier * gambleAmount));

                long? newPoints;
                this.channelDetails.pointsManager.GivePlayerPoints(c.Username, amountGained, out newPoints);

                sendMessage($"{c.Username} rolled a {roll} and won {amountGained} {(amountGained == 1 ? this.channelDetails.pointsManager.CurrencySingular : this.channelDetails.pointsManager.CurrencyPlural)}{(newPoints.HasValue ? ($" ({newPoints} total)") : "")}!");
                return;
            }
        }
    }
}
