using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Facebook
{
    public interface IFacebookSubscriptionService
    {
        #region Subscriptions

        Task<string> AddSubscriptionAsync(SimsipUserEntity simsipUser, 
                                          FacebookSubscriptionEntity album); 

        Task<IList<FacebookSubscriptionEntity>> GetSubscriptionsAsync(string userId); 

        Task<IList<FacebookHybridEntity>> GetSubscriptionHybridsAsync(IList<FacebookSubscriptionEntity> subscriptions);

        Task UpdateSubscriptionAsync(FacebookSubscriptionEntity album);

        Task DeleteSubscriptionAsync(FacebookSubscriptionEntity album);

        #endregion
    }
}
