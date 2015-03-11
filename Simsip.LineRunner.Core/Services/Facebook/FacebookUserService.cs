using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Services.Rest;
using Simsip.LineRunner.Data.Facebook;
using Newtonsoft.Json.Linq;
using Simsip.LineRunner.Entities.OAuth;

namespace Simsip.LineRunner.Services.Facebook
{
    public class FacebookUserService : IFacebookUserService
    {
        private const string TRACE_TAG = "FacebookUserService";

        private readonly IRestService _restService;
        private readonly IFacebookUserRepository _userRepository;

        public FacebookUserService(IRestService restService, 
                                   IFacebookUserRepository userRepository)
        {
            _restService = restService;
            _userRepository = userRepository;
        }

        #region IFacebookUserService Implementation

        #region User

        public async Task<FacebookUserEntity> GetUser()
        {
            FacebookUserEntity returnUser = null;

            try
            {
                var fields = "id,name,username,last_name,first_name,middle_name,gender,email,website,picture";

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                                        "me?fields=" + fields,
                                        RestVerbs.Get,
                                        OAuthType.Facebook);

                // TODO: Analyze http status code here before parsing

                returnUser = ParseGetUserResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook user: " + ex);
            }

            return returnUser;
        }

        #endregion

        #endregion

        #region Helper methods


        private FacebookUserEntity ParseGetUserResponse(string response)
        {
            var returnUser = new FacebookUserEntity();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedUser = parsedResponse["data"];

                // Get the simplistic values first
                returnUser.ServerId = parsedUser["id"].Value<string>();
                returnUser.Name = parsedUser["name"].Value<string>();
                returnUser.Username = parsedUser["username"].Value<string>();
                returnUser.LastName = parsedUser["last_name"].Value<string>();
                returnUser.FirstName = parsedUser["first_name"].Value<string>();
                returnUser.MiddleName = parsedUser["middle_name"].Value<string>();
                returnUser.Gender = parsedUser["gender"].Value<string>();
                returnUser.Email = parsedUser["email"].Value<string>();
                returnUser.Website = parsedUser["website"].Value<string>();

                // Carefully extract out the picture url (has changed)
                try
                {
                    returnUser.Picture = parsedUser["picture"].Value<string>();
                }
                catch (Exception ex)
                {
                    try
                    {
                        returnUser.Picture = parsedUser["picture"]["data"]["url"].Value<string>();
                    }
                    catch (Exception exInner)
                    {
                        Debug.WriteLine("Exception parsing facebook user picture: " + exInner);
                    }
                }

                // Is this a new user?
                // TODO: Walk create flow, do we want to auto-delete to maintain just one record?
                var userOnDevice = _userRepository.ReadByServerId(returnUser.ServerId);
                if (userOnDevice == null)
                {
                    _userRepository.Create(returnUser);
                }
                else
                {
                    _userRepository.Update(returnUser);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing facebook user: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnUser;
        }

        #endregion
    }
}