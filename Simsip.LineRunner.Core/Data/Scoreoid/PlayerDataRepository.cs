using Simsip.LineRunner.Entities.Scoreoid;
using SQLite;
using System;
using System.Linq;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public class PlayerDataRepository : IPlayerDataRepository
    {
        public void Create(PlayerDataEntity playerData)
        {
            // Make sure we have required object/fields
            if (playerData == null)
            {
                throw new ArgumentException("Player data parameter cannot be null");
            }

            // Make sure this is not a duplicate
            var existingPlayerData = GetPlayerData(playerData.Key);
            if (existingPlayerData != null)
            {
                throw new InvalidOperationException("Cannot create duplicate player data. There is an existing player data with this key.");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(playerData);
                        });
                }
            }
        }

        public PlayerDataEntity GetPlayerData(string key)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<PlayerDataEntity>()
                                 .Where(x => x.Key == key))
                                 .FirstOrDefault<PlayerDataEntity>();

                    return result;
                }
            }
        }

        public void Update(PlayerDataEntity playerData)
        {
            // Make sure we have required object/fields
            if (playerData  == null)
            {
                throw new ArgumentException("Player data parameter cannot be null");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(playerData);
                        });
                }
            }
        }

        public void Delete(PlayerDataEntity playerData)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(playerData);
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
                        return connection.Table<PlayerDataEntity>().Count();
                    }
                }
            }
        }
    }
}

