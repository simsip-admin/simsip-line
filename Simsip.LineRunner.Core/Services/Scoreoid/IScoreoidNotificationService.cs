using Simsip.LineRunner.Entities.Scoreoid;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Simsip.LineRunner.Services.Scoreoid
{
    public interface IScoreoidNotificationService
    {
        Task<IList<NotificationEntity>> GetNotificationsAsync();
    }
}
