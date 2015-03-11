using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookVideoRepository : IFacebookVideoRepository
    {
        public void Create(FacebookVideoEntity video)
        {
            // Make sure we have required object/fields
            if (video == null)
            {
                throw new ArgumentException("FacebookVideoEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(video.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(video.FromId))
            {
                throw new ArgumentException("FromId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingVideo = GetVideoByServerId(video.ServerId);
            if (existingVideo != null)
            {
                throw new InvalidOperationException("Cannot create duplicate video. There is an existing video with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(video);
                        });
                }
            }
        }

        public IList<FacebookVideoEntity> GetVideos(int skip = -1, int pageSize = -1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookVideoEntity>()
                                  .OrderBy(x => x.UpdatedTime);

                    if (skip != -1)
                    {
                        results.Skip(skip);
                    }

                    if (pageSize != -1)
                    {
                        results.Take(pageSize);
                    }

                    return results.ToList();
                }
            }
        }

        public FacebookVideoEntity GetVideoByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<FacebookVideoEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<FacebookVideoEntity>();

                    return result;
                }
            }
        }

        public IList<FacebookVideoEntity> GetVideosByUser(FacebookUserEntity user, int skip=-1, int pageSize=-1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookVideoEntity>()
                                  .Where(x => x.FromId == user.ServerId)
                                  .OrderBy(x => x.UpdatedTime);

                    if (skip != -1)
                    {
                        results.Skip(skip);
                    }

                    if (pageSize != -1)
                    {
                        results.Take(pageSize);
                    }

                    return results.ToList();
                }
            }
        }

        public void Update(FacebookVideoEntity video)
        {
            // Make sure we have required object/fields
            if (video == null)
            {
                throw new ArgumentException("FacebookVideoEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(video.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(video.FromId))
            {
                throw new ArgumentException("FromId cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {

                            connection.Update(video);
                        });
                }
            }
        }

        public void Delete(FacebookVideoEntity video)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(video);
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
                        return connection.Table<FacebookVideoEntity>().Count();
                    }
                }
            }
        }
    }
}


