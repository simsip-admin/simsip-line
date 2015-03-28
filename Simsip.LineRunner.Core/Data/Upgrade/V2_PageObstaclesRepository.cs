using Simsip.LineRunner.Entities.Upgrade;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simsip.LineRunner.Data.Upgrade
{
    public class V2_PageObstaclesRepository : IV2_PageObstaclesRepository
    {
        public List<V2_PageObstaclesEntity> GetObstacles()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.UpgradePath()))
                {
                    var results = connection.Table<V2_PageObstaclesEntity>()
                                  .OrderBy(x => x.PageNumber)
                                  .ThenBy(x => x.LineNumber)
                                  .ThenBy(x => x.ObstacleNumber);

                    return results.ToList();
                }
            }
        }
    }
}


