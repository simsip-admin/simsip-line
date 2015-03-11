using System;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Groupme;

namespace Simsip.LineRunner.Data.Groupme
{
    public class GroupmeUserRepository : IGroupmeUserRepository
    {
        public void Create(GroupmeUserEntity user)
        {
            // Make sure we have required object/fields
            if (user == null)
            {
                throw new ArgumentException("GroupmeUserEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(user.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingUser = ReadByServerId(user.ServerId);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Cannot create duplicate groupme user. There is an existing groupme user with this ServerId");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(user);
                        });
                }
            }
        }

        public GroupmeUserEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<GroupmeUserEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<GroupmeUserEntity>();

                    return result;
                }
            }
        }

        public GroupmeUserEntity ReadUser()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    return connection.Table<GroupmeUserEntity>()
                           .FirstOrDefault();
                }
            }
        }

        public void Update(GroupmeUserEntity user)
        {
            // Make sure we have required object/fields
            if (user == null)
            {
                throw new ArgumentException("GroupmeUserEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(user.ServerId))
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
                            connection.Update(user);
                        });
                }
            }
        }

        public void Delete(GroupmeUserEntity user)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(user);
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
                        return connection.Table<GroupmeUserEntity>().Count();
                    }
                }
            }
        }
    }
}


