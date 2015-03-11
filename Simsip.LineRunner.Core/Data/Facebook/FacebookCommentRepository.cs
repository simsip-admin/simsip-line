using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookCommentRepository : IFacebookCommentRepository
    {
        public void Create(FacebookCommentEntity comment)
        {
            // Make sure we have required object/fields
            if (comment == null)
            {
                throw new ArgumentException("FacebookCommentEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(comment.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(comment.ForeignKey))
            {
                throw new ArgumentException("ForeignKey cannot be empty");
            }
            if (string.IsNullOrEmpty(comment.FromId))
            {
                throw new ArgumentException("FromId cannot be empty");
            }
            if (string.IsNullOrEmpty(comment.Message))
            {
                throw new ArgumentException("Message cannot be empty");
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

        public FacebookCommentEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<FacebookCommentEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<FacebookCommentEntity>();

                    return result;
                }
            }
        }

        public List<FacebookCommentEntity> ReadByForeignKey(string foreignKey, int skip=-1, int pageSize=-1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookCommentEntity>()
                                  .Where(x => x.ForeignKey == foreignKey)
                                  .OrderBy(x => x.CreatedTime);

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

        public void Update(FacebookCommentEntity comment)
        {
            // Make sure we have required object/fields
            if (comment == null)
            {
                throw new ArgumentException("FacebookCommentEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(comment.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(comment.ForeignKey))
            {
                throw new ArgumentException("ForeignKey cannot be empty");
            }
            if (string.IsNullOrEmpty(comment.FromId))
            {
                throw new ArgumentException("FromId cannot be empty");
            }
            if (string.IsNullOrEmpty(comment.Message))
            {
                throw new ArgumentException("Message cannot be empty");
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

        public void Delete(FacebookCommentEntity comment)
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
                        return connection.Table<FacebookCommentEntity>().Count();
                    }
                }
            }
        }
    }
}


