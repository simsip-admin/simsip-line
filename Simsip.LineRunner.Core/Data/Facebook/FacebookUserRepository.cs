using System;
using System.Linq;
using SQLite;
using Simsip.LineRunner.Entities.Facebook;

namespace Simsip.LineRunner.Data.Facebook
{
    public class FacebookUserRepository : IFacebookUserRepository
    {
        public void Create(FacebookUserEntity user)
        {
            // Make sure we have required object/fields
            if (user == null)
            {
                throw new ArgumentException("FacebookUserEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(user.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            // Make sure this is not a duplicate
            var existingUser = ReadByServerId(user.ServerId);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Cannot create duplicate facebook user. There is an existing facebook user with this ServerId");
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

        public FacebookUserEntity ReadByServerId(string serverId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<FacebookUserEntity>()
                                 .Where(x => x.ServerId == serverId))
                                 .FirstOrDefault<FacebookUserEntity>();

                    return result;
                }
            }
        }

        public FacebookUserEntity ReadUser()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<FacebookUserEntity>()
                                  .FirstOrDefault();

                    return result;
                }
            }
        }

        public void Update(FacebookUserEntity user)
        {
            // Make sure we have required object/fields
            if (user == null)
            {
                throw new ArgumentException("FacebookUserEntity parameter cannot be null");
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

        public void Delete(FacebookUserEntity user)
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
                        return connection.Table<FacebookUserEntity>().Count();
                    }
                }
            }
        }
    }
}


