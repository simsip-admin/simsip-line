using Simsip.LineRunner.Entities.Scoreoid;
using SQLite;
using System;
using System.Linq;


namespace Simsip.LineRunner.Data.Scoreoid
{
    public class PlayerRepository : IPlayerRepository
    {
        public void Create(PlayerEntity player)
        {
            // Make sure we have required object/fields
            if (player == null)
            {
                throw new ArgumentException("PlayerEntity parameter cannot be null");
            }

            // Make sure this is not a duplicate
            var existingPlayer = GetPlayer(player.UniqueId);
            if (existingPlayer != null)
            {
                throw new InvalidOperationException("Cannot create duplicate player. There is an existing player with this player ID.");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Insert(player);
                        });
                }
            }
        }

        public PlayerEntity GetPlayer()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<PlayerEntity>()
                                 .FirstOrDefault<PlayerEntity>();

                    return result;
                }
            }
        }

        public PlayerEntity GetPlayer(int playerId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<PlayerEntity>()
                                 .Where(x => x.UniqueId == playerId))
                                 .FirstOrDefault<PlayerEntity>();

                    return result;
                }
            }
        }

        public void Update(PlayerEntity player)
        {
            // Make sure we have required object/fields
            if (player == null)
            {
                throw new ArgumentException("Player parameter cannot be null");
            }

            // Good to go
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Update(player);
                        });
                }
            }
        }

        public void Delete(PlayerEntity player)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    connection.RunInTransaction(() =>
                        {
                            connection.Delete(player);
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
                        return connection.Table<PlayerEntity>().Count();
                    }
                }
            }
        }
    }
}

