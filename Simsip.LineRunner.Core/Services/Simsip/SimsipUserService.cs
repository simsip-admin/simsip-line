using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Data.Simsip;
// using Simsip.Plugins.MobileServices;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Simsip
{
    public class SimsipUserService : ISimsipUserService
    {
        private const string TRACE_TAG = "SimsipUserService";

        private readonly ISimsipUserRepository _userRepository;
        
        public SimsipUserService(ISimsipUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        #region ISimsipUserService Implementation

        public async Task<SimsipUserEntity> AddUserAsync(SimsipUserEntity user) 
        {
            SimsipUserEntity returnUser = null;

            try
            {
                /* TODO
                var userTable = App.MobileServiceClient.GetTable(MobileServiceConstants.UserTableName);

                var userJson = new JObject();
                userJson.Add("facebook_id",user.FacebookId);

                var response = await userTable.InsertAsync(userJson);

                returnUser = ParseGetUserResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding simsip user: " + ex);
            }

            return returnUser;
        }

        public async Task<SimsipUserEntity> GetUserAsync(FacebookUserEntity facebookUser)
        {
            SimsipUserEntity returnUser = null;

            try
            {
                /* TODO
                var userTable = App.MobileServiceClient.GetTable(MobileServiceConstants.UserTableName);

                var response = await userTable.ReadAsync("$filter=facebook_id eq " + facebookUser.ServerId);

                returnUser = ParseGetUserResponse(response);
                */
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Exception getting simsip user: " + ex);
            }

            return returnUser;
        }

        public async Task<SimsipUserEntity> UpdateUserAsync(SimsipUserEntity user) 
        {
            SimsipUserEntity returnUser = null;

            try
            {
                /* TODO
                var userTable = App.MobileServiceClient.GetTable(MobileServiceConstants.UserTableName);

                var userJson = new JObject();
                userJson.Add("id", user.ServerId);
                userJson.Add("facebook_id", user.FacebookId);

                var response = await userTable.UpdateAsync(userJson);

                returnUser = ParseGetUserResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception updating simsip user: " + ex);
            }

            return returnUser;
        }

        public async Task DeleteUserAsync(SimsipUserEntity user) 
        {
            try
            {
                /*
                var userTable = App.MobileServiceClient.GetTable(MobileServiceConstants.UserTableName);

                var userJson = new JObject();
                userJson.Add("id", user.ServerId);

                await userTable.DeleteAsync(userJson);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception deleting simsip user: " + ex);
            }
        }

        #endregion

        #region Helper functions

        private SimsipUserEntity ParseGetUserResponse(JToken response)
        {
            SimsipUserEntity returnUser = null;

            try
            {
                returnUser = new SimsipUserEntity();

                // Get the simplistic values first
                returnUser.ServerId = response["id"].Value<string>();

                // Is this a new user?
                var userOnDevice = _userRepository.ReadUser();
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
                Debug.WriteLine("Exception parsing simsip user: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnUser;
        }

        #endregion
    }
}