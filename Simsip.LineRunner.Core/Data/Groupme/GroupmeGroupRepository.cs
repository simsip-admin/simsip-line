using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Data.Groupme
{
    public class GroupmeGroupRepository : IGroupemeIGroupRepository
    {
        public void Create(GroupmeGroupEntity group)
        {
            // Make sure we have required object/fields
            if (group == null)
            {
                throw new ArgumentException("GroupModel parameter cannot be null");
            }
            if (string.IsNullOrEmpty(group.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingGroup = ReadByServerId(group.ServerId);
            if (existingGroup != null)
            {
                throw new InvalidOperationException("Cannot create duplicate group. There is an existing group with this ServerId");
            }

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

        public List<GroupmeGroupEntity> ReadInbox(int skip, int pageSize)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<GroupmeGroupEntity>()
                                  .OrderBy(x => x.UpdatedAt);

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

        public GroupmeGroupEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    return connection.Table<GroupmeGroupEntity>()
                           .Where(x => x.ServerId == serverId)
                           .FirstOrDefault();
                }
            }
        }

        public void Update(GroupmeGroupEntity group)
        {
            // Make sure we have required object/fields
            if (group == null)
            {
                throw new ArgumentException("GroupModel parameter cannot be null");
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

        public void Delete(GroupmeGroupEntity group)
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
                        return connection.Table<GroupmeGroupEntity>().Count();
                    }
                }

            }
        }
    }
}


