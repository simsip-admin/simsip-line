using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Services.OAuth;

namespace Simsip.LineRunner.Services.Rest
{
    public class RestService : IRestService
    {
        #region IRestService implementation

        public async Task<string> MakeRequestAsync(string url,
                                                   RestVerbs verb,
                                                   OAuthType oauthType)
        {
            using (var content = new StringContent(string.Empty))
            {
                var response = await MakeRequestAsync(url, verb, content, oauthType);
                return response;
            }
        }

        public async Task<string> MakeRequestAsync(string url,
                                                   RestVerbs verb,
                                                   Dictionary<string, string> parameters,
                                                   OAuthType oauthType)
        {
            using (var content = new FormUrlEncodedContent(parameters))
            {
                var response = await MakeRequestAsync(url, verb, content, oauthType);
                return response;
            }
        }

        public async Task<string> MakeRequestAsync(string url,
                                                   RestVerbs verb,
                                                   string body,
                                                   OAuthType oauthType)
        {
            using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
            {
                var response = await MakeRequestAsync(url, verb, content, oauthType);
                return response;
            }

        }

        public async Task<string> MakeRequestAsync(HttpClient client,
                                                   string url,
                                                   RestVerbs verb,
                                                   HttpContent content)
        {
            string response = string.Empty;

            switch (verb)
            {
                case RestVerbs.Get:
                    {
                        response = await client.GetStringAsync(url);
                        break;
                    }
                case RestVerbs.Post:
                    {
                        var result = await client.PostAsync(url, content);
                        response = await result.Content.ReadAsStringAsync();
                        break;
                    }
                case RestVerbs.Put:
                    {
                        var result = await client.PutAsync(url, content);
                        response = await result.Content.ReadAsStringAsync();
                        break;
                    }
                case RestVerbs.Delete:
                    {
                        var result = await client.DeleteAsync(url);
                        response = await result.Content.ReadAsStringAsync();
                        break;
                    }
            }

            return response;
        }


        #endregion

        #region Helper methods

        private async Task<string> MakeRequestAsync(string url,
                                                    RestVerbs verb,
                                                    HttpContent content,
                                                    OAuthType oauthType)
        {
            string response = string.Empty;

            using (var client = new HttpClient())
            {
                switch (oauthType)
                {
                    case OAuthType.Facebook:
                        {
                            // Reference: https://developers.facebook.com/docs/facebook-login/using-login-with-devices/#step5
                            url += "access_token="; // + App.FacebookAccessToken;
                            break;
                        }
                    case OAuthType.Groupme:
                        {
                            client.DefaultRequestHeaders.Add("X-Access-Token", string.Empty /*App.GroupmeAccessToken*/);
                            break;
                        }
                    case OAuthType.Youtube:
                    {
                        url += "&access_token=" + /* + App.YoutubeAccessToken + */
                                   "&key=" + OAuthCredentials.YOUTUBE_APP_ID;
                            break;
                        }
                }

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                switch (verb)
                {
                    case RestVerbs.Get:
                        {
                            response = await client.GetStringAsync(url);
                            break;
                        }
                    case RestVerbs.Post:
                        {
                            var result = await client.PostAsync(url, content);
                            response = await result.Content.ReadAsStringAsync();
                            break;
                        }
                    case RestVerbs.Put:
                        {
                            var result = await client.PutAsync(url, content);
                            response = await result.Content.ReadAsStringAsync();
                            break;
                        }
                    case RestVerbs.Delete:
                        {
                            var result = await client.DeleteAsync(url);
                            response = await result.Content.ReadAsStringAsync();
                            break;
                        }
                }

            }

            return response;
        }

        #endregion

    }
}
