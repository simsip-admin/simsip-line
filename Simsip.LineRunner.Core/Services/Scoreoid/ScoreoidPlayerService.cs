using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using Simsip.LineRunner.Data;
using Simsip.LineRunner.Entities.Scoreoid;
using Simsip.LineRunner.Services.Rest;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Simsip.LineRunner.Data.Scoreoid;


namespace Simsip.LineRunner.Services.Scoreoid
{
    public class ScoreoidPlayerService : IScoreoidPlayerService
    {

        private readonly IRestService _restService;

        private const string CREATE_PLAYER_SUCCESS_STRING = "The player has been created";

        public ScoreoidPlayerService()
        {
            _restService = new RestService();
        }

        #region IScoreoidPlayerService Implementation

        /// <summary>
        /// Lets you create a player with a number of optional values.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CreatePlayer(PlayerEntity playerEntity)
        {
            var success = false;

            try
            {
                /* TODO
                var parameters = new Dictionary<string, string>();
                parameters["api_key"] = ScoreoidCredentials.API_KEY;
                parameters["game_id"] = ScoreoidCredentials.LINERUNNER_GAME_ID;
                parameters["response"] = ScoreoidServiceUrls.RESPONSE_TYPE;
                parameters["username"] = playerEntity.Username;
                parameters["password"] = playerEntity.Password;

                var response = await _restService.MakeRequestAsync(ScoreoidServiceUrls.BaseUrl + "createPlayer",
                                        RestVerbs.Post,
                                        parameters);

                success = ParseCreatePlayerResponse(response);

                if (success)
                {
                    var playerRepository = new PlayerRepository();
                    playerRepository.Create(playerEntity);
                }
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in CreatePlayer: " + ex);
            }

            return success;
        }

        /// <summary>
        /// Check if player exists and returns the player information. Post parameters work as query conditions. 
        /// 
        /// This method can be used for login by adding username and password parameters.
        /// </summary>
        /// <returns></returns>
        public async Task<PlayerEntity> GetPlayer(PlayerEntity playerEntity)
        {
            PlayerEntity responsePlayerEntity = null;

            try
            {
                var parameters = new Dictionary<string, string>();
                parameters["api_key"] = ScoreoidCredentials.API_KEY;
                parameters["game_id"] = ScoreoidCredentials.LINERUNNER_GAME_ID;
                parameters["response"] = ScoreoidServiceUrls.RESPONSE_TYPE;
                parameters["username"] = playerEntity.Username;
                parameters["password"] = playerEntity.Password;

                /* TODO
                var response = await _restService.MakeRequestAsync(ScoreoidServiceUrls.BaseUrl + "getPlayer",
                                        RestVerbs.Post,
                                        parameters);

                responsePlayerEntity = ParseGetPlayerResponse(response);
                */

                // Propogate these forward
                responsePlayerEntity.Username = playerEntity.Username;
                responsePlayerEntity.Password = playerEntity.Password;
            }
            catch (Exception ex)
            {
                responsePlayerEntity = null;

                Debug.WriteLine("Exception in GetPlayer: " + ex);
            }

            return responsePlayerEntity;
        }

        /// <summary>
        /// Lets you edit your player information.
        /// </summary>
        /// <returns></returns>
        public async Task EditPlayer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a player and all the players scores.
        /// </summary>
        /// <returns></returns>
        public async Task DeletePlayer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lets you pull a specific field from your player info.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetPlayerField()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lets you update your player field’s.
        /// </summary>
        /// <returns></returns>
        public async Task UpdatePlayerField()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lets you pull all the scores for a player.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<ScoreEntity>> GetPlayerScores()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the player’s rank based on the parmeters set.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetPlayerRank()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lets you count all your players.
        /// </summary>
        /// <returns></returns>
        public async Task<int> CountPlayers()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper functions

        private bool ParseCreatePlayerResponse(string response)
        {
            var success = false;

            try
            {
                var parsedResponse = JObject.Parse(response);
                
                var successString = (string)parsedResponse["success"];
                if (successString == CREATE_PLAYER_SUCCESS_STRING)
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in ParseCreatePlayerResponse");
            }

            return success;
        }

        private PlayerEntity ParseGetPlayerResponse(string response)
        {
            PlayerEntity returnPlayerEntity = null;

            try
            {
                var parsedResponse = JObject.Parse(response);
                var parsedPlayer = parsedResponse["Player"];

                returnPlayerEntity = new PlayerEntity();
                returnPlayerEntity.UniqueId = (int)parsedPlayer["unique_id"];
                returnPlayerEntity.BestScore = (int)parsedPlayer["best_score"];
                returnPlayerEntity.CreatedDate = (DateTime)parsedPlayer["created"];
                returnPlayerEntity.UpdatedDate = (DateTime)parsedPlayer["updated"];
            }
            catch (Exception ex)
            {
                returnPlayerEntity = null;

                Debug.WriteLine("Exception in ParseGetPlayerResponse");
            }

            return returnPlayerEntity;
        }


        #endregion
    }
}