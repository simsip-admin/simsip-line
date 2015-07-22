using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simsip.LineRunner.Data.LineRunner
{
    public class PageObstaclesRepository : IPageObstaclesRepository
    {
        public List<PageObstaclesEntity> GetObstacles(string productId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<PageObstaclesEntity>()
                                  .Where(x => x.ProductId == productId)
                                  .OrderBy(x => x.PageNumber)
                                  .ThenBy(x => x.LineNumber)
                                  .ThenBy(x => x.ObstacleNumber);

                    return results.ToList();
                }
            }
        }

        public List<PageObstaclesEntity> GetObstacles(string productId, int pageNumber)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<PageObstaclesEntity>()
                                  .Where(x => x.ProductId == productId &&
                                              x.PageNumber == pageNumber)
                                  .OrderBy(x => x.LineNumber)
                                  .ThenBy(x => x.ObstacleNumber);

                    return results.ToList();
                }
            }
        }

        public List<PageObstaclesEntity> GetObstacles(string productId, int pageNumber, int lineNumber)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<PageObstaclesEntity>()
                                  .Where(x => x.ProductId == productId &&
                                              x.PageNumber == pageNumber &&
                                              x.LineNumber == lineNumber)
                                  .OrderBy(x => x.ObstacleNumber);

                    return results.ToList();
                }
            }
        }

        public List<PageObstaclesEntity> GetObstacles(string productId, int pageNumber, int[] lineNumbers)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var results = connection.Table<PageObstaclesEntity>()
                                  .Where(x => x.ProductId == productId &&
                                              x.PageNumber == pageNumber &&
                                              lineNumbers.Contains(x.LineNumber))
                                  .OrderBy(x => x.ObstacleNumber);

                    return results.ToList();
                }
            }
        }

        public PageObstaclesEntity GetObstacle(string productId, int pageNumber, int lineNumber, int obstacleNumber)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<PageObstaclesEntity>()
                                  .Where(x => x.ProductId == productId &&
                                              x.PageNumber == pageNumber &&
                                              x.LineNumber == lineNumber &&
                                              x.ObstacleNumber == obstacleNumber))
                                 .FirstOrDefault<PageObstaclesEntity>();

                    return result;
                }
            }
        }
    }
}


