using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Youtube;

namespace Simsip.LineRunner.Data.Youtube
{
    public class YoutubeVideoRepository : IYoutubeVideoRepository
    {
        public void Create(YoutubeVideoEntity video)
        {
            // Make sure we have required object/fields
            if (video == null)
            {
                throw new ArgumentException("VideoModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(video.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingVideo = ReadByServerId(video.ServerId);
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

        public YoutubeVideoEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<YoutubeVideoEntity>()
                                 .Where(x => x.ServerId == serverId)
                                 .FirstOrDefault<YoutubeVideoEntity>();

                    return result;
                }
            }
        }

        public List<YoutubeVideoEntity> Read(int skip=-1, int pageSize=-1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<YoutubeVideoEntity>()
                                  .OrderBy(x => x.Updated);

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

        public void Update(YoutubeVideoEntity video)
        {
            // Make sure we have required object/fields
            if (video == null)
            {
                throw new ArgumentException("VideoModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(video.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
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

        public void Delete(YoutubeVideoEntity video)
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
                        return connection.Table<YoutubeVideoEntity>().Count();
                    }
                }
            }
        }
    }
}


