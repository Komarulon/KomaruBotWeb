using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomaruBot.ChatBot;
using KomaruBot.Common;
using KomaruBot.WebAPI.Helpers;
using KomaruBot.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KomaruBot.WebAPI.Controllers
{
    [Route("api/settings/[controller]")]
    public class CeresController : Controller
    {

        private AppSettings appSettings { get; set; }
        private Authentication.IAuthenticationProvider authenticationProvider { get; set; }
        private UserHelper userHelper { get; set; }
        private ILogger<CeresController> _logger { get; set; }

        public CeresController(
            IOptions<AppSettings> settings,
            Authentication.IAuthenticationProvider authenticationProvider,
            UserHelper userHelper,
            ILogger<CeresController> logger)
        {
            this.appSettings = settings.Value;
            this.authenticationProvider = authenticationProvider;
            this.userHelper = userHelper;
            this._logger = logger;
        }

        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                var userInfo = this.authenticationProvider.Authenticate(this.HttpContext);
                if (userInfo.Result != Constants.AuthenticationResult.Success) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"Could not authenticate"); }
                if (this.appSettings.TwitchClientID != userInfo.GetClientID()) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"{nameof(this.appSettings.TwitchClientID)} does not match"); }

                var settings = this.userHelper.EnsureDefaultCeresSettings(userInfo.GetUserID());

                return Json(settings);
            }
            catch (Exception exc)
            {
                this._logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        public ActionResult Update(
            [FromBody]
            CeresSettings settings
            )
        {
            try
            {
                if (settings == null) { return StatusCode((int)System.Net.HttpStatusCode.BadRequest, $"{nameof(settings)} is null"); }
                if (string.IsNullOrWhiteSpace(settings.userID)) { return StatusCode((int)System.Net.HttpStatusCode.BadRequest, $"{nameof(settings.userID)} is null"); }

                var userInfo = this.authenticationProvider.Authenticate(this.HttpContext);
                if (userInfo.Result != Constants.AuthenticationResult.Success) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"Could not authenticate"); }
                if (this.appSettings.TwitchClientID != userInfo.GetClientID()) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"{nameof(this.appSettings.TwitchClientID)} does not match"); }
                if (settings.userID != userInfo.GetUserID()) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"{nameof(settings.userID)} is not yours"); }

                var validationResult = settings.validate();
                if (validationResult != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                {
                    return StatusCode((int)System.Net.HttpStatusCode.BadRequest, new { message = validationResult.ErrorMessage });
                }

                this.userHelper.SaveSettings(settings);
                Startup.chatBotManager.UpdateConnection(userInfo.GetUserID(), settings.ceresConfiguration);

                return Json(settings);
            }
            catch (Exception exc)
            {
                this._logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
