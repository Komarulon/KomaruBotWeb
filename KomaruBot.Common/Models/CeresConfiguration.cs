using System;
using System.Collections.Generic;
using System.Text;

namespace KomaruBot.Common.Models
{
    public class CeresConfiguration
    { 
        public bool ceresEnabled { get; set; }

        /// <summary>
        /// "Magic time" where when this is the time, all guessers receive a set amount of points
        /// <para>For example getting a 4600 could give everyone 1000 points</para>
        /// </summary>
        public class MagicTime
        {  
            public int ceresTime { get; set; }
            public int pointsAwarded { get; set; }
        }

        public int numberOfSecondsToGuess { get; set; }

        public List<MagicTime> magicTimes { get; set; }

        public List<StaticReward> staticRewards { get; set; }
        public class StaticReward
        {
            public StaticReward() { }

            public int hundrethsLeewayStart { get; set; }
            public int hundrethsLeewayEnd { get; set; }
            public int pointsAwarded { get; set; }
            public StaticReward(
                int hundrethsLeewayStart,
                int hundrethsLeewayEnd,
                int pointsAwarded)
            {
                this.hundrethsLeewayStart = hundrethsLeewayStart;
                this.hundrethsLeewayEnd = hundrethsLeewayEnd;
                this.pointsAwarded = pointsAwarded;
            }
        }

        public List<ClosestReward> closestRewards { get; set; }
        public class ClosestReward
        {
            public ClosestReward() { }

            public ClosestReward(
                int rankAwarded,
                int pointsAwarded,
                bool awardEvenIfOtherWinners
                )
            {
                this.rankAwarded = rankAwarded;
                this.pointsAwarded = pointsAwarded;
                this.awardEvenIfOtherWinners = awardEvenIfOtherWinners;
            }

            public int rankAwarded { get; set; }
            public int pointsAwarded { get; set; }
            public bool awardEvenIfOtherWinners { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actualTime"></param>
        /// <param name="guessedTime"></param>
        /// <param name="closenessString">will be a string like "exactly", "within +/- 0.02", </param>
        /// <returns></returns>
        public int? GetStaticPointsAwarded(int actualTime, int guessedTime, out string closenessString)
        {
            closenessString = "";
            foreach (var rewardClass in this.staticRewards)
            {
                var difference = Math.Abs(actualTime - guessedTime);
                if (difference >= rewardClass.hundrethsLeewayStart && difference <= rewardClass.hundrethsLeewayEnd)
                {
                    if (rewardClass.hundrethsLeewayEnd == 0 && rewardClass.hundrethsLeewayStart == 0)
                    {
                        closenessString = "exactly";
                    }
                    else //if (rewardClass.hundrethsLeewayStart == 0 && rewardClass.hundrethsLeewayEnd != 0)
                    {
                        var hundrethsString = "";
                        if (rewardClass.hundrethsLeewayEnd <= 9)
                        {
                            hundrethsString = "0" + (rewardClass.hundrethsLeewayEnd.ToString());
                        }
                        else
                        {
                            hundrethsString = (rewardClass.hundrethsLeewayEnd.ToString());
                        }

                        closenessString = $"within +/- 0.{hundrethsString}";
                    }
                    return rewardClass.pointsAwarded;
                }
            }

            return null;
        }
    }
}
