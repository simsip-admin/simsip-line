using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace Simsip.LineRunner.Data.LineRunner
{
    public class ResourcePackRepository : IResourcePackRepository
    {
        public IList<ResourcePackEntity> GetResourcePacks()
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<ResourcePackEntity>()
                                 .ToList();

                    return result;
                }
            }
        }

        public ResourcePackEntity GetResourcePack(string resourcePackName)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<ResourcePackEntity>()
                                 .Where(x => x.ResourcePackName == resourcePackName))
                                 .FirstOrDefault<ResourcePackEntity>();

                    return result;
                }
            }
        }
    }
}


