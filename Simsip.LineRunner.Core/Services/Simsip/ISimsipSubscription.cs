using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simsip.LineRunner.Entities.Facebook;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Services.Simsip
{
    public interface ISimsipSubscriptionService
    {
        Task<FacebookSubscriptionEntity> AddSubscriptionAsync(FacebookSubscriptionEntity subscription);

        Task<IList<FacebookSubscriptionEntity>> GetSubscriptionsAsync(SimsipUserEntity simsipUser);

        Task<FacebookSubscriptionEntity> UpdateSubscriptionAsync(FacebookSubscriptionEntity subscription);

        Task DeleteSubscriptionAsync(FacebookSubscriptionEntity subscription);
    }
}
