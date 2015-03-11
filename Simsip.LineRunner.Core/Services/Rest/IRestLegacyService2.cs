using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace Simsip.LineRunner.Services.Rest
{
    public interface IRestService
    {
        Task<string> MakeRequestAsync(string url,
                                      RestVerbs verb);

        Task<string> MakeRequestAsync(string url,
                                      RestVerbs verb,
                                      Dictionary<string, string> parameters);
            
        Task<string> MakeRequestAsync(string url, 
                                      RestVerbs verb, 
                                      string body);

        Task<string> MakeRequestAsync(HttpClient client,
                                      string url,
                                      RestVerbs verb,
                                      HttpContent content);

    }
}