using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookGroupRepository : IFacebookGroupRepository
    {
        public void Create(FacebookGroupEntity group)
        {
            // Make sure we have required object/fields
            if (group == null)
            {
                throw new ArgumentException("FacebookGroupEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(group.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingGroup = GetGroupByServerId(group.ServerId);
            if (existingGroup != null)
            {
                throw new InvalidOperationException("Cannot create duplicate group. There is an existing group with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(group);
                        });
                }
            }
        }

        public IList<FacebookGroupEntity> GetGroups(int skip = -1, int pageSize = -1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookGroupEntity>()
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

        public FacebookGroupEntity GetGroupByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<FacebookGroupEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<FacebookGroupEntity>();

                    return result;
                }
            }
        }

        public void Update(FacebookGroupEntity group)
        {
            // Make sure we have required object/fields
            if (group == null)
            {
                throw new ArgumentException("FacebookGroupEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(group.ServerId))
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
                            connection.Update(group);
                        });
                }
            }
        }

        public void Delete(FacebookGroupEntity group)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(group);
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
                        return connection.Table<FacebookGroupEntity>().Count();
                    }
                }
            }
        }
    }
}


