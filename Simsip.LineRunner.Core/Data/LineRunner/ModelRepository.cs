using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System;
using System.Linq;

namespace Simsip.LineRunner.Data.LineRunner
{
    public class ModelRepository : IModelRepository
    {
        public ModelEntity GetModel(string modelName)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<ModelEntity>()
                                 .Where(x => x.ModelName == modelName))
                                 .FirstOrDefault<ModelEntity>();

                    return result;
                }
            }
        }
    }
}

