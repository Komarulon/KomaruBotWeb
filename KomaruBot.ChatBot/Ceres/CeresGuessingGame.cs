using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using KomaruBot.Common;
using KomaruBot.Common.Extensions;

namespace KomaruBot.ChatBot.Ceres
{
    public class CeresGuessingGame
    {
        public CeresGuessingGame(
            ILogger logger,
            string channelName,
            int numberOfSecondsToGuess,
            Action onGameStarted,
            Action onGameCanceled,
            Action<int, List<CeresGuess>> onGameCompleted,
            Action onGuessingTimeFinished
            )
        {
            this.RoundRunning = false;
            this.GuessesAllowed = false;
            this.guesses = new List<CeresGuess>();
            this.logger = logger;
            this.channelName = channelName;
            this.secondsToGuess = numberOfSecondsToGuess;

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
        public int secondsToGuess { get; private set; }

        // This is the "extra" time where guesses are allowed but it is not shown as such
        public static int secondsToGuessSecretExtra = 5;

        private Action onGameStarted;
        public void beginGuessing()
        {
            lock (this)
            {
                if (!this.RoundRunning)
                {
                    logger.LogInformation($"{channelName} | Ceres round started.");
                    this.RoundRunning = true;
                    this.GuessesAllowed = true;
                    this.setRoundEndTimer();
                    this.guesses = new List<CeresGuess>();

                    if (onGameStarted != null)
                    {
                        onGameStarted();
                    }
                }
            }
        }

        private Action onGameCanceled;
        public void cancelGuessing()
        {
            lock (this)
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
            lock (this)
            {
                this.RoundRunning = false;
                this.GuessesAllowed = false;
                this.cancelRoundEndTimer();
                this.cancelRoundRealEndTimer();
            }
        }

        private Action<int, List<CeresGuess>> onGameCompleted;

        /// <summary>
        /// endTime should be something like 4600
        /// </summary>
        /// <param name="endTime"></param>
        public void completeGuessingGame(int endTime)
        {
            lock (this)
            {
                logger.LogInformation($"{channelName} | Ceres round ended with time {endTime}");

                if (this.RoundRunning)
                {
                    this.RoundRunning = false;
                    this.GuessesAllowed = false;
                    this.cancelRoundEndTimer();
                    this.cancelRoundRealEndTimer();

                    if (onGameCompleted != null)
                    {
                        guesses.Rank((guess) =>
                        {
                            return Math.Abs(endTime - guess.guess);
                        },
                        (guess, rank, idx) =>
                        {
                            guess.rank = rank;
                        });

                        onGameCompleted(endTime, this.guesses);
                    }
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
