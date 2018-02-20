using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KomaruBot.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KomaruBot.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private AppSettings appSettings { get; set; }
        private Authentication.IAuthenticationProvider authenticationProvider { get; set; }
        private ILogger<AuthController> _logger { get; set; }

        public AuthController(
            IOptions<AppSettings> settings,
            Authentication.IAuthenticationProvider authenticationProvider,
            ILogger<AuthController> logger
            )
        {
            this.appSettings = settings.Value;
            this.authenticationProvider = authenticationProvider;
            this._logger = logger;
        }

        [HttpGet("authendpoint")]
        public ActionResult GetAuthEndpoint()
        {
            return Json(new
            {
                AuthEndpoint = (this.appSettings.TwitchOauthEndpoint +
                    "?client_id=" + WebUtility.UrlEncode(this.appSettings.TwitchClientID) +
                    "&redirect_uri=" +
                        WebUtility.UrlEncode(
                            this.appSettings.ClientUrl + this.appSettings.ClientAuthRedirectRoute
                        )
                        +
                    "&response_type=token" +
                    "&scope=" + this.appSettings.TwitchScopes)
            });
        }

        
        [HttpGet]
        public ActionResult CheckLogin()
        {
            var status = this.authenticationProvider.Authenticate(this.HttpContext);
            return Json(status);
        }

    }
}
