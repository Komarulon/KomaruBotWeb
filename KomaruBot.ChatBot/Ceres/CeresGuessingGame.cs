using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace KomaruBot.ChatBot.Ceres
{
    public class CeresGuessingGame
    {
        public CeresGuessingGame(
            ILogger logger,
            string channelName,
            Action onGameStarted,
            Action onGameCanceled,
            Action onGameCompleted,
            Action onGuessingTimeFinished
            )
        {
            this.RoundRunning = false;
            this.GuessesAllowed = false;
            this.guesses = new List<CeresGuess>();
            this.logger = logger;
            this.channelName = channelName;

            this.onGameStarted = onGameStarted;
            this.onGameCanceled = onGameCanceled;
            this.onGameCompleted = onGameCompleted;
            this.onGuessingTimeFinished = onGuessingTimeFinished;
        }

        public bool RoundRunning { get; private set; }
        public bool GuessesAllowed { get; private set; }
        public List<CeresGuess> guesses { get; set; }
        private ILogger logger;
        private string channelName;

        // So there's the time allowed to guess. 
        // After this time, there's a hidden range we'll allow more times to guess
        // and still accept them
        public static int secondsToGuess = 45;

        // This is the "extra" time where guesses are allowed but it is not shown as such
        public static int secondsToGuessSecretExtra = 5;

        private Action onGameStarted;
        public void beginGuessing()
        {
            if (!this.RoundRunning)
            {
                logger.LogInformation($"{channelName} | Ceres round started.");
                this.RoundRunning = true;
                this.GuessesAllowed = true;
                this.setRoundEndTimer();
                lock (this) { this.guesses = new List<CeresGuess>(); }

                if (onGameStarted != null)
                {
                    onGameStarted();
                }
            }
        }

        private Action onGameCanceled;
        public void cancelGuessing()
        {
            if (this.RoundRunning)
            {
                logger.LogInformation($"{channelName} | Ceres round canceled.");
                this.RoundRunning = false;
                this.GuessesAllowed = false;
                this.cancelRoundEndTimer();
                this.cancelRoundRealEndTimer();

                if (this.onGameCanceled != null)
                {
                    onGameCanceled();
                }
            }
        }

        public void makeGuess(string userID, int guess)
        {
            lock(this)
            {
                if (this.GuessesAllowed)
                {
                    var existingGuess = this.guesses.Find(x => x.userID.ToLower() == userID.ToLower());
                    if (existingGuess != null)
                    {
                        existingGuess.guess = guess;
                    }
                    else
                    {
                        this.guesses.Add(new CeresGuess
                        {
                            userID = userID,
                            guess = guess,
                        });
                    }
                }
            }
        }

        public void shutdown()
        {
            this.RoundRunning = false;
            this.GuessesAllowed = false;
            this.cancelRoundEndTimer();
            this.cancelRoundRealEndTimer();
        }

        private Action onGameCompleted;

        /// <summary>
        /// endTime should be something like 4600
        /// </summary>
        /// <param name="endTime"></param>
        public void completeGuessingGame(int endTime)
        {
            logger.LogInformation($"{channelName} | Ceres round ended with time {endTime}");

            if (this.RoundRunning)
            {
                this.RoundRunning = false;
                this.GuessesAllowed = false;
                this.cancelRoundEndTimer();
                this.cancelRoundRealEndTimer();

                lock (this)
                {
                    foreach (var a in this.guesses)
                    {
                        //award stuff build up string etc
                    }
                }

                if (onGameCompleted != null)
                {
                    onGameCompleted();
                }
            }
        }

        private Action onGuessingTimeFinished;

        private Timer roundEndTimer = null;
        private void setRoundEndTimer()
        {
            lock (this)
            {
                roundEndTimer = new Timer((state) =>
                {
                    lock (this)
                    {
                        if (onGuessingTimeFinished != null)
                        {
                            onGuessingTimeFinished();
                        }

                        //sendMessage($"Guessing for round #{round_id} has ended. Good luck!");
                        logger.LogInformation($"{channelName} | Ceres Guesses ending warning message sent. ({secondsToGuess} has passed, but an additional {secondsToGuessSecretExtra} is allowed for guesses).");

                        cancelRoundEndTimer();
                        setRoundRealEndTimer();

                    }
                }, null, secondsToGuess * 1000, Timeout.Infinite);
            }
        }

        private void cancelRoundEndTimer()
        {
            lock (this)
            {
                if (roundEndTimer != null)
                {
                    roundEndTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    roundEndTimer.Dispose();
                    roundEndTimer = null;
                }
            }
        }

        private Timer roundRealEndTimer = null;
        private void setRoundRealEndTimer()
        {
            lock (this)
            {
                roundEndTimer = new Timer((state2) =>
                {
                    this.GuessesAllowed = false;
                    cancelRoundRealEndTimer();
                    logger.LogInformation($"{channelName} | Ceres guessing time is over.");
                }, null, secondsToGuessSecretExtra * 1000, Timeout.Infinite);
            }
        }

        private void cancelRoundRealEndTimer()
        {
            lock (this)
            {
                if (roundRealEndTimer != null)
                {
                    roundRealEndTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    roundRealEndTimer.Dispose();
                    roundRealEndTimer = null;
                }
            }
        }
    }
}
