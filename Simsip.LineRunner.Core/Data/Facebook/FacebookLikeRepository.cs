using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookLikeRepository : IFacebookLikeRepository
    {
        public void Create(FacebookLikeEntity like)
        {
            // Make sure we have required object/fields
            if (like == null)
            {
                throw new ArgumentException("LikeModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(like.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(like.ForeignKey))
            {
                throw new ArgumentException("ForeignKey cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingLike = ReadByServerId(like.ServerId);
            if (existingLike != null)
            {
                throw new InvalidOperationException("Cannot create duplicate like. There is an existing like with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(like);
                        });
                }
            }
        }

        public FacebookLikeEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<FacebookLikeEntity>()
                                  .Where(x => x.ServerId == serverId))
                                  .FirstOrDefault<FacebookLikeEntity>();

                    return result;
                }
            }
        }

        public List<FacebookLikeEntity> ReadByForeignKey(string foreignKey, int skip=-1, int pageSize=-1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookLikeEntity>()
                                  .Where(x => x.ForeignKey == foreignKey);

                    if (skip != -1)
                    {
                        results.Skip(skip);
                    }

                    if (pageSize != -1)
                    {
                        results.Take(pageSize);
                    }

                    return results.ToList<FacebookLikeEntity>();
                }
            }
        }

        public void Update(FacebookLikeEntity like)
        {
            // Make sure we have required object/fields
            if (like == null)
            {
                throw new ArgumentException("LikeModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(like.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }
            if (string.IsNullOrEmpty(like.ForeignKey))
            {
                throw new ArgumentException("ForeignKey cannot be empty");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(like);
                        });
                }
            }

        }

        public void Delete(FacebookLikeEntity like)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(like);
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


