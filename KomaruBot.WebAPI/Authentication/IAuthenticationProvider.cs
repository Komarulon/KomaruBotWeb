using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Authentication
{
    public interface IAuthenticationProvider
    {
        Models.AuthenticationResult Authenticate(string authorizationHeader);
        Models.AuthenticationResult Authenticate(HttpContext context);
    }
}
