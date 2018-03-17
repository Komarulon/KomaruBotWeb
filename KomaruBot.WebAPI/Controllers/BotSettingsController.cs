using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KomaruBot.ChatBot;
using KomaruBot.Common;
using KomaruBot.WebAPI.Helpers;
using KomaruBot.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KomaruBot.WebAPI.Controllers
{
    [Route("api/settings/[controller]")]
    public class BotSettingsController : Controller
    {

        private AppSettings appSettings { get; set; }
        private Authentication.IAuthenticationProvider authenticationProvider { get; set; }
        private UserHelper userHelper { get; set; }
        private ILogger<BotSettingsController> _logger { get; set; }

        public BotSettingsController(
            IOptions<AppSettings> settings,
            Authentication.IAuthenticationProvider authenticationProvider,
            UserHelper userHelper,
            ILogger<BotSettingsController> logger)
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

                var settings = this.userHelper.EnsureDefaultUserSettings(userInfo.GetUserID());

                return Json(settings);
            }
            catch (Exception exc)
            {
                this._logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Route("all")]
        [HttpGet]
        public ActionResult GetAll(string auth = null)
        {
            try
            {
                var userInfo = this.authenticationProvider.Authenticate("Bearer " + auth);
                if (userInfo.Result != Constants.AuthenticationResult.Success) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"Could not authenticate"); }
                if (this.appSettings.TwitchClientID != userInfo.GetClientID()) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"{nameof(this.appSettings.TwitchClientID)} does not match"); }

                var botSettings = this.userHelper.EnsureDefaultUserSettings(userInfo.GetUserID());
                var ceresSettings = this.userHelper.EnsureDefaultCeresSettings(userInfo.GetUserID());
                var gambleSettings = this.userHelper.EnsureDefaultGambleSettings(userInfo.GetUserID());
                var hypeSettings = this.userHelper.EnsureDefaultHypeSettings(userInfo.GetUserID());


                var json = JsonConvert.SerializeObject(new
                {
                    botSettings,
                    ceresSettings,
                    gambleSettings,
                    hypeSettings
                });
                var fileName = "KomaruBotBackup" + DateTime.Now.ToString("yyyy-MM-dd") + ".json";
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                var response = File(bytes, "application/json", fileName);
                return response;
            }
            catch (Exception exc)
            {
                this._logger.LogError(ExceptionFormatter.FormatException(exc, $"Exception in {this.GetType().Name} - {System.Reflection.MethodBase.GetCurrentMethod().Name}"));
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Route("allusers")]
        [HttpGet]
        public ActionResult GetAllUsers()
        {
            try
            {
                var userInfo = this.authenticationProvider.Authenticate(this.HttpContext);
                if (userInfo.Result != Constants.AuthenticationResult.Success) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"Could not authenticate"); }
                if (this.appSettings.TwitchClientID != userInfo.GetClientID()) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"{nameof(this.appSettings.TwitchClientID)} does not match"); }

                if (userInfo.GetUserID().ToLower() != "komaru")
                {
                    return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"Not an admin");
                }

                var allUsers = this.userHelper.GetAllUserIDs();

                return Json(new
                {
                    activeUsers = allUsers.Where(x => x.botEnabled).ToList(),
                    inactiveUsers = allUsers.Where(x => !x.botEnabled).ToList(),
                });
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
            UserBotSettings settings
            )
        {
            try
            {
                if (settings == null) { return StatusCode((int)System.Net.HttpStatusCode.BadRequest, $"{nameof(settings)} is null"); }
                if (string.IsNullOrWhiteSpace(settings.userID)) { return StatusCode((int)System.Net.HttpStatusCode.BadRequest, $"{nameof(settings.userID)} is null"); }
                if (string.IsNullOrWhiteSpace(settings.currencyPlural)) { return StatusCode((int)System.Net.HttpStatusCode.BadRequest, $"{nameof(settings.currencyPlural)} is null"); }
                if (string.IsNullOrWhiteSpace(settings.currencySingular)) { return StatusCode((int)System.Net.HttpStatusCode.BadRequest, $"{nameof(settings.currencySingular)} is null"); }

                var userInfo = this.authenticationProvider.Authenticate(this.HttpContext);
                if (userInfo.Result != Constants.AuthenticationResult.Success) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"Could not authenticate"); }
                if (this.appSettings.TwitchClientID != userInfo.GetClientID()) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"{nameof(this.appSettings.TwitchClientID)} does not match"); }
                if (settings.userID != userInfo.GetUserID()) { return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, $"{nameof(settings.userID)} is not yours"); }

                var validationResult = settings.validate();
                if (validationResult != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                {
                    return StatusCode((int)System.Net.HttpStatusCode.BadRequest, new { message = validationResult.ErrorMessage });
                }

                var currentSettings = this.userHelper.EnsureDefaultUserSettings(userInfo.GetUserID());
                var pointsManager = this.userHelper.GetPointsManager(settings);

                if (settings.streamElementsAccountID != currentSettings.streamElementsAccountID ||
                    settings.streamElementsJWTToken != currentSettings.streamElementsJWTToken)
                {
                    var setupCorrect = pointsManager.CheckSettingsCorrect(userInfo.GetUserID());

                    if (!setupCorrect)
                    {
                        return StatusCode((int)System.Net.HttpStatusCode.BadRequest, new { message = "Could not verify Stream Elements Account settings. Please make sure they're configured correctly." });
                    }
                }


                this.userHelper.SaveSettings(settings);

                if (settings.basicBotConfiguration.botEnabled)
                {
                    var config = this.userHelper.GetConfigurationForUser(settings);
                    if (config != null)
                    {
                        var botJustCreated = Startup.chatBotManager.RegisterConnection(config);
                        if (!botJustCreated)
                        {
                            Startup.chatBotManager.UpdateConnection(userInfo.GetUserID(), pointsManager, config.basicConfiguration);
                        }
                    }
                    else
                    {
                        this._logger.LogError($"UserID {settings.userID} got back null as BotConfiguration");
                    }
                }
                else
                {
                    Startup.chatBotManager.UnregisterConnection(settings.userID);
                }

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
