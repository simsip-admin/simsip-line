using Simsip.LineRunner.Entities.LineRunner;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simsip.LineRunner.Data.LineRunner
{
    public class CharacterRepository : ICharacterRepository
    {
        public CharacterEntity GetCharacter(string productId, string modelName)
        {
            lock (Database.DATABASE_LOCK)
            {
                using (var connection = new SQLiteConnection(Database.DatabasePath()))
                {
                    var result = (connection.Table<CharacterEntity>()
                                 .Where(x => x.ProductId == productId &&
                                             x.ModelName == modelName))
                                 .FirstOrDefault<CharacterEntity>();

                    return result;
                }
            }
        }
    }
}


