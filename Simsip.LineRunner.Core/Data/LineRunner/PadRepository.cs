using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace Simsip.LineRunner.Data.LineRunner
{
    public class PadRepository : IPadRepository
    {
        public IList<PadEntity> GetPads(string productId)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<PadEntity>()
                                 .Where(x => x.ProductId == productId)
                                 .ToList();

                    return result;
                }
            }
        }

        public PadEntity GetPad(string productId, string modelName)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<PadEntity>()
                                 .Where(x => x.ProductId == productId &&
                                             x.ModelName == modelName))
                                 .FirstOrDefault<PadEntity>();

                    return result;
                }
            }
        }
    }
}


