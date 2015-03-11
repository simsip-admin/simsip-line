using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Entities.Simsip;
using Simsip.LineRunner.Data.Facebook;
using Simsip.LineRunner.Data.Simsip;
using Simsip.LineRunner.Services.Facebook;
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Simsip
{
    public class SimsipFeedService : ISimsipFeedService
    {
        private const string TRACE_TAG = "SimsipFeedService";

        private IFacebookSubscriptionService _subscriptionService;
        public SimsipFeedService(IFacebookSubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        #region ISimsipFeedService Implementation

        public async Task<IList<FacebookHybridEntity>> GetFeedAsync() 
        {
            IList<FacebookHybridEntity> returnHybrids = new List<FacebookHybridEntity>();

            try
            {
                /* TODO
                var feedTable = App.MobileServiceClient.GetTable(MobileServiceConstants.FeedTableName);

                var response = await feedTable.ReadAsync("");

                var returnSubscriptions = ParseGetFeedResponse(response);

                returnHybrids = await _subscriptionService.GetSubscriptionHybridsAsync(returnSubscriptions);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting simsip feed: " + ex);
            }

            return returnHybrids;
        }

        #endregion

        #region Helper functions

        private List<FacebookSubscriptionEntity> ParseGetFeedResponse(JToken response)
        {
            var returnSubscriptions = new List<FacebookSubscriptionEntity>();

            try
            {
                var subscriptionCount = response.Count();
                for (int i = 0; i < subscriptionCount; i++)
                {
                    // Make sure one bad message doesn't spoil it for everyone else
                    try
                    {
                        var parsedSubscription = response[i];
                        var subscriptionModel = new FacebookSubscriptionEntity();

                        subscriptionModel.ServerId = parsedSubscription["id"].Value<string>();
                        subscriptionModel.SubscribedId = parsedSubscription["subscribed_id"].Value<string>();
                      
                        returnSubscriptions.Add(subscriptionModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing simsip feed: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing simsip feeds: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnSubscriptions;
        }

        #endregion

    }
}