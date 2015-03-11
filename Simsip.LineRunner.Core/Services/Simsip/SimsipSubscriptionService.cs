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
using Simsip.LineRunner.Services.Rest;
using Newtonsoft.Json.Linq;


namespace Simsip.LineRunner.Services.Simsip
{
    public class SimsipSubscriptionService : ISimsipSubscriptionService
    {
        private const string TRACE_TAG = "SimsipSubscriptionService";

        private readonly IFacebookSubscriptionRepository _subscriptionRepository;
        
        public SimsipSubscriptionService(IFacebookSubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        #region ISimsipSubscriptionService Implementation

        public async Task<FacebookSubscriptionEntity> AddSubscriptionAsync(FacebookSubscriptionEntity subscription) 
        {
            FacebookSubscriptionEntity returnSubscription = null;

            try
            {
                /* TODO
                var subscriptionTable = App.MobileServiceClient.GetTable(MobileServiceConstants.SubscriptionTableName);

                var subscriptionJson = new JObject();
                subscriptionJson.Add("subscriber_id", subscription.SubscriberId);
                subscriptionJson.Add("subscribed_id", subscription.SubscribedId);

                var response = await subscriptionTable.InsertAsync(subscriptionJson);

                returnSubscription = ParseSubscriptionResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception adding simsip subscription: " + ex);
            }

            return returnSubscription;
        }

        public async Task<IList<FacebookSubscriptionEntity>> GetSubscriptionsAsync(SimsipUserEntity simsipUser) 
        {
            var returnSubscriptions = new List<FacebookSubscriptionEntity>();

            try
            {
                /* TODO
                var subscriptionTable = App.MobileServiceClient.GetTable(MobileServiceConstants.SubscriptionTableName);

                // TODO: Add in support for SimsipUser
                // var response = await subscriptionTable.ReadAsync("$filter=subscriber_id eq " + simsipUser.FacebookId);
                var response = await subscriptionTable.ReadAsync("$filter=subscriber_id eq " + "100005066855972");

                returnSubscriptions = ParseGetSubscriptionsResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception getting simsip subscriptions: " + ex);
            }

            return returnSubscriptions;
        }

        public async Task<FacebookSubscriptionEntity> UpdateSubscriptionAsync(FacebookSubscriptionEntity subscription) 
        {
            FacebookSubscriptionEntity returnSubscription = null;

            try
            {
                /* TODO
                var subscriptionTable = App.MobileServiceClient.GetTable(MobileServiceConstants.SubscriptionTableName);

                var subscriptionJson = new JObject();
                subscriptionJson.Add("id", subscription.ServerId);
                subscriptionJson.Add("subscriber_id", subscription.SubscriberId);
                subscriptionJson.Add("subscribed_id", subscription.SubscribedId);

                var response = await subscriptionTable.UpdateAsync(subscriptionJson);

                returnSubscription = ParseSubscriptionResponse(response);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception updating simsip subscription: " + ex);
            }

            return returnSubscription;
        }

        public async Task DeleteSubscriptionAsync(FacebookSubscriptionEntity subscription) 
        {
            try
            {
                /* TODO
                var subscriptionTable = App.MobileServiceClient.GetTable(MobileServiceConstants.SubscriptionTableName);

                var subscriptionJson = new JObject();
                subscriptionJson.Add("id", subscription.ServerId);

                await subscriptionTable.DeleteAsync(subscriptionJson);
                */
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception deleting simsip subscription: " + ex);
            }
        }

        #endregion

        #region Helper functions

        private FacebookSubscriptionEntity ParseSubscriptionResponse(JToken response)
        {
            FacebookSubscriptionEntity returnSubscription = null;

            try
            {
                returnSubscription = new FacebookSubscriptionEntity();

                // Get the simplistic values first
                returnSubscription.ServerId = response["id"].Value<string>();
                returnSubscription.SubscriberId = response["subscriber_id"].Value<string>();
                returnSubscription.SubscribedId = response["subscribed_id"].Value<string>();

                // Is this a new subscription?
                var subscriptionOnDevice = _subscriptionRepository.GetSubscriptionByServerId(returnSubscription.ServerId);
                if (subscriptionOnDevice == null)
                {
                    _subscriptionRepository.Create(returnSubscription);
                }
                else
                {
                    _subscriptionRepository.Update(returnSubscription);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing simsip subscription: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnSubscription;
        }

        private List<FacebookSubscriptionEntity> ParseGetSubscriptionsResponse(JToken response)
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
                        var subscriptionModel = ParseSubscriptionResponse(parsedSubscription);

                        returnSubscriptions.Add(subscriptionModel);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception parsing an individual simsip subscription: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception parsing simsip subscriptions: " + ex);

                // This one we throw, will get caught above and turned into async response
                throw ex;
            }

            return returnSubscriptions;
        }

        #endregion

    }
}