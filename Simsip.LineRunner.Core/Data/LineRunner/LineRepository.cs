using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System.Collections.Generic;
using System.Linq;


namespace Simsip.LineRunner.Data.LineRunner
{
    public class LineRepository : ILineRepository
    {
        public LineEntity GetLine(string productId, string modelName)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<LineEntity>()
                                 .Where(x => x.ProductId == productId &&
                                             x.ModelName == modelName)
                                 .FirstOrDefault<LineEntity>();

                    return result;
                }
            }
        }

        public IList<LineEntity> GetLines(string productId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<LineEntity>()
                                 .Where(x => x.ProductId == productId)
                                 .ToList<LineEntity>();

                    return result;
                }
            }
        }

    }
}


