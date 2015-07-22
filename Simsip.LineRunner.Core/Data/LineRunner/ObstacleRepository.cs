using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System.Linq;

namespace Simsip.LineRunner.Data.LineRunner
{
    public class ObstacleRepository : IObstacleRepository
    {
        public ObstacleEntity GetObstacle(string productId, string modelName)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<ObstacleEntity>()
                                 .Where(x => x.ProductId == productId &&
                                             x.ModelName == modelName))
                                 .FirstOrDefault<ObstacleEntity>();

                    return result;
                }
            }
        }
    }
}


