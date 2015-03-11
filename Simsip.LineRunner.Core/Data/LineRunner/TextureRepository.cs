using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System.Linq;
using System.Collections.Generic;


namespace Simsip.LineRunner.Data.LineRunner
{
    public class TextureRepository : ITextureRepository
    {
        public IList<TextureEntity> GetTextures(string modelName)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = connection.Table<TextureEntity>()
                                 .Where(x => x.ModelName == modelName)
                                 .OrderBy(x => x.TexturePosition)
                                 .ToList();

                    return result;
                }
            }
        }
    }
}


