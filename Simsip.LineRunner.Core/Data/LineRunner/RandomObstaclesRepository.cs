using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simsip.LineRunner.Data.LineRunner
{
    public class RandomObstaclesRepository : IRandomObstaclesRepository
    {
        public List<RandomObstaclesEntity> GetAllRandomObstacles(string productId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<RandomObstaclesEntity>()
                                 .Where(x => x.ProductId == productId)
                                 .OrderBy(x => x.RandomObstaclesSet)
                                 .ThenBy(x => x.ObstacleNumber))
                                 .ToList<RandomObstaclesEntity>();

                    return result;
                }
            }
        }

        public List<RandomObstaclesEntity> GetRandomObstacleSet(string productId, string randomObstaclesSet)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<RandomObstaclesEntity>()
                                 .Where(x => x.ProductId == productId &&
                                             x.RandomObstaclesSet == randomObstaclesSet)
                                 .OrderBy(x => x.RandomObstaclesSet)
                                 .ThenBy(x => x.ObstacleNumber))
                                 .ToList<RandomObstaclesEntity>();

                    return result;
                }
            }
        }
    }
}

