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

            this.connect();

            this.ceres = new CeresGuessingGame(
                logger,
                channelDetails.channelName,
                () =>
                {
                    // On begin guessing
                    this.sendMessage($"Ceres round started. Type {(this.commands.FirstOrDefault(x => x.commandType == Constants.CommandType.Guess).commandText)} xxxx to register your Ceres time guess. You have {CeresGuessingGame.secondsToGuess} seconds to place your guess.");
                },
                () =>
                {
                    // On round canceled
                    this.sendMessage("Ceres round cancelled.");
                },
                () =>
                {
                    // On round finished
                    this.sendMessage("Ceres round completed or something.");
                },
                () =>
                {
                    // On guessing time finished
                    this.sendMessage($"Ceres time guessing has ended. Good luck!");
                });

            if (this.channelDetails.ceresConfiguration != null && this.channelDetails.ceresConfiguration.ceresEnabled)
            {

                this.commands.Add(new Command(
                    Constants.CommandType.StartCeres,
                    new Action<ChatMessage, Command>((message, command) =>
                    {
                        this.ceres.beginGuessing();
                    }),
                    Constants.AccessLevel.Moderator,
                    false,
                    true,
                    "!startceres"
                ));

                this.commands.Add(new Command(
                    Constants.CommandType.EndCeres,
                    new Action<ChatMessage, Command>((message, command) =>
                    {
                        string endtime = new string(message.Message.Where(Char.IsDigit).ToArray()); // linq magic to extract any leading/trailing chars

                    if (endtime.Length != 4)
                        {
                            this.logger.LogInformation("Invalid endtime (" + endtime + ")", true);
                            return;
                        }

                        var time = int.Parse(endtime);

                        this.ceres.completeGuessingGame(time);
                    }),
                    Constants.AccessLevel.Moderator,
                    true,
                    false,
                    "!endceres"
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
                    "!cancelceres"
                ));


                this.commands.Add(new Command(
                    Constants.CommandType.Guess,
                    new Action<ChatMessage, Command>((message, command) =>
                    {
                        string guess = new string(message.Message.Where(Char.IsDigit).ToArray()); // linq magic to extract any leading/trailing chars

                    if (guess.Length != 4)
                        {
                            sendMessage($"I'm not sure what guess you meant, @{message.Username} . Please enter a new guess with {(this.commands.FirstOrDefault(x => x.commandType == Constants.CommandType.Guess).commandText)} xxxx");
                            return;
                        }

                        var time = int.Parse(guess);

                        this.ceres.makeGuess(message.Username, time);
                    }),
                    Constants.AccessLevel.Public,
                    true,
                    false,
                    "!guess"
                ));
            }

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
                    "!gamble"
                ));
            }

            foreach (var cmd in this.channelDetails.hypeCommands)
            {
                if (!cmd.enabled) { continue; }

                this.commands.Add(new Command(
                    Constants.CommandType.Hype,
                    new Action<ChatMessage, Command>((message, command) =>
                    {
                        var hCommand = this.channelDetails.hypeCommands.FirstOrDefault(x => x.commandText == command.commandText);
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
                            res.Add(text);
                        }

                        foreach (var a in res)
                        {
                            this.sendMessage(a);
                        }
                    }),
                    (Constants.AccessLevel)cmd.accessLevel,
                    false,
                    false,
                    cmd.commandText));
            }
        }

        private List<Command> commands = new List<Command>();
        private Command GetCommand(string messageText)
        {
            foreach (var command in commands)
            {
                if (command.commandText != null &&

                    // TODO: should we use .Trim() to compare here? want to avoid commands clashing like cmd and cmd1
                    messageText.StartsWith(command.commandText))
                {
                    return command;
                }
            }

            // special command for !xxxx guesses
            if (messageText.Length == 5 && messageText.StartsWith("!"))
            {
                var str = messageText.Remove(0, 1);
                if (str.Length == 4 && !str.Any(x => !Char.IsDigit(x)))
                {
                    var guessCmd = commands.FirstOrDefault(x => x.commandType == Constants.CommandType.Guess);
                    return guessCmd;
                }
            }

            return null;
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
            this.ceres.shutdown();
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
                    this.logger.LogInformation("Chat Disconnected. Not attempting to reconnect.", true);
                }
                else
                {
                    new Thread(() =>
                    {
                        try
                        {
                            this.logger.LogInformation($"Chat Disconnected. Trying to reconnect (attempt {reconnectTryCt}) in 15 seconds... ", true);
                            Thread.Sleep(15000);
                            this.logger.LogInformation($"Attempt {reconnectTryCt} Connecting...", true);
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
                this.logger.LogInformation($"Incorrect Login Exception Occurred.");
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
                this.logger.LogInformation("An Exception Occurred Connecting!  Perhaps restarting could help?", true);
                this.logger.LogInformation($"Error username: {e.BotUsername}");
                this.logger.LogInformation($"Error message: {e.Error}");
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
                    (command.requiredRoundStarted && !this.ceres.RoundRunning) ||
                    (command.requiredRoundNotStarted && this.ceres.RoundRunning)
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

                command.onRun(e.ChatMessage, command);
            }
            catch (Exception exc)
            {
                this.logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
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

        private static Random random = new Random();
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
                    var lastGambleMustBeBeforeThisDate = DateTime.Now.AddMinutes(this.channelDetails.gambleConfiguration.minMinutesBetweenGambles * -1);
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

                        sendMessage($"@{c.Username}, you can only gamble once every {this.channelDetails.gambleConfiguration.minMinutesBetweenGambles} {(this.channelDetails.gambleConfiguration.minMinutesBetweenGambles == 1 ? "minute" : "minutes")}. Please wait another {timeString}.");
                        return;
                    }
                }
            }


            int gambleAmount;
            if (!int.TryParse(gambleAmountStr, out gambleAmount))
            {
                sendMessage($"I'm not sure what guess you meant, @{c.Username} . Please enter a new gamble with {(this.commands.FirstOrDefault(x => x.commandType == Constants.CommandType.Gamble).commandText)} [amount]");
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

            var roll = random.Next(1, 101);
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
