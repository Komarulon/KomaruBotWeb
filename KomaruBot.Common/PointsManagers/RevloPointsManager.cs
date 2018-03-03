using KomaruBot.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace KomaruBot.Common.PointsManagers
{
    public class RevloPointsManager : IPointsManager
    {
        private string apiKey;
        public string CurrencyPlural { get; private set; }
        public string CurrencySingular { get; private set; }
        private ILogger _logger;
        public RevloPointsManager(
            ILogger logger,
            string apiKey,
            string currencyPlural,
            string currencySingular
            )
        {
            this._logger = logger;
            this.apiKey = apiKey;
            this.CurrencyPlural = currencyPlural;
            this.CurrencySingular = currencySingular;
        }

        public bool CheckSettingsCorrect(string userID)
        {
            throw new NotImplementedException();
        }

        public long GetCurrentPlayerPoints(string userName)
        {
            throw new NotImplementedException();
        }

        public void GivePlayerPoints(string userName, long amount, out long? newAmount)
        {
            newAmount = null;
            try
            {
                HttpClient client = new HttpClient();

                var request = new HttpRequestMessage(new HttpMethod("POST"), $"https://api.revlo.co/1/fans/{userName}/points/bonus");
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Host = "api.revlo.co";
                request.Headers.ConnectionClose = true;
                request.Headers.Add("x-api-key", apiKey);
                request.Content = new StringContent($"{{\"amount\": {amount}}}");

                var content = client.SendAsync(request).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to award {userName} {amount} {(amount == 1 ? CurrencySingular : CurrencyPlural)}. Please do so manually.");

            }
        }
    }
}
