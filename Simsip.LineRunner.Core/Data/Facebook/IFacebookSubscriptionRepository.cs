using System.Collections.Generic;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public interface IFacebookSubscriptionRepository
    {
        void Create(FacebookSubscriptionEntity subscription);
        FacebookSubscriptionEntity GetSubscription(string subscriberId, string subscribedId);
        FacebookSubscriptionEntity GetSubscriptionByServerId(string serverId);
        IList<FacebookHybridEntity> GetHybrids(int skip, int pageSize);
        void Update(FacebookSubscriptionEntity video);
        void Delete(FacebookSubscriptionEntity video);
        int Count { get; }
    }
}
