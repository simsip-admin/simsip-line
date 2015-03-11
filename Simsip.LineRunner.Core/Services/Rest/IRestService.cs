using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Simsip.LineRunner.Entities.OAuth;


namespace Simsip.LineRunner.Services.Rest
{
    public interface IRestService
    {
        Task<string> MakeRequestAsync(string url,
                                      RestVerbs verb,
                                      OAuthType oauthType);

        Task<string> MakeRequestAsync(string url,
                                      RestVerbs verb,
                                      Dictionary<string, string> parameters,
                                      OAuthType oauthType);
            
        Task<string> MakeRequestAsync(string url, 
                                      RestVerbs verb, 
                                      string body, 
                                      OAuthType oauthType);

        Task<string> MakeRequestAsync(HttpClient client,
                                      string url,
                                      RestVerbs verb,
                                      HttpContent content);

    }
}