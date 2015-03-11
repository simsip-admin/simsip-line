using Simsip.LineRunner.Entities.Scoreoid;
using SQLite;
using System;
using System.Linq;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public class GameRepository : IGameRepository
    {
        public void Create(GameEntity game)
        {
            // Make sure we have required object/fields
            if (game == null)
            {
                throw new ArgumentException("Game parameter cannot be null");
            }

            // Make sure this is not a duplicate
            var existingGame = GetGame(game.GameId);
            if (existingGame != null)
            {
                throw new InvalidOperationException("Cannot create duplicate game. There is an existing game with this game ID.");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(game);
                        });
                }
            }
        }

        public GameEntity GetGame(string gameId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<GameEntity>()
                                 .Where(x => x.GameId == gameId))
                                 .FirstOrDefault<GameEntity>();

                    return result;
                }
            }
        }

        public void Update(GameEntity game)
        {
            // Make sure we have required object/fields
            if (game == null)
            {
                throw new ArgumentException("Game parameter cannot be null");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(game);
                        });
                }
            }
        }

        public void Delete(GameEntity game)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(game);
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
                        return connection.Table<GameEntity>().Count();
                    }
                }
            }
        }
    }
}

