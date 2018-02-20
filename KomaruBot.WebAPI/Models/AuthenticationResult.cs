using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Models
{
    public class AuthenticationResult
    {
        public string GetUserID()
        {
            return this.Details?.token?.user_name;
        }

        public string GetClientID()
        {
            return this.Details?.token?.client_id;
        }

        public string Message { get; set; }
        public Constants.AuthenticationResult Result { get; set; }
        public string ResultString { get { return Result.ToString(); } }
        public TwitchAuthenticationResult Details { get; set; }
        public class TwitchAuthenticationResult
        {
            public bool identified { get; set; }
            public Token token { get; set; }
            public class Token
            {
                public bool valid { get; set; }
                public string user_name { get; set; }
                public string client_id { get; set; }
            }
        }
    }
}

