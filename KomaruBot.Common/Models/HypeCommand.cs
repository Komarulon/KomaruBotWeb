using System;
using System.Collections.Generic;
using System.Text;

namespace KomaruBot.Common.Models
{
    public class HypeCommand
    {
        public bool enabled { get; set; }
        public int pointsCost { get; set; }
        public int accessLevel { get; set; }
        public string commandText { get; set; }
        public List<string> commandResponses { get; set; }
        public bool randomizeResponseOrders { get; set; }
        public int numberOfResponses { get; set; }
    }
}
