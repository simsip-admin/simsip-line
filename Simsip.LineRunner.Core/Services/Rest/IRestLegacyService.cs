using System;
using System.Collections.Generic;
using System.Net;
using Simsip.Core.Models.OAuth;

namespace Simsip.Core.Services.Rest
{
    public interface IRestLegacyService
    {
        /* TODO: Couldn't figure out how to do a synchronous version yet
        string MakeRequest(string url,
                           string verb,
                           string body,
                           OAuthType oauthType);
        */

        void MakeRequestAsync(string url,
                              string verb,
                              OAuthType oauthType,
                              Action<HttpStatusCode, string> serviceSuccessAction,
                              Action<Exception> serviceErrorAction);

        void MakeRequestAsync(string url,
                              string verb,
                              Dictionary<string, string> parameters,
                              OAuthType oauthType,
                              Action<HttpStatusCode, string> serviceSuccessAction,
                              Action<Exception> serviceErrorAction);
            
        void MakeRequestAsync(string url, 
                              string verb, 
                              string body, 
                              OAuthType oauthType,
                              Action<HttpStatusCode, string> successAction, 
                              Action<Exception> errorAction);

        void MakeRequestAsync(HttpWebRequest request,
                              string body,
                              OAuthType oauthType,
                              Action<HttpStatusCode, string> serviceSuccessAction,
                              Action<Exception> serviceErrorAction);
    }
}