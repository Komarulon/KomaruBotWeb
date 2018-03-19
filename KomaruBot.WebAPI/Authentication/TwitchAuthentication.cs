using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KomaruBot.WebAPI.Authentication
{
    public class TwitchAuthentication : IAuthenticationProvider
    {
        public Models.AuthenticationResult Authenticate(HttpContext context)
        {
            string tkn;
            return this.Authenticate(context, out tkn);
        }

        public Models.AuthenticationResult Authenticate(HttpContext context, out string token)
        {
            var tokenHeader = context.Request?.Headers["Authorization"];
            var tokenHeaderValue = tokenHeader.Value.FirstOrDefault();
            if (tokenHeaderValue == null)
            {
                token = null;
                return new Models.AuthenticationResult
                {
                    Result = Constants.AuthenticationResult.MissingHeader,
                    Message = "Header 'Authorization' was missing",
                };
            }
            return this.Authenticate(tokenHeaderValue, out token);
        }

        public Models.AuthenticationResult Authenticate(string authorizationHeader)
        {
            string tkn;
            return this.Authenticate(authorizationHeader, out tkn);
        }

        public Models.AuthenticationResult Authenticate(string authorizationHeader, out string token)
        {
            token = null;
            try
            {
                if (string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    return new Models.AuthenticationResult
                    {
                        Result = Constants.AuthenticationResult.MissingHeader,
                        Message = "authorization header is empty",
                    };
                }
                var tokenSplit = authorizationHeader.Split(' ');
                if (tokenSplit.Length != 2)
                {
                    return new Models.AuthenticationResult
                    {
                        Result = Constants.AuthenticationResult.MalformedHeader,
                        Message = "authorization header was not two pieces (Bearer [token])",
                    };
                }

                if (tokenSplit[0].ToLower() != "Bearer".ToLower())
                {
                    return new Models.AuthenticationResult
                    {
                        Result = Constants.AuthenticationResult.MalformedHeader,
                        Message = "authorization header was not Bearer (Bearer [token])",
                    };
                }

                HttpClient client = new HttpClient();

                var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/kraken?oauth_token=" + tokenSplit[1]);
                request.Headers.Accept.Clear();
                //request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                //request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                //request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v5+json"));
                //request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Oauth", tokenSplit[1]);

                var content = client.SendAsync(request).Result;
                string responseBody = content.Content.ReadAsStringAsync().Result;

                
                if (content.IsSuccessStatusCode)
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.AuthenticationResult.TwitchAuthenticationResult>(responseBody);
                    if (result.identified && result.token.valid)
                    {
                        token = tokenSplit[1];

                        return new Models.AuthenticationResult
                        {
                            Result = Constants.AuthenticationResult.Success,
                            Message = null,
                            Details = result,
                        };
                    }
                    else
                    {
                        return new Models.AuthenticationResult
                        {
                            Result = Constants.AuthenticationResult.Success,
                            Message = $"Either result.identified ({result.identified}) or result.token.valid ({result.token.valid}) was false",
                            Details = result,
                        };
                    }
                }
                else
                {
                    throw new Exception($"Response status code ({content.StatusCode}) was not a success code. Response body: {responseBody}");
                }
            }
            catch (Exception exc)
            {
                return new Models.AuthenticationResult
                {
                    Result = Constants.AuthenticationResult.Fail,
                    Message = "There was an internal error authenticating.",
                };
            }
        }
    }
}
