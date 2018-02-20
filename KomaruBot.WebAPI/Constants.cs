using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI
{
    public class Constants
    {
        public enum AuthenticationResult
        {
            Success,
            Fail,
            MissingHeader,
            MalformedHeader
        }
    }
}
