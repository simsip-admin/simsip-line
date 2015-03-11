using Simsip.LineRunner.Entities.Scoreoid;
using SQLite;
using System;
using System.Linq;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public class GameDataRepository : IGameDataRepository
    {
        public void Create(GameDataEntity gameData)
        {
            // Make sure we have required object/fields
            if (gameData == null)
            {
                throw new ArgumentException("Game data parameter cannot be null");
            }

            // Make sure this is not a duplicate
            var existingGameData = GetGameData(gameData.Key);
            if (existingGameData != null)
            {
                throw new InvalidOperationException("Cannot create duplicate game data. There is an existing game data with this key.");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(gameData);
                        });
                }
            }
        }

        public GameDataEntity GetGameData(string key)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<GameDataEntity>()
                                 .Where(x => x.Key == key))
                                 .FirstOrDefault<GameDataEntity>();

                    return result;
                }
            }
        }

        public void Update(GameDataEntity gameData)
        {
            // Make sure we have required object/fields
            if (gameData == null)
            {
                throw new ArgumentException("Game data parameter cannot be null");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(gameData);
                        });
                }
            }
        }

        public void Delete(GameDataEntity gameData)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(gameData);
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
                        return connection.Table<GameDataEntity>().Count();
                    }
                }
            }
        }
    }
}

