using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Cirrious.CrossCore;
using Simsip.Core.Models.OAuth;
using Simsip.Core.Services.OAuth;

namespace Simsip.Core.Services.Rest
{
    public class RestLegacyService : IRestLegacyService
    {
        #region IRestService implementation

        public string MakeRequest(string url, 
                                  string verb, 
                                  string body,
                                  OAuthType oauthType)
        {
            
            string result = null;

            /* TODO: Can't figure a synchronous verion of this one yet
            var req = (HttpWebRequest) WebRequest.Create(url);

            req.Method = verb;
            if (verb == "POST")
            {
                req.ContentType = "application/x-www-form-urlencoded";
            }

            if (!string.IsNullOrEmpty(body))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body.ToString());
                req.ContentLength = bytes.Length;
                using (Stream st = req.GetRequestStream())
                {
                    st.Write(bytes, 0, bytes.Length);
                }
            }

            try
            {
                using (WebResponse resp = (WebResponse)req.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        result = sr.ReadToEnd();
                    }
                }
            }
            catch (WebException wexc)
            {
            }
            */

            return result;
        }

        public void MakeRequestAsync(string url,
                                     string verb,
                                     OAuthType oauthType,
                                     Action<HttpStatusCode, string> serviceSuccessAction,
                                     Action<Exception> serviceErrorAction)
        {
            var body = string.Empty;
            MakeRequestAsync(url, verb, body, oauthType, serviceSuccessAction, serviceErrorAction);
        }

        public void MakeRequestAsync(string url, 
                                     string verb, 
                                     Dictionary<string,string> parameters,
                                     OAuthType oauthType,
                                     Action<HttpStatusCode, string> serviceSuccessAction, 
                                     Action<Exception> serviceErrorAction)
        {
            // Or, alternative implementation: http://stackoverflow.com/questions/7023046/sending-namevaluecollection-to-http-request-c-sharp
            List<string> items = new List<string>();
            foreach (var key in parameters.Keys)
            {
                /*
                You use EscapeUriString if what you are escaping is a URI, and EscapeDataString everywhere else.
                There are differences on how those two encode the strings
                More info here: http://blogs.msdn.com/b/yangxind/archive/2006/11/09/don-t-use-net-system-uri-unescapedatastring-in-url-decoding.aspx
                */
                items.Add(String.Concat(Uri.EscapeDataString(key), "=", Uri.EscapeDataString(parameters[key])));
            }
            var body = String.Join("&", items.ToArray());

            MakeRequestAsync(url, verb, body, oauthType, serviceSuccessAction, serviceErrorAction);
        }

        public void MakeRequestAsync(string url,
                                string verb,
                                string body,
                                OAuthType oauthType,
                                Action<HttpStatusCode, string> serviceSuccessAction,
                                Action<Exception> serviceErrorAction)
        {
            switch (oauthType)
            {
                case OAuthType.Facebook:
                    {
                        // Reference: https://developers.facebook.com/docs/facebook-login/using-login-with-devices/#step5
                        url += "access_token=" + App.FacebookAccessToken;
                        break;
                    }
                case OAuthType.Youtube:
                    {
                        url += "&access_token=" + App.YoutubeAccessToken +
                               "&key=" + OAuthCredentials.YOUTUBE_APP_ID;
                        break;
                    }
            }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = verb;
            request.Accept = "application/json";

            MakeRequestAsync(request, body, oauthType, serviceSuccessAction, serviceErrorAction);
        }

        public void MakeRequestAsync(HttpWebRequest request,
                                     string body,
                                     OAuthType oauthType,
                                     Action<HttpStatusCode, string> serviceSuccessAction, 
                                     Action<Exception> serviceErrorAction)
        {
            switch (oauthType)
            {
                case OAuthType.Groupme:
                    {
                        request.Headers["X-Access-Token"] = App.GroupmeAccessToken;
                        break;
                    }
            }

            if (!string.IsNullOrEmpty(body))
            {
                request.BeginGetRequestStream(token =>
                {
                    try
                    {
                        using (var postStream = request.EndGetRequestStream(token))
                        {
                            byte[] byteArray = Encoding.UTF8.GetBytes(body);
                            postStream.Write(byteArray, 0, body.Length);
                        }

                        MakeRequest(request,
                            (code, response) =>
                                {
                                    if (serviceSuccessAction != null)
                                    {
                                        serviceSuccessAction(code, response);
                                    }
                                },
                            error =>
                            {
                                if (serviceErrorAction != null)
                                {
                                    serviceErrorAction(error);
                                }
                            }
                        );
                    }
                    catch (WebException ex)
                    {
                        Mvx.Error("ERROR: '{0}' when making {1} request to {2}", ex.Message, request.Method, request.RequestUri.AbsoluteUri);

                        if (serviceErrorAction != null)
                        {
                            serviceErrorAction(ex);
                        }
                    }
                }, null);

            }
            else
            {
                MakeRequest(request,
                   (code, response) =>
                   {
                       if (serviceSuccessAction != null)
                       {
                           serviceSuccessAction(code, response);
                       }
                   },
                   error =>
                   {
                       if (serviceErrorAction != null)
                       {
                           serviceErrorAction(error);
                       }
                   }
                );
            }
        }

        #endregion

        #region Helper functions

        private void MakeRequest(HttpWebRequest request, 
                                 Action<HttpStatusCode, string> requestSuccessAction, 
                                 Action<Exception> requestErrorAction)
        {
            request.BeginGetResponse(token =>
            {
                try
                {
                    using (var response = (HttpWebResponse)request.EndGetResponse(token))
                    {
                        using (var stream = response.GetResponseStream())
                        {
                            var reader = new StreamReader(stream);
                            requestSuccessAction(response.StatusCode, reader.ReadToEnd());
                        }
                    }
                }
                catch (WebException ex)
                {
                    Mvx.Error("ERROR: '{0}' when making {1} request to {2}", ex.Message, request.Method, request.RequestUri.AbsoluteUri);
                    requestErrorAction(ex);
                }
            }, null);
        }

        #endregion
    }
}
