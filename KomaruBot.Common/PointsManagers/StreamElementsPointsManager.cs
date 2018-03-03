using KomaruBot.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace KomaruBot.Common.PointsManagers
{
    public class StreamElementsPointsManager : IPointsManager
    {
        private string apiKey;
        public string CurrencyPlural { get; private set; }
        public string CurrencySingular { get; private set; }
        private string streamElementsAccountID;
        private ILogger _logger;

        public StreamElementsPointsManager(
            ILogger logger,
            string apiKey,
            string currencyPlural,
            string currencySingular,
            string streamElementsAccountID
            )
        {
            this._logger = logger;
            this.apiKey = apiKey;
            this.CurrencyPlural = currencyPlural;
            this.CurrencySingular = currencySingular;
            this.streamElementsAccountID = streamElementsAccountID;
        }

        public bool CheckSettingsCorrect(string userID)
        {
            long? newAmount;
            var success = this.givePoints(userID, 10, out newAmount);
            if (success)
            {
                success = this.givePoints(userID, -10, out newAmount);
            }
            return success;
        }

        public void GivePlayerPoints(string userName, long amount, out long? newAmount)
        {
            this.givePoints(userName, amount, out newAmount);
        }

        private bool givePoints(string userName, long amount, out long? newAmount)
        {
            try
            {
                HttpClient client = new HttpClient();

                var request = new HttpRequestMessage(new HttpMethod("PUT"), $"https://api.streamelements.com/kappa/v2/points/{streamElementsAccountID}/{userName}/{amount}");
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var content = client.SendAsync(request).Result;
                string responseBody = content.Content.ReadAsStringAsync().Result;

                if (content.IsSuccessStatusCode)
                {
                    var points = Newtonsoft.Json.JsonConvert.DeserializeObject<newPointsContainer>(responseBody);
                    newAmount = points.newAmount;
                    return true;
                }
                else
                {
                    throw new Exception($"Response status code ({content.StatusCode}) was not a success code. Response body: {responseBody}");
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Unable to award {userName} {amount} {(amount == 1 ? CurrencySingular : CurrencyPlural)}. Please do so manually.", true);
                newAmount = null;
                return false;
            }
        }

        public long GetCurrentPlayerPoints(string userName)
        {
            try
            {
                HttpClient client = new HttpClient();

                var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.streamelements.com/kappa/v2/points/{streamElementsAccountID}/{userName}");
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var content = client.SendAsync(request).Result;
                string responseBody = content.Content.ReadAsStringAsync().Result;

                if (content.IsSuccessStatusCode)
                {
                    var points = Newtonsoft.Json.JsonConvert.DeserializeObject<pointsContainer>(responseBody);
                    return points.points;
                }
                // TODO: figure out if this is dumb or not
                // I'm not sure if a user who just shows up will be found or not
                // and if they're showing up as NotFound than this is good
                // but this stops all logging for legit 404 responses
                else if (content.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Unable to get points for {userName}. StreamElements API returned NotFound (maybe the user has no points yet? If it happens a lot this is an error!)");
                    return 0;
                }
                else
                {
                    throw new Exception($"Response status code ({content.StatusCode}) was not a success code. Response body: {responseBody}");
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Unable to get points for {userName}.");
                return 0;
            }
        }

        private class pointsContainer
        {
            public string channel { get; set; }
            public string username { get; set; }
            public long points { get; set; }
            public long pointsAlltime { get; set; }
            public long rank { get; set; }
        }

        private class newPointsContainer
        {
            public string channel { get; set; }
            public string username { get; set; }
            public long amount { get; set; }
            public long newAmount { get; set; }
            public string message { get; set; }
        }
    }
}
