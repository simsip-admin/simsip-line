using System;
using System.Linq;
using System.Collections.Generic;
using SQLite;
using Simsip.LineRunner.Entities.Simsip;

namespace Simsip.LineRunner.Data.Simsip
{
    public class SimsipUserRepository : ISimsipUserRepository
    {
        public void Create(SimsipUserEntity user)
        {
            // Make sure we have required object/fields
            if (user == null)
            {
                throw new ArgumentException("SimsipUserEntity parameter cannot be null");
            }
            if (string.IsNullOrEmpty(user.ServerId))
            {
                throw new ArgumentException("ServerId cannot be empty");
            }

            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            // Remove any previous records so we mainntain only one record
                            var existingModels = connection.Table<SimsipUserEntity>()
                                                 .ToList<SimsipUserEntity>();
                            foreach (var existingModel in existingModels)
                            {
                                connection.Delete(existingModel);
                            }

                            // Ok, good to go insert our one record
                            connection.Insert(user);
                        });
                }
            }
        }

        public SimsipUserEntity ReadUser()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    return connection.Table<SimsipUserEntity>()
                           .FirstOrDefault();
                }
            }
        }

        public void Update(SimsipUserEntity user)
        {
            // Make sure we have required object/fields
            if (user == null)
            {
                throw new ArgumentException("SimsipUserEntity parameter cannot be null");
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

        public void Delete(SimsipUserEntity user)
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
                        return connection.Table<SimsipUserEntity>().Count();
                    }
                }
            }
        }
    }
}


