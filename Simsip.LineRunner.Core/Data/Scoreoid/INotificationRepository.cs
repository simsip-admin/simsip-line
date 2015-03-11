using Simsip.LineRunner.Entities.Scoreoid;
using System.Collections.Generic;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public interface INotificationRepository
    {
        void Create(NotificationEntity notification);
        IList<NotificationEntity> GetNotifications();
        NotificationEntity GetNotification(int notificationId);
        void Update(NotificationEntity notification);
        void Delete(NotificationEntity notification);
        int Count { get; }
    }
}
