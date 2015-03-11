using System;
using System.Collections.Generic;
using SQLite;
using System.Linq;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookActionRepository : IFacebookActionRepository
    {
        public void Create(FacebookActionEntity action)
        {
            // Make sure we have required object/fields
            if (action == null)
            {
                throw new ArgumentException("FacebookActionEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(action.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingAction = ReadByServerId(action.ServerId);
            if (existingAction != null)
            {
                throw new InvalidOperationException("Cannot create duplicate action. There is an existing action with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(action);
                        });
                }
            }
        }

        public IList<FacebookActionEntity> Read(int skip = -1, int pageSize = -1)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<FacebookActionEntity>()
                                    .OrderBy(x => x.StartTime);

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

        public FacebookActionEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<FacebookActionEntity>()
                                    .Where(x => x.ServerId == serverId))
                                    .FirstOrDefault<FacebookActionEntity>();

                    return result;
                }
            }
        }

        public void Update(FacebookActionEntity action)
        {
            // Make sure we have required object/fields
            if (action == null)
            {
                throw new ArgumentException("FacebookActionEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(action.ServerId))
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
                            connection.Update(action);
                        });
                }
            }
        }

        public void Delete(FacebookActionEntity action)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(action);
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
                        return connection.Table<FacebookActionEntity>().Count();
                    }
                }
            }
        }
    }
}


