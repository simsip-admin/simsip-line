using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;
using Simsip.LineRunner.Data.OAuth;
using Simsip.LineRunner.Data.Simsip;
using Simsip.LineRunner.Services.Simsip;


namespace Simsip.LineRunner.Services.Facebook
{
    public class FacebookScoreService : IFacebookScoreService
    {

        private readonly IRestService _restService;
        private readonly IFacebookScoreRepository _scoreRepository;

        public FacebookScoreService()
        {
            this._restService = new RestService();
            this._scoreRepository = new FacebookScoreRepository();
        }

        #region IFacebookScoreService Implementation

        public async Task<bool> AddScoreAsync(FacebookScoreEntity score)
        {
            var success = false;

            // Make sure we have required object/fields
            if (score == null)
            {
                throw new ArgumentException("FacebookScoreEntity parameter cannot be null");
            }
            if (score.Score <= 0)
            {
                throw new ArgumentException("FacebookScoreEntity Score parameter must be greater than 0");
            }

            try
            {
                var simsipUserRepository = new SimsipUserRepository();
                var simsipUserEntity = simsipUserRepository.ReadUser();

                var parameters = new Dictionary<string, string>();
                parameters["score"] = score.Score.ToString();
                // upload1.Add("privacy", "{\"value\":\"SELF\"}");

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" + 
                                    simsipUserEntity.FacebookId + "/scores",
                                    RestVerbs.Post,
                                    parameters,
                                    OAuthType.Facebook);

                // TODO: Will this be too brittle?
                if (response.ToLower() == "true")
                {
                    success = true;
                }

                _scoreRepository.Create(score);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook score: " + ex);
            }

            return success;
        }

        public async Task<IList<FacebookScoreEntity>> GetScoresForPlayerAsync()
        {
            var returnScores = new List<FacebookScoreEntity>();

            try
            {
                var simsipUserRepository = new SimsipUserRepository();
                var simsipUserEntity = simsipUserRepository.ReadUser();

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                                    simsipUserEntity.FacebookId + "/scores",
                                    RestVerbs.Get,
                                    OAuthType.Facebook);
                
                returnScores = ParseGetScoresResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting facebook player scores: " + ex);
            }

            return returnScores;
        }

        public async Task<IList<FacebookScoreEntity>> GetScoresForAllPlayersAsync()
        {
            var returnScores = new List<FacebookScoreEntity>();

            try
            {
                var oauthAccessRepository = new OAuthAccessRepository();
                var facebookOAuthAccess = oauthAccessRepository.Read(OAuthType.Facebook);

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                                    facebookOAuthAccess.AppId + "/scores",
                                    RestVerbs.Get,
                                    OAuthType.Facebook);

                returnScores = ParseGetScoresResponse(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting all facebook player scores: " + ex);
            }

            return returnScores;
        }

        public async Task<bool> UpdateScoreAsync(SimsipUserEntity simsipUser, FacebookScoreEntity score)
        {
            var success = false;

            // Make sure we have required object/fields
            if (score == null)
            {
                throw new ArgumentException("FacebookScoreEntity parameter cannot be null");
            }
            if (score.Score <= 0)
            {
                throw new ArgumentException("FacebookScoreEntity Score parameter must be greater than 0");
            }

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["score"] = score.Score.ToString();
                // upload1.Add("privacy", "{\"value\":\"SELF\"}");

                var response = await _restService.MakeRequestAsync(FacebookServiceUrls.BaseUrl + "/" +
                                    simsipUser.FacebookId + "/scores",
                                    RestVerbs.Post,
                                    parameters,
                                    OAuthType.Facebook);

                // TODO: Will this be too brittle?
                if (response.ToLower() == "true")
                {
                    success = true;
                }

                _scoreRepository.Create(score);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding facebook score: " + ex);
            }

            return success;

        }

        #endregion

        #region Helper functions

        private List<FacebookScoreEntity> ParseGetScoresResponse(string response)
        {
            var returnScores = new List<FacebookScoreEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedScores = parsedResponse["data"];

                int scoreCount = parsedScores.Count();
                for(int i = 0; i < scoreCount; i++)
                {
                     // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedScore = parsedScores[i];
                        var facebookScoreEntity = new FacebookScoreEntity();

                        // Get the simplistic values first
                        facebookScoreEntity.PlayerId = (string)parsedScore["user"]["id"];
                        facebookScoreEntity.PlayerName = (string)parsedScore["user"]["name"];
                        facebookScoreEntity.Score = (int)parsedScore["score"];
                        facebookScoreEntity.AppId = (string)parsedScore["application"]["id"];
                        facebookScoreEntity.AppName = (string)parsedScore["appliction"]["name"];
                        facebookScoreEntity.Type = (string)parsedScore["type"];
                        
                        // Is this a new score?
                        // TODO: What do we want to do here?
                        var scoreOnDevice = _scoreRepository.GetScoreForUserAndAppId(facebookScoreEntity.PlayerId,
                            facebookScoreEntity.AppId);
                        if (scoreOnDevice == null)
                        {
                            _scoreRepository.Create(facebookScoreEntity);
                        }
                        else
                        {
                            _scoreRepository.Update(facebookScoreEntity);
                        }

                        returnScores.Add(facebookScoreEntity);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual facebook score: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing facebook scores: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnScores;
        }

        #endregion
    }
}