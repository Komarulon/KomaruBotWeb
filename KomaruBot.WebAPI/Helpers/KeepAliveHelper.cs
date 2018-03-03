using KomaruBot.DAL;
using KomaruBot.WebAPI.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Helpers
{
    public class KeepAliveHelper
    {
        private string uri;
        private ILogger _logger;
        public KeepAliveHelper(
            ILogger logger,
            string uri)
        {
            this.uri = uri;
            this._logger = logger;

            this.setActionTimer();
        }


        public int intervalInSeconds = (19 * 60);

        private Timer actionTimer = null;
        private void setActionTimer()
        {
            

            lock (this)
            {
                actionTimer = new Timer((state2) =>
                {
                    _logger.LogWarning("Sending Keep-alive request");

                    try
                    {
                        HttpClient client = new HttpClient();

                        var request = new HttpRequestMessage(new HttpMethod("GET"), uri);

                        var content = client.SendAsync(request).Result;
                        string responseBody = content.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception exc)
                    {
                        _logger.LogError(exc, "Exception running keep-alive request");
                    }
                    cancelActionTimer();
                    setActionTimer();


                }, null, intervalInSeconds * 1000, Timeout.Infinite);
            }
        }

        private void cancelActionTimer()
        {
            lock (this)
            {
                if (actionTimer != null)
                {
                    actionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    actionTimer.Dispose();
                    actionTimer = null;
                }
            }
        }
    }
}
