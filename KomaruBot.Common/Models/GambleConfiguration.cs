using System;
using System.Collections.Generic;
using System.Text;

namespace KomaruBot.Common.Models
{
    public class GambleConfiguration
    {
        public int minBid { get; set; }
        public int maxBid { get; set; }
        public bool gambleEnabled { get; set; }
        public int minMinutesBetweenGambles { get; set; }
        public List<GambleRewards> rollResults { get; set; }
    }

    public class GambleRewards
    {
        public int roll { get; set; }
        public decimal multiplier { get; set; }
    }
}
