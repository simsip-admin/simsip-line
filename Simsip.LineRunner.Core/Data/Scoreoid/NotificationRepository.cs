using Simsip.LineRunner.Entities.Scoreoid;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public class NotificationRepository : INotificationRepository
    {
        public void Create(NotificationEntity notification)
        {
            // Make sure we have required object/fields
            if (notification == null)
            {
                throw new ArgumentException("Notification parameter cannot be null");
            }

            // Make sure this is not a duplicate
            var existingNotification = GetNotification(notification.NotificationId);
            if (existingNotification != null)
            {
                throw new InvalidOperationException("Cannot create duplicate notification. There is an existing notification with this notification ID.");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(notification);
                        });
                }
            }
        }

        public IList<NotificationEntity> GetNotifications()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<NotificationEntity>()
                                 .OrderBy( x => x.NotificationId)
                                 .ToList<NotificationEntity>();

                    return result;
                }
            }
        }

        public NotificationEntity GetNotification(int notificationId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<NotificationEntity>()
                                 .Where(x => x.NotificationId == notificationId))
                                 .FirstOrDefault<NotificationEntity>();

                    return result;
                }
            }
        }

        public void Update(NotificationEntity notification)
        {
            // Make sure we have required object/fields
            if (notification == null)
            {
                throw new ArgumentException("Notification parameter cannot be null");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(notification);
                        });
                }
            }
        }

        public void Delete(NotificationEntity notification)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(notification);
                        });
                }
            }
        }

        public int Count
        {
            get
            {
                lock (Database.DATABASE_LOCK)
                {
                    using (var connection = new SQLiteConnection(Database.DatabasePath()))
                    {
                        return connection.Table<NotificationEntity>().Count();
                    }
                }
            }
        }
    }
}

