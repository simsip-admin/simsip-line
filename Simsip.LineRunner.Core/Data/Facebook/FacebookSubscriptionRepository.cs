using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookSubscriptionRepository : IFacebookSubscriptionRepository
    {
        public void Create(FacebookSubscriptionEntity subscription)
        {
            // Make sure we have required object/fields
            if (subscription == null)
            {
                throw new ArgumentException("FacebookSubscriptionEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(subscription.SubscriberId))
            {
                throw new ArgumentException("SubscriberId cannot be empty");
            }
            if (string.IsNullOrEmpty(subscription.SubscribedId))
            {
                throw new ArgumentException("SubscribedId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingSubscription = GetSubscription(subscription.SubscriberId, subscription.SubscribedId);
            if (existingSubscription != null)
            {
                throw new InvalidOperationException("Cannot create duplicate subscription. There is an existing subscription with this SubscriberId and SubscribedId.");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {

                            connection.Insert(subscription);
                        });
                }
            }
        }

        public FacebookSubscriptionEntity GetSubscription(string subscriberId, string subscribedId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<FacebookSubscriptionEntity>()
                                 .Where(x => x.SubscriberId == subscriberId &&
                                             x.SubscribedId == subscribedId)
                                 .FirstOrDefault<FacebookSubscriptionEntity>();

                    return result;
                }
            }
        }

        public FacebookSubscriptionEntity GetSubscriptionByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<FacebookSubscriptionEntity>()
                                 .Where(x => x.ServerId == serverId)
                                 .FirstOrDefault<FacebookSubscriptionEntity>();

                    return result;
                }
            }
        }

        public IList<FacebookHybridEntity> GetHybrids(int skip = -1, int pageSize = -1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {

                    // First get list of users we subscribe to
                    var table = connection.Table<FacebookSubscriptionEntity>() as IEnumerable<FacebookSubscriptionEntity>;   // TODO: Why do we need to do this?
                    List<string> subscriptionResults =
                        (from subscription in table
                         select subscription.SubscribedId)
                        .ToList<string>();

                    // Now pull albums we subscribe to
                    var albumResults =
                        (from album in connection.Table<FacebookAlbumEntity>()
                         where subscriptionResults.Contains(album.FromId)
                         orderby album.UpdatedTime
                         select new FacebookHybridEntity()
                             {
                                 DeviceId = album.DeviceId,
                                 ServerId = album.ServerId,
                                 Type = "album",
                                 FromId = album.FromId,
                                 FromName = album.FromName,
                                 Name = album.Name,
                                 Description = album.Description,
                                 CoverPhotoThumbnailUrl = album.CoverPhotoThumbnailUrl,
                                 Count = album.Count,
                                 UpdatedTime = album.UpdatedTime,
                                 IsUnseen = album.IsUnseen
                             });

                    // Next pull videos we subscribe to
                    var videoResults =
                        (from video in connection.Table<FacebookVideoEntity>()
                         where subscriptionResults.Contains(video.FromId)
                         orderby video.UpdatedTime
                         select new FacebookHybridEntity()
                             {
                                 DeviceId = video.DeviceId,
                                 ServerId = video.ServerId,
                                 Type = "video",
                                 FromId = video.FromId,
                                 FromName = video.FromName,
                                 Name = video.Name,
                                 Description = video.Description,
                                 CoverPhotoThumbnailUrl = video.Picture,
                                 Count = 0,
                                 UpdatedTime = video.UpdatedTime.ToString(), // TODO: Get consistent dates in place
                                 IsUnseen = video.IsUnseen
                             });

                    // Combine the results
                    var results = (albumResults.Union<FacebookHybridEntity>(videoResults))
                                  .OrderBy(x => x.UpdatedTime)
                                  .ToList<FacebookHybridEntity>();

                    // Extract and trim as requested
                    if (skip != -1)
                    {
                        results.Skip(skip);
                    }

                    if (pageSize != -1)
                    {
                        results.Take(pageSize);
                    }

                    return results;
                }
            }
        }

        public void Update(FacebookSubscriptionEntity subscription)
        {
            // Make sure we have required object/fields
            if (subscription == null)
            {
                throw new ArgumentException("FacebookSubscriptionEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(subscription.SubscriberId))
            {
                throw new ArgumentException("SubscriberId cannot be empty");
            }
            if (string.IsNullOrEmpty(subscription.SubscribedId))
            {
                throw new ArgumentException("SubscribedId cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(subscription);
                        });
                }
            }
        }

        public void Delete(FacebookSubscriptionEntity subscription)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(subscription);
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
                        return connection.Table<FacebookSubscriptionEntity>().Count();
                    }
                }
            }
        }
    }
}


