using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using Simsip.LineRunner.Data.Scoreoid;
using Simsip.LineRunner.Entities.Scoreoid;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Scoreoid
{
    public class ScoreoidScoreService : IScoreoidScoreService
    {

        private readonly IRestService _restService;

        private const string CREATE_SCORE_SUCCESS_STRING = "The score has been created";
        private const string ORDER_BY_SCORE_PARAMETER = "score";
        private const string ORDER_DESC_PARAMETER = "desc";

        public ScoreoidScoreService()
        {
            _restService = new RestService();
        }

        #region IScoreoidScoreService Implementation

        /// <summary>
        /// Lets you create a user score.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreateScore(ScoreEntity scoreEntity)
        {
            var success = false;

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["api_key"] = ScoreoidCredentials.API_KEY;
                parameters["game_id"] = ScoreoidCredentials.LINERUNNER_GAME_ID;
                parameters["response"] = ScoreoidServiceUrls.RESPONSE_TYPE;
                parameters["username"] = scoreEntity.Username;
                parameters["score"] = scoreEntity.Score.ToString();

                /* TODO
                var response = await _restService.MakeRequestAsync(ScoreoidServiceUrls.BaseUrl + "createScore",
                                        RestVerbs.Post,
                                        parameters);

                success = ParseCreateScoreResponse(response);

                if (success)
                {
                    var scoreRepository = new ScoreRepository();
                    scoreRepository.Create(scoreEntity);
                }
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in CreateScore: " + ex);
            }

            return success;
        }

        /// <summary>
        /// Lets you create incremental leaderboards.
        /// </summary>
        /// <returns></returns>
        public async Task IncrementScore()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lets you pull all your game scores.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<ScoreEntity>> GetScores(PlayerEntity playerEntity)
        {
            IList<ScoreEntity> responseScoreEntities = null;

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["api_key"] = ScoreoidCredentials.API_KEY;
                parameters["game_id"] = ScoreoidCredentials.LINERUNNER_GAME_ID;
                parameters["response"] = ScoreoidServiceUrls.RESPONSE_TYPE;
                parameters["order_by"] = ORDER_BY_SCORE_PARAMETER;
                parameters["order"] = ORDER_DESC_PARAMETER;
                parameters["limit"] = "5";
                parameters["username"] = playerEntity.Username;

                /* TODO
                var response = await _restService.MakeRequestAsync(ScoreoidServiceUrls.BaseUrl + "getScores",
                                        RestVerbs.Post,
                                        parameters);

                responseScoreEntities = ParseGetScoresResponse(response);
                */
            }
            catch (Exception ex)
            {
                responseScoreEntities = null;

                Debug.WriteLine("Exception in GetScores: " + ex);
            }

            return responseScoreEntities;
        }

        /// <summary>
        /// Lets you get all your games best scores.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<ScoreEntity>> GetBestScores()
        {
            IList<ScoreEntity> responseScoreEntities = null;

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["api_key"] = ScoreoidCredentials.API_KEY;
                parameters["game_id"] = ScoreoidCredentials.LINERUNNER_GAME_ID;
                parameters["response"] = ScoreoidServiceUrls.RESPONSE_TYPE;
                parameters["order_by"] = ORDER_BY_SCORE_PARAMETER;
                parameters["order"] = ORDER_DESC_PARAMETER;
                parameters["limit"] = "5";

                /* TODO
                var response = await _restService.MakeRequestAsync(ScoreoidServiceUrls.BaseUrl + "getBestScores",
                                        RestVerbs.Post,
                                        parameters);

                responseScoreEntities = ParseGetBestScoresResponse(response);
                */
            }
            catch (Exception ex)
            {
                responseScoreEntities = null;

                Debug.WriteLine("Exception in GetBestScores: " + ex);
            }

            return responseScoreEntities;
        }

        /// <summary>
        /// Lets you count all your game scores.
        /// </summary>
        /// <returns></returns>
        public async Task<int> CountScores()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lets you count all your game best scores.
        /// </summary>
        /// <returns></returns>
        public async Task<int> CountBestScores()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lets you get all your game average scores.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetAverageScore()
        {
            var averageScore = 0;

            try
            {
                /* TODO
                var parameters = new Dictionary<string, string>();
                parameters["api_key"] = ScoreoidCredentials.API_KEY;
                parameters["game_id"] = ScoreoidCredentials.LINERUNNER_GAME_ID;
                parameters["response"] = ScoreoidServiceUrls.RESPONSE_TYPE;

                var response = await _restService.MakeRequestAsync(ScoreoidServiceUrls.BaseUrl + "getAverageScore",
                                        RestVerbs.Post,
                                        parameters);

                averageScore = ParseGetAverageScoreResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in GetAverageScore: " + ex);
            }

            return averageScore;
        }

        #endregion

        #region Helper functions

        private bool ParseCreateScoreResponse(string response)
        {
            var success = false;

            try
            {
                var parsedResponse = JObject.Parse(response);

                var successString = (string)parsedResponse["success"];
                if (successString == CREATE_SCORE_SUCCESS_STRING)
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in ParseCreateScoreResponse: " + ex);
            }

            return success;
        }

        private IList<ScoreEntity> ParseGetScoresResponse(string response)
        {
            var returnScores = new List<ScoreEntity>();
            var scoreRepository = new ScoreRepository();

            try
            {
                var parsedResponse = JObject.Parse(response);

                int scoreCount = parsedResponse.Count;
                for(int i = 0; i < scoreCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedScore = parsedResponse[i]["Score"];

                        var scoreEntity = new ScoreEntity();
                        scoreEntity.Score = (int)parsedScore["score"];
                        scoreEntity.CreatedDate = (DateTime)parsedScore["created"];

                        // Is this a new score?
                        var scoreOnDevice = scoreRepository.GetScoreByScore(scoreEntity.Score);
                        if (scoreOnDevice == null)
                        {
                            scoreRepository.Create(scoreEntity);
                        }

                        returnScores.Add(scoreEntity);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual score: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing scores: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnScores;
        }

        private IList<ScoreEntity> ParseGetBestScoresResponse(string response)
        {
            var returnScores = new List<ScoreEntity>();

            try
            {
                var parsedResponse = JObject.Parse(response);

                int scoreCount = parsedResponse.Count;
                for (int i = 0; i < scoreCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedPlayer = parsedResponse[i]["Player"];
                        var parsedScore = parsedResponse[i]["Score"];

                        var scoreEntity = new ScoreEntity();
                        scoreEntity.Username = (string)parsedPlayer["username"];
                        scoreEntity.Score = (int)parsedScore["score"];
                        scoreEntity.CreatedDate = (DateTime)parsedScore["created"];

                        returnScores.Add(scoreEntity);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual best score: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing best scores: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnScores;
        }

        private int ParseGetAverageScoreResponse(string response)
        {
            var averageScore = 0;

            try
            {
                var parsedResponse = JObject.Parse(response);

                averageScore = (int)parsedResponse["average_score"];
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in ParseGetAverageScoreResponse: " + ex);
            }

            return averageScore;
        }

        #endregion
    }
}