using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Youtube;

namespace Simsip.LineRunner.Data.Youtube
{
    public class YoutubeCommentRepository : IYoutubeCommentRepository
    {
        public void Create(YoutubeCommentEntity comment)
        {
            // Make sure we have required object/fields
            if (comment == null)
            {
                throw new ArgumentException("YoutubeCommentEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(comment.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingComment = ReadByServerId(comment.ServerId);
            if (existingComment != null)
            {
                throw new InvalidOperationException("Cannot create duplicate comment. There is an existing comment with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(comment);
                        });
                }
            }
        }

        public YoutubeCommentEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<YoutubeCommentEntity>()
                                 .Where(x => x.ServerId == serverId)
                                 .FirstOrDefault<YoutubeCommentEntity>();

                    return result;
                }
            }
        }

        public List<YoutubeCommentEntity> ReadByForeignKey(string foreignKey, int skip=-1, int pageSize=-1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<YoutubeCommentEntity>()
                                  .Where(x => x.ForeignKey == foreignKey)
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

        public void Update(YoutubeCommentEntity comment)
        {
            // Make sure we have required object/fields
            if (comment == null)
            {
                throw new ArgumentException("YoutubeCommentEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(comment.ServerId))
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
                            connection.Update(comment);
                        });
                }
            }
        }

        public void Delete(YoutubeCommentEntity comment)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(comment);
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
                        return connection.Table<YoutubeCommentEntity>().Count();
                    }
                }
            }
        }
    }
}


